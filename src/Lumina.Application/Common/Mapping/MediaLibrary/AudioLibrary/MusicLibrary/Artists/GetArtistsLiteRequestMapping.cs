#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DTO.Pagination;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Queries.GetArtistsLite;
using Lumina.Contracts.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Artists;
#endregion

namespace Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Artists;

/// <summary>
/// Extension methods for converting <see cref="GetArtistsLiteRequest"/>.
/// </summary>
public static class GetArtistsLiteRequestMapping
{
    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="GetArtistsLiteQuery"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <param name="libraryId">The Id of the library whose artists are retrieved, taken from the route.</param>
    /// <returns>The converted query.</returns>
    public static GetArtistsLiteQuery ToQuery(this GetArtistsLiteRequest request, string? libraryId)
    {
        PaginationDataDto? paginationData = null;
        if (request.CurrentPage is not null || request.PerPage is not null)
            paginationData = new PaginationDataDto
            {
                CurrentPage = request.CurrentPage ?? 1,
                PerPage = request.PerPage ?? 200
            };

        return new GetArtistsLiteQuery(
            libraryId,
            paginationData,
            request.SearchTerm);
    }
}
