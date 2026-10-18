#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Playback;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Playback;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
#endregion

namespace Lumina.Infrastructure.Core.MediaLibrary.AudioLibrary.MusicLibrary.Playback;

/// <summary>
/// Issues and resolves the opaque delivery handles of the playback manifest, which bind a normalized delivery to the user, the track and the inputs the delivery was declared with.
/// </summary>
/// <remarks>
/// The handles are process local, like the normalized audio cache they address. An API process must therefore keep handling both the manifest and
/// the segments of a track for a handle to resolve, which is the deployment the cache already requires: a single API process per cache directory.
/// A handle expires after a bounded lifetime, and the registry is bounded both globally and per user. Expired entries are removed from a queue in
/// issue order, so the sweep is amortized constant time and does not scan every handle. A repeated manifest for the same delivery inputs reuses the
/// handle that already exists for them, so reloading a manifest does not grow the registry.
/// </remarks>
internal sealed class PlaybackDeliveryHandleService : IPlaybackDeliveryHandleService
{
    private const int MAX_DELIVERIES = 20_000;
    private const int MAX_DELIVERIES_PER_USER = 64;
    private static readonly TimeSpan s_deliveryLifetime = TimeSpan.FromHours(12);

    private readonly Lock _deliveryLock = new();
    // These dictionaries share one logical invariant: a delivery handle is split across the delivery map, its signature map, its per-user queue and count,
    // and the global expiry queue. ConcurrentDictionary would make each collection thread-safe on its own, but not the multi-step issue, reuse, evict and remove sequences,
    // which must see all of them consistently. The Lock is held only for short, O(1), non-awaiting work, so replacing these collections with concurrent ones would add
    // complexity without removing the lock. Do not "fix" this without redesigning the state into one atomically updated map.
    private readonly Dictionary<string, StoredDelivery> _deliveries = [];
    private readonly Dictionary<string, string> _deliveryIdsBySignature = [];
    private readonly Dictionary<Guid, Queue<string>> _deliveryIdsByUser = [];
    private readonly Dictionary<Guid, int> _deliveryCountsByUser = [];
    private readonly Queue<(string DeliveryId, DateTimeOffset ExpiresAtUtc)> _expiryQueue = [];

    /// <summary>
    /// Issues an opaque handle for a delivery, which the playback manifest reports so its segments can be addressed by that handle.
    /// </summary>
    /// <param name="delivery">The delivery, its source file identity, its quality cap and its user.</param>
    /// <returns>The opaque delivery handle.</returns>
    public string IssueDeliveryId(PlaybackDeliveryHandleDto delivery)
    {
        string signature = BuildSignature(delivery);
        lock (_deliveryLock)
        {
            DateTimeOffset now = DateTimeOffset.UtcNow;
            RemoveExpiredDeliveries(now);
            if (_deliveryIdsBySignature.TryGetValue(signature, out string? existingDeliveryId)
                && _deliveries.TryGetValue(existingDeliveryId, out StoredDelivery? existingDelivery)
                && existingDelivery.ExpiresAtUtc > now)
                return existingDeliveryId;
            if (existingDeliveryId is not null)
                RemoveDelivery(existingDeliveryId);

            EnforceUserLimit(delivery.Request.UserId);
            EnforceGlobalLimit();

            // The handle is a random opaque value, not a derived one, so it cannot be computed from the track or the file by a caller.
            string deliveryId = Guid.NewGuid().ToString("N");
            DateTimeOffset expiresAtUtc = now.Add(s_deliveryLifetime);
            _deliveries[deliveryId] = new StoredDelivery(delivery, signature, expiresAtUtc);
            _deliveryIdsBySignature[signature] = deliveryId;
            _deliveryCountsByUser[delivery.Request.UserId] = _deliveryCountsByUser.GetValueOrDefault(delivery.Request.UserId) + 1;
            if (!_deliveryIdsByUser.TryGetValue(delivery.Request.UserId, out Queue<string>? userDeliveryIds))
            {
                userDeliveryIds = [];
                _deliveryIdsByUser[delivery.Request.UserId] = userDeliveryIds;
            }
            userDeliveryIds.Enqueue(deliveryId);
            _expiryQueue.Enqueue((deliveryId, expiresAtUtc));
            return deliveryId;
        }
    }

    /// <summary>
    /// Resolves the delivery a handle addresses, after verifying that it was issued to the provided user and describes the provided track.
    /// </summary>
    /// <param name="deliveryId">The opaque delivery handle reported by the playback manifest.</param>
    /// <param name="userId">The Id of the current user, which the handle must have been issued to.</param>
    /// <param name="trackId">The Id of the track named by the route, which the handle must describe.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either the delivery, or <see cref="Errors.Playback.PlaybackDeliveryNotFound"/> when the handle
    /// is unknown, expired, or belongs to another user or track.
    /// </returns>
    public Result<PlaybackDeliveryHandleDto> GetDelivery(string? deliveryId, Guid userId, Guid trackId)
    {
        if (string.IsNullOrWhiteSpace(deliveryId))
            return Errors.Playback.PlaybackDeliveryNotFound;

        lock (_deliveryLock)
        {
            if (!_deliveries.TryGetValue(deliveryId, out StoredDelivery? entry))
                return Errors.Playback.PlaybackDeliveryNotFound;
            if (entry.ExpiresAtUtc <= DateTimeOffset.UtcNow)
            {
                RemoveDelivery(deliveryId);
                return Errors.Playback.PlaybackDeliveryNotFound;
            }
            // A handle is not a capability: it is only valid for the user it was issued to and the track it describes, so a leaked handle
            // cannot be used to read another user's track or another track of the same user.
            if (entry.Delivery.Request.UserId != userId || entry.Delivery.Request.TrackId != trackId)
                return Errors.Playback.PlaybackDeliveryNotFound;
            return entry.Delivery;
        }
    }

    /// <summary>
    /// Enforces the per-user handle limit by evicting the oldest handles of that user.
    /// </summary>
    /// <param name="userId">The Id of the user whose handles are bounded.</param>
    private void EnforceUserLimit(Guid userId)
    {
        while (_deliveryCountsByUser.GetValueOrDefault(userId) >= MAX_DELIVERIES_PER_USER)
            if (!TryEvictOldestForUser(userId))
                break;
    }

    /// <summary>
    /// Enforces the global handle limit by evicting the oldest handle.
    /// </summary>
    private void EnforceGlobalLimit()
    {
        while (_deliveries.Count >= MAX_DELIVERIES)
            if (!TryEvictOldestDelivery())
                break;
    }

    /// <summary>
    /// Evicts the oldest handle that is still registered for a user.
    /// </summary>
    /// <param name="userId">The Id of the user whose oldest handle is evicted.</param>
    /// <returns><see langword="true"/> when a handle was evicted, otherwise <see langword="false"/>.</returns>
    private bool TryEvictOldestForUser(Guid userId)
    {
        if (!_deliveryIdsByUser.TryGetValue(userId, out Queue<string>? userDeliveryIds))
            return false;
        while (userDeliveryIds.Count > 0)
        {
            string deliveryId = userDeliveryIds.Dequeue();
            if (!_deliveries.ContainsKey(deliveryId))
                continue;
            RemoveDelivery(deliveryId);
            return true;
        }
        _deliveryIdsByUser.Remove(userId);
        return false;
    }

    /// <summary>
    /// Evicts the oldest handle that is still registered, using the expiry queue as the age order.
    /// </summary>
    /// <returns><see langword="true"/> when a handle was evicted, otherwise <see langword="false"/>.</returns>
    private bool TryEvictOldestDelivery()
    {
        while (_expiryQueue.Count > 0)
        {
            (string deliveryId, DateTimeOffset expiresAtUtc) = _expiryQueue.Dequeue();
            if (!_deliveries.TryGetValue(deliveryId, out StoredDelivery? storedDelivery) || storedDelivery.ExpiresAtUtc != expiresAtUtc)
                continue;
            RemoveDelivery(deliveryId);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Removes the deliveries whose lifetime ended, in expiry order, so the sweep does not scan the registry.
    /// </summary>
    /// <param name="now">The current time, in UTC.</param>
    private void RemoveExpiredDeliveries(DateTimeOffset now)
    {
        while (_expiryQueue.Count > 0)
        {
            (string deliveryId, DateTimeOffset expiresAtUtc) = _expiryQueue.Peek();
            if (expiresAtUtc > now)
                return;
            _expiryQueue.Dequeue();
            if (_deliveries.TryGetValue(deliveryId, out StoredDelivery? storedDelivery) && storedDelivery.ExpiresAtUtc == expiresAtUtc)
                RemoveDelivery(deliveryId);
        }
    }

    /// <summary>
    /// Removes one delivery and its signature and per-user accounting.
    /// </summary>
    /// <param name="deliveryId">The opaque handle of the delivery to remove.</param>
    private void RemoveDelivery(string deliveryId)
    {
        if (!_deliveries.Remove(deliveryId, out StoredDelivery? storedDelivery))
            return;
        if (_deliveryIdsBySignature.TryGetValue(storedDelivery.Signature, out string? mappedDeliveryId) && mappedDeliveryId == deliveryId)
            _deliveryIdsBySignature.Remove(storedDelivery.Signature);
        if (_deliveryCountsByUser.TryGetValue(storedDelivery.Delivery.Request.UserId, out int count))
        {
            if (count <= 1)
                _deliveryCountsByUser.Remove(storedDelivery.Delivery.Request.UserId);
            else
                _deliveryCountsByUser[storedDelivery.Delivery.Request.UserId] = count - 1;
        }
    }

    /// <summary>
    /// Builds the key that identifies a delivery for de-duplication, from every input that changes the handle's meaning.
    /// </summary>
    /// <param name="delivery">The delivery to key.</param>
    /// <returns>The signature of the delivery.</returns>
    private static string BuildSignature(PlaybackDeliveryHandleDto delivery)
    {
        return string.Join('\u001F', new string[]
        {
            delivery.Request.UserId.ToString("N"),
            delivery.Request.TrackId.ToString("N"),
            delivery.LibraryId.ToString("N"),
            delivery.AlbumId.ToString("N"),
            delivery.ArtistId.ToString("N"),
            ((int)delivery.Request.Mode).ToString(CultureInfo.InvariantCulture),
            ((int)delivery.Shape).ToString(CultureInfo.InvariantCulture),
            delivery.Request.SourceContainer ?? string.Empty,
            delivery.Request.Container,
            delivery.Request.Codec,
            delivery.Request.SegmentDurationSeconds.ToString(CultureInfo.InvariantCulture),
            delivery.Request.DurationInSeconds.ToString(CultureInfo.InvariantCulture),
            delivery.Request.Bitrate?.ToString(CultureInfo.InvariantCulture) ?? string.Empty,
            delivery.Request.SampleRate.ToString(CultureInfo.InvariantCulture),
            delivery.Request.Channels.ToString(CultureInfo.InvariantCulture),
            delivery.Request.BitDepth?.ToString(CultureInfo.InvariantCulture) ?? string.Empty,
            ((int)delivery.Request.Quality).ToString(CultureInfo.InvariantCulture),
            delivery.Request.SourcePath,
            delivery.Request.SourceLength.ToString(CultureInfo.InvariantCulture),
            delivery.Request.SourceLastModifiedUtc.Ticks.ToString(CultureInfo.InvariantCulture),
            string.Join('\u001E', delivery.Request.AllowedRoots)
        });
    }

    /// <summary>
    /// A registered delivery, its de-duplication signature and its expiry.
    /// </summary>
    /// <param name="Delivery">The delivery the handle addresses.</param>
    /// <param name="Signature">The signature of every input that changes the handle's meaning.</param>
    /// <param name="ExpiresAtUtc">The time, in UTC, after which the handle is no longer accepted.</param>
    private sealed record StoredDelivery(
        PlaybackDeliveryHandleDto Delivery,
        string Signature,
        DateTimeOffset ExpiresAtUtc
    );
}
