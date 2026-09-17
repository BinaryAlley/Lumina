#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;

/// <summary>
/// Represents a request to delete a track by the specified Id.
/// </summary>
/// <param name="ArtistId">The unique identifier of the artist the album of the track belongs to, taken from the route.</param>
/// <param name="AlbumId">The unique identifier of the album the track belongs to, taken from the route.</param>
/// <param name="TrackId">The unique identifier of the track to delete.</param>
[DebuggerDisplay("TrackId: {TrackId}")]
public record DeleteTrackRequest(
    Guid ArtistId,
    Guid AlbumId,
    Guid TrackId
);
