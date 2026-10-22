#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DTO.Pagination;
using Lumina.Application.Core.Favorites.Queries.GetFavorites;
using Lumina.Contracts.Requests.Favorites;
#endregion

namespace Lumina.Application.Common.Mapping.Favorites;

/// <summary>
/// Extension methods for converting <see cref="GetFavoritesRequest"/>.
/// </summary>
public static class GetFavoritesRequestMapping
{
    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="GetFavoritesQuery"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <returns>The converted query.</returns>
    public static GetFavoritesQuery ToQuery(this GetFavoritesRequest request)
    {
        PaginationDataDto? paginationData = null;
        if (request.CurrentPage is not null || request.PerPage is not null)
            paginationData = new PaginationDataDto
            {
                CurrentPage = request.CurrentPage ?? 1,
                PerPage = request.PerPage ?? 200
            };

        return new GetFavoritesQuery(
            request.MediaKind,
            paginationData);
    }
}
