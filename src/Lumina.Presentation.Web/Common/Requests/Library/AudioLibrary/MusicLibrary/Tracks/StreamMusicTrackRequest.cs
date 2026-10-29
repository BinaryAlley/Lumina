#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.Requests.Library.AudioLibrary.MusicLibrary.Tracks;

/// <summary>
/// Represents a request to stream the audio file of a track.
/// </summary>
/// <param name="ArtistId">The unique identifier of the artist the album belongs to, taken from the route. Required.</param>
/// <param name="AlbumId">The unique identifier of the album the track belongs to, taken from the route. Required.</param>
/// <param name="TrackId">The unique identifier of the track to stream, taken from the route. Required.</param>
/// <param name="LibraryId">The Id of the media library the track belongs to. Required.</param>
[DebuggerDisplay("TrackId: {TrackId}")]
public record StreamMusicTrackRequest(
    string ArtistId,
    string AlbumId,
    string TrackId,
    Guid LibraryId
);
