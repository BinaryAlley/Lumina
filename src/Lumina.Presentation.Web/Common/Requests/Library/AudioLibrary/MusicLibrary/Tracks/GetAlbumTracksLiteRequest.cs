#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.Requests.Library.AudioLibrary.MusicLibrary.Tracks;

/// <summary>
/// Represents a request to get the lightweight details of the tracks of an album.
/// </summary>
/// <param name="ArtistId">The unique identifier of the artist the album belongs to, taken from the route. Required.</param>
/// <param name="AlbumId">The unique identifier of the album whose tracks are retrieved, taken from the route. Required.</param>
/// <param name="LibraryId">The unique identifier of the media library the album belongs to. Required.</param>
/// <param name="CurrentPage">The page of results to retrieve. Optional.</param>
/// <param name="PerPage">The maximum number of tracks to retrieve per page. Optional.</param>
[DebuggerDisplay("AlbumId: {AlbumId}")]
public record GetAlbumTracksLiteRequest(
    string ArtistId,
    string AlbumId,
    Guid LibraryId,
    int? CurrentPage,
    int? PerPage
);
