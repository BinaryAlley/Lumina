#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Playback;
using Lumina.Domain.Common.Primitives;
using System;
using Lumina.Domain.Common.Errors;
#endregion

namespace Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Playback;

/// <summary>
/// Interface for the service that issues and resolves the opaque delivery handles of the playback manifest.
/// </summary>
/// <remarks>
/// A delivery handle binds the delivery decision, the quality cap, the source file identity and the user to one opaque value, so the segment route
/// never recomputes them from inputs that can change while the track plays. A handle is only accepted from the user it was issued to, and only for
/// the track it describes, so it is not a capability that can be transferred to another user or track. Its format is exactly 32 lowercase
/// hexadecimal characters, the <c>N</c> format of a <see cref="Guid"/>, and that format is the only rule every producer and consumer applies: a value
/// that does not match it is rejected, never normalized into another handle.
/// </remarks>
public interface IPlaybackDeliveryHandleService
{
    /// <summary>
    /// Issues an opaque handle for a delivery, which the playback manifest reports so its segments can be addressed by that handle.
    /// </summary>
    /// <param name="delivery">The delivery, its source file identity, its quality cap and its user.</param>
    /// <returns>The opaque delivery handle.</returns>
    string IssueDeliveryId(PlaybackDeliveryHandleDto delivery);

    /// <summary>
    /// Resolves the delivery a handle addresses, after verifying that it was issued to the provided user and describes the provided track.
    /// </summary>
    /// <param name="deliveryId">The opaque delivery handle reported by the playback manifest.</param>
    /// <param name="userId">The Id of the current user, which the handle must have been issued to.</param>
    /// <param name="trackId">The Id of the track named by the route, which the handle must describe.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either the delivery, or <see cref="Errors.Playback.PlaybackDeliveryNotFound"/> when the handle is unknown, expired, or belongs to another user or track.
    /// </returns>
    Result<PlaybackDeliveryHandleDto> GetDelivery(string? deliveryId, Guid userId, Guid trackId);
}
