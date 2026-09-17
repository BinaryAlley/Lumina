#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Albums;

/// <summary>
/// Represents a request to get the lightweight read models of the tracks of an album.
/// </summary>
/// <param name="ArtistId">The unique identifier of the artist the album belongs to, taken from the route.</param>
/// <param name="AlbumId">The unique identifier of the album whose tracks are retrieved.</param>
/// <param name="CurrentPage">The page of results to retrieve. Optional.</param>
/// <param name="PerPage">The maximum number of tracks to retrieve per page. Optional.</param>
[DebuggerDisplay("AlbumId: {AlbumId}")]
public record GetAlbumTracksLiteRequest(
    Guid ArtistId,
    Guid AlbumId,
    int? CurrentPage,
    int? PerPage
);
