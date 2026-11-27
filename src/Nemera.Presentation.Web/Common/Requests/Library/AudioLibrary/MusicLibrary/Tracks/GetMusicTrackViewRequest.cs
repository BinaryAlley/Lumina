#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.Requests.Library.AudioLibrary.MusicLibrary.Tracks;

/// <summary>
/// Represents a request for displaying the details of a track of an album of a music artist.
/// </summary>
/// <param name="ArtistId">The unique identifier of the artist the album belongs to, taken from the route. Required.</param>
/// <param name="AlbumId">The unique identifier of the album the track belongs to, taken from the route. Required.</param>
/// <param name="TrackId">The unique identifier of the track whose details are displayed, taken from the route. Required.</param>
/// <param name="LibraryId">The Id of the media library the track belongs to. Required.</param>
[DebuggerDisplay("TrackId: {TrackId}")]
public record GetMusicTrackViewRequest(
    string ArtistId,
    string AlbumId,
    string TrackId,
    Guid LibraryId
);
