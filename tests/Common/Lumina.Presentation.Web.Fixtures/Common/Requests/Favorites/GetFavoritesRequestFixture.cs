#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.Enums.Favorites;
using Lumina.Presentation.Web.Common.Requests.Favorites;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.Requests.Favorites;

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
    /// Creates a new <see cref="GetFavoritesRequest"/> instance with randomized test data.
    /// </summary>
    /// <param name="mediaKind">Optional kind of media item to restrict the favorites to.</param>
    /// <param name="currentPage">Optional one based page of favorites to return.</param>
    /// <param name="perPage">Optional number of favorites per page.</param>
    /// <param name="includeMediaKind">Whether the media kind should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeCurrentPage">Whether the current page should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includePerPage">Whether the page size should be included, or forced to <see langword="null"/>.</param>
    /// <returns>A configured <see cref="GetFavoritesRequest"/> instance.</returns>
    public GetFavoritesRequest Create(
        FavoriteMediaKind? mediaKind = null,
        int? currentPage = null,
        int? perPage = null,
        bool includeMediaKind = true,
        bool includeCurrentPage = true,
        bool includePerPage = true)
    {
        return new GetFavoritesRequest(
            MediaKind: includeMediaKind ? (mediaKind ?? _faker.PickRandom(_supportedMediaKinds)) : null,
            CurrentPage: includeCurrentPage ? (currentPage ?? _faker.Random.Int(1, 10)) : null,
            PerPage: includePerPage ? (perPage ?? _faker.Random.Int(1, 100)) : null
        );
    }

    /// <summary>
    /// Creates multiple <see cref="GetFavoritesRequest"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="GetFavoritesRequest"/> instances.</returns>
    public List<GetFavoritesRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
