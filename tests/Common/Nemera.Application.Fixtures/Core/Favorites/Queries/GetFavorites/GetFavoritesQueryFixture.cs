#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DTO.Pagination;
using Lumina.Application.Core.Favorites.Queries.GetFavorites;
using Lumina.Application.Fixtures.Common.DTO.Pagination;
using Lumina.Domain.SharedKernel.Common.Enums.Favorites;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.Favorites.Queries.GetFavorites;

/// <summary>
/// Fixture class for the <see cref="GetFavoritesQuery"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetFavoritesQueryFixture
{
    private readonly PaginationDataDtoFixture _paginationDataDtoFixture = new();

    /// <summary>
    /// Creates a random valid <see cref="GetFavoritesQuery"/>.
    /// </summary>
    /// <param name="mediaKind">Optional. The kind of media item to restrict the favorites to. Defaults to a supported kind.</param>
    /// <param name="paginationData">Optional. The object containing the requested pagination data. Defaults to a random page.</param>
    /// <param name="includeMediaKind">Whether the media kind should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includePaginationData">Whether the pagination data should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="GetFavoritesQuery"/>.</returns>
    public GetFavoritesQuery Create(
        FavoriteMediaKind? mediaKind = null,
        PaginationDataDto? paginationData = null,
        bool includeMediaKind = true,
        bool includePaginationData = true)
    {
        return new GetFavoritesQuery(
            includeMediaKind ? (mediaKind ?? FavoriteMediaKind.MusicTrack) : null,
            includePaginationData ? (paginationData ?? _paginationDataDtoFixture.Create()) : null);
    }

    /// <summary>
    /// Creates a list of <see cref="GetFavoritesQuery"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="GetFavoritesQuery"/> instances.</returns>
    public List<GetFavoritesQuery> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
