#region ========================================================================= USING =====================================================================================
using Lumina.Infrastructure.Common.Models.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Playback;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Infrastructure.Core.MediaLibrary.AudioLibrary.MusicLibrary.Playback;

/// <summary>
/// Represents the capacity a running normalization job reserved in the normalized audio cache, until it either produces an entry or is aborted.
/// </summary>
[DebuggerDisplay("Id: {Id}")]
internal sealed class TranscodingCacheReservation : IDisposable
{
    private readonly TranscodingCacheCoordinator _coordinator;
    private bool _isCompleted;
    private bool _isDisposed;

    /// <summary>
    /// Gets the identifier of the reservation.
    /// </summary>
    internal string Id { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TranscodingCacheReservation"/> class.
    /// </summary>
    /// <param name="coordinator">The coordinator that accounts for the reservation.</param>
    /// <param name="id">The identifier of the reservation.</param>
    internal TranscodingCacheReservation(TranscodingCacheCoordinator coordinator, string id)
    {
        _coordinator = coordinator;
        Id = id;
    }

    /// <summary>
    /// Updates the reservation to the number of bytes the job has actually written, growing it when the output is larger than the estimate.
    /// </summary>
    /// <param name="actualBytes">The number of bytes the job has actually written, in bytes.</param>
    /// <param name="maxCacheBytes">The maximum total size of the cache, in bytes.</param>
    /// <param name="gracePeriod">The period during which a recently produced or served entry is protected from eviction.</param>
    /// <returns><see langword="true"/> when the reservation covers the actual size, otherwise <see langword="false"/>.</returns>
    public bool TryUpdate(long actualBytes, long maxCacheBytes, TimeSpan gracePeriod)
    {
        if (_isCompleted || _isDisposed)
            return false;
        return _coordinator.TryGrow(Id, actualBytes, maxCacheBytes, gracePeriod);
    }

    /// <summary>
    /// Replaces the reservation with the produced cache entry.
    /// </summary>
    /// <param name="cacheId">The identifier of the produced cache entry.</param>
    /// <param name="temporaryPath">The temporary directory that holds the complete produced entry.</param>
    /// <param name="size">The total size of the produced cache entry, in bytes.</param>
    /// <param name="index">The validated index of the produced cache entry.</param>
    /// <param name="maxCacheBytes">The maximum total size of the cache, in bytes.</param>
    /// <param name="gracePeriod">The period during which a recently produced or served entry is protected from eviction.</param>
    /// <param name="isKeptOverCapacity"><see langword="true"/> when the produced entry remains readable even though it exceeds the cache limit, otherwise <see langword="false"/>.</param>
    /// <param name="isEntryInUse"><see langword="true"/> when a reader still holds the previous entry of the same cache identifier, so the replacement was not attempted.</param>
    /// <returns><see langword="true"/> when the produced entry is readable and may be served, otherwise <see langword="false"/> when it was not published.</returns>
    public bool TryComplete(string cacheId, string temporaryPath, long size, NormalizedTrackIndexDto index, long maxCacheBytes, TimeSpan gracePeriod, out bool isKeptOverCapacity, out bool isEntryInUse)
    {
        isKeptOverCapacity = false;
        isEntryInUse = false;
        if (_isCompleted || _isDisposed)
            return false;
        bool isCompleted = _coordinator.TryPublish(Id, cacheId, temporaryPath, size, index, maxCacheBytes, gracePeriod, out isKeptOverCapacity, out isEntryInUse);
        // The reservation is consumed even when the produced entry had to be removed, so it is never released a second time.
        _isCompleted = true;
        return isCompleted;
    }

    /// <summary>
    /// Releases the reservation when the job did not produce a cache entry.
    /// </summary>
    public void Dispose()
    {
        if (_isDisposed)
            return;
        _isDisposed = true;
        if (!_isCompleted)
            _coordinator.ReleaseReservation(Id);
    }
}
