#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.Requests.Favorites;
using Lumina.Domain.SharedKernel.Common.Enums.Favorites;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Requests.Favorites;

/// <summary>
/// Fixture class for the <see cref="GetFavoritesRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetFavoritesRequestFixture
{
    private readonly Faker _faker = new();
    private readonly FavoriteMediaKind[] _supportedMediaKinds =
    [
        FavoriteMediaKind.MusicArtist,
        FavoriteMediaKind.MusicAlbum,
        FavoriteMediaKind.MusicTrack,
        FavoriteMediaKind.Book
    ];

    /// <summary>
    /// Creates a random valid <see cref="GetFavoritesRequest"/>.
    /// </summary>
    /// <param name="mediaKind">Optional. The kind of media item to restrict the favorites to. Defaults to a random supported kind.</param>
    /// <param name="currentPage">Optional. The one based page of favorites to return. Defaults to a bounded random page.</param>
    /// <param name="perPage">Optional. The number of favorites per page. Defaults to a bounded random page size.</param>
    /// <param name="includeMediaKind">Whether the media kind should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeCurrentPage">Whether the current page should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includePerPage">Whether the per page count should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="GetFavoritesRequest"/>.</returns>
    public GetFavoritesRequest Create(
        FavoriteMediaKind? mediaKind = null,
        int? currentPage = null,
        int? perPage = null,
        bool includeMediaKind = true,
        bool includeCurrentPage = true,
        bool includePerPage = true)
    {
        return new GetFavoritesRequest(
            includeMediaKind ? (mediaKind ?? _faker.Random.ArrayElement(_supportedMediaKinds)) : null,
            includeCurrentPage ? (currentPage ?? _faker.Random.Int(1, 100)) : null,
            includePerPage ? (perPage ?? _faker.Random.Int(1, 200)) : null);
    }

    /// <summary>
    /// Creates a list of <see cref="GetFavoritesRequest"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="GetFavoritesRequest"/> instances.</returns>
    public List<GetFavoritesRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
