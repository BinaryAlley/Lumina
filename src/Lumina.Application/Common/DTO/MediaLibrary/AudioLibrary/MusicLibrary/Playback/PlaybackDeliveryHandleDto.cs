#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.Playback;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Playback;

/// <summary>
/// Data transfer object for the delivery a playback manifest issued to a user. It composes the exact normalization request the delivery declares,
/// so a new production input is added in one place and can never drift between the cache identity and the handle.
/// </summary>
/// <param name="Request">The complete normalization request the delivery declares, including its user, track, decision, quality and source identity.</param>
/// <param name="Shape">The shape the delivered audio is addressed by, which must be time addressed segments for a segment route.</param>
/// <param name="LibraryId">The Id of the library the track belongs to, so the segment route can scope the delivery without querying it again.</param>
/// <param name="AlbumId">The Id of the album the track belongs to, so the segment route can scope the delivery without querying it again.</param>
/// <param name="ArtistId">The Id of the artist that owns the album, so the segment route can scope the delivery without querying it again.</param>
[DebuggerDisplay("TrackId: {Request.TrackId} Mode: {Request.Mode} Container: {Request.Container} Codec: {Request.Codec}")]
public sealed record PlaybackDeliveryHandleDto(
    NormalizedTrackRequestDto Request,
    PlaybackDeliveryShape Shape,
    Guid LibraryId,
    Guid AlbumId,
    Guid ArtistId
);
