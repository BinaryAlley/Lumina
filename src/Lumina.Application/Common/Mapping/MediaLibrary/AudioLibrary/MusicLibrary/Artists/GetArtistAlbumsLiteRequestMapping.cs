#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DTO.Pagination;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Queries.GetArtistAlbumsLite;
using Lumina.Contracts.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Artists;
#endregion

namespace Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Artists;

/// <summary>
/// Extension methods for converting <see cref="GetArtistAlbumsLiteRequest"/>.
/// </summary>
public static class GetArtistAlbumsLiteRequestMapping
{
    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="GetArtistAlbumsLiteQuery"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <param name="libraryId">The Id of the library the artist belongs to, taken from the route.</param>
    /// <param name="artistId">The Id of the artist whose albums are retrieved, taken from the route.</param>
    /// <returns>The converted query.</returns>
    public static GetArtistAlbumsLiteQuery ToQuery(this GetArtistAlbumsLiteRequest request, string? libraryId, string? artistId)
    {
        PaginationDataDto? paginationData = null;
        if (request.CurrentPage is not null || request.PerPage is not null)
            paginationData = new PaginationDataDto
            {
                CurrentPage = request.CurrentPage ?? 1,
                PerPage = request.PerPage ?? 200
            };

        return new GetArtistAlbumsLiteQuery(
            libraryId,
            artistId,
            paginationData);
    }
}
