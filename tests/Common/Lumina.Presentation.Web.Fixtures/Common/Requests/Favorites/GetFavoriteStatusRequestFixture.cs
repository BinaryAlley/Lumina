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
/// Fixture class for the <see cref="GetFavoriteStatusRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetFavoriteStatusRequestFixture
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
    /// Creates a new <see cref="GetFavoriteStatusRequest"/> instance with randomized test data.
    /// </summary>
    /// <param name="mediaKind">Optional kind of the media items to check.</param>
    /// <param name="mediaItemIds">Optional Ids of the media items to check.</param>
    /// <param name="mediaItemCount">Optional number of random media item Ids to generate, between 1 and 5.</param>
    /// <param name="includeMediaKind">Whether the media kind should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeMediaItemIds">Whether the media item Ids should be included, or forced to <see langword="null"/>.</param>
    /// <returns>A configured <see cref="GetFavoriteStatusRequest"/> instance.</returns>
    public GetFavoriteStatusRequest Create(
        FavoriteMediaKind? mediaKind = null,
        List<Guid>? mediaItemIds = null,
        int? mediaItemCount = null,
        bool includeMediaKind = true,
        bool includeMediaItemIds = true)
    {
        int resolvedCount = mediaItemCount ?? _faker.Random.Int(1, 5);
        return new GetFavoriteStatusRequest(
            MediaKind: includeMediaKind ? (mediaKind ?? _faker.PickRandom(_supportedMediaKinds)) : null,
            MediaItemIds: includeMediaItemIds ? (mediaItemIds ?? [.. Enumerable.Range(0, resolvedCount).Select(_ => _faker.Random.Guid())]) : null
        );
    }

    /// <summary>
    /// Creates multiple <see cref="GetFavoriteStatusRequest"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="GetFavoriteStatusRequest"/> instances.</returns>
    public List<GetFavoriteStatusRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
