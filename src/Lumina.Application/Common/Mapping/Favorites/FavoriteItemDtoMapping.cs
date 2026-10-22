#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DTO.Favorites;
using Lumina.Application.Common.DTO.Pagination;
using Lumina.Contracts.Responses.Common;
using Lumina.Contracts.Responses.Favorites;
using System;
using System.Linq;
#endregion

namespace Lumina.Application.Common.Mapping.Favorites;

/// <summary>
/// Extension methods for converting <see cref="FavoriteItemDto"/>.
/// </summary>
public static class FavoriteItemDtoMapping
{
    /// <summary>
    /// Converts <paramref name="data"/> to <see cref="FavoriteItemResponse"/>.
    /// </summary>
    /// <param name="data">The favorite data to be converted.</param>
    /// <returns>The converted response, flagged as not available with empty display fields when the display data is missing.</returns>
    public static FavoriteItemResponse ToResponse(this FavoriteItemDto data)
    {
        if (data.Display is null)
            return new FavoriteItemResponse(
                data.Favorite.MediaKind,
                data.Favorite.MediaItemId,
                false,
                Guid.Empty,
                string.Empty,
                Guid.Empty,
                string.Empty,
                Guid.Empty,
                string.Empty,
                null);

        FavoriteItemDisplayDto display = data.Display;
        return new FavoriteItemResponse(
            data.Favorite.MediaKind,
            data.Favorite.MediaItemId,
            true,
            display.LibraryId,
            display.Title,
            display.CollectionId,
            display.CollectionTitle,
            display.CreatorId,
            display.CreatorName,
            display.CoverPath);
    }

    /// <summary>
    /// Converts <paramref name="data"/> to a paginated collection of <see cref="FavoriteItemResponse"/>.
    /// </summary>
    /// <param name="data">The paginated favorite data to be converted.</param>
    /// <returns>The converted paginated responses.</returns>
    public static PaginatedResponse<FavoriteItemResponse> ToResponses(this PaginatedResultDto<FavoriteItemDto> data)
    {
        return new PaginatedResponse<FavoriteItemResponse>
        {
            Data = [.. data.Data.Select(item => item.ToResponse())],
            CurrentPage = data.CurrentPage,
            PerPage = data.PerPage,
            Count = data.Count,
            NumberOfPages = data.NumberOfPages
        };
    }
}
