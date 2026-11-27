#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.DTO.Pagination;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Albums.Queries.GetAlbumTracksLite;

/// <summary>
/// Query for getting the lightweight read models of all the tracks of an album.
/// </summary>
/// <param name="LibraryId">The unique identifier of the media library the album belongs to, taken from the route.</param>
/// <param name="ArtistId">The unique identifier of the artist the album belongs to, taken from the route.</param>
/// <param name="AlbumId">The unique identifier of the album whose tracks are retrieved, taken from the route.</param>
/// <param name="PaginationData">The object containing the requested pagination data.</param>
[DebuggerDisplay("AlbumId: {AlbumId}")]
public record GetAlbumTracksLiteQuery(
    string? LibraryId,
    string? ArtistId,
    string? AlbumId,
    PaginationDataDto? PaginationData
) : IQuery;
