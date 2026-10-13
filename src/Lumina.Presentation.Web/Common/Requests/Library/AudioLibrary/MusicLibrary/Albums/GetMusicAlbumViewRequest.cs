#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.Requests.Library.AudioLibrary.MusicLibrary.Albums;

/// <summary>
/// Represents a request for displaying the details of an album of a music artist.
/// </summary>
/// <param name="ArtistId">The unique identifier of the artist the album belongs to, taken from the route. Required.</param>
/// <param name="AlbumId">The unique identifier of the album whose details are displayed, taken from the route. Required.</param>
/// <param name="LibraryId">The Id of the media library the album belongs to. Required.</param>
[DebuggerDisplay("AlbumId: {AlbumId}")]
public record GetMusicAlbumViewRequest(
    string ArtistId,
    string AlbumId,
    Guid LibraryId
);
