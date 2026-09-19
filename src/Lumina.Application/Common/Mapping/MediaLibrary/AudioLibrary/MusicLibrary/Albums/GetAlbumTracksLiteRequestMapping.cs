#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DTO.Pagination;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Albums.Queries.GetAlbumTracksLite;
using Lumina.Contracts.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Albums;
#endregion

namespace Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Albums;

/// <summary>
/// Extension methods for converting <see cref="GetAlbumTracksLiteRequest"/>.
/// </summary>
public static class GetAlbumTracksLiteRequestMapping
{
    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="GetAlbumTracksLiteQuery"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <param name="libraryId">The Id of the library the album belongs to, taken from the route.</param>
    /// <param name="artistId">The Id of the artist the album belongs to, taken from the route.</param>
    /// <param name="albumId">The Id of the album whose tracks are retrieved, taken from the route.</param>
    /// <returns>The converted query.</returns>
    public static GetAlbumTracksLiteQuery ToQuery(this GetAlbumTracksLiteRequest request, string? libraryId, string? artistId, string? albumId)
    {
        PaginationDataDto? paginationData = null;
        if (request.CurrentPage is not null || request.PerPage is not null)
            paginationData = new PaginationDataDto
            {
                CurrentPage = request.CurrentPage ?? 1,
                PerPage = request.PerPage ?? 200
            };

        return new GetAlbumTracksLiteQuery(
            libraryId,
            artistId,
            albumId,
            paginationData);
    }
}
