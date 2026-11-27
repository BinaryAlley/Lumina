#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.Requests.Library.AudioLibrary.MusicLibrary.Tracks;

/// <summary>
/// Represents a request to get the playback manifest of a track.
/// </summary>
/// <param name="ArtistId">The unique identifier of the artist the album belongs to, taken from the route. Required.</param>
/// <param name="AlbumId">The unique identifier of the album the track belongs to, taken from the route. Required.</param>
/// <param name="TrackId">The unique identifier of the track whose manifest is requested, taken from the route. Required.</param>
/// <param name="LibraryId">The Id of the media library the track belongs to. Required.</param>
/// <param name="Capabilities">The comma separated list of the container and codec pairs the player advertises it can play directly. Optional.</param>
[DebuggerDisplay("TrackId: {TrackId}")]
public record GetPlaybackManifestRequest(
    string ArtistId,
    string AlbumId,
    string TrackId,
    Guid LibraryId,
    string? Capabilities
);
