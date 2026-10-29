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
/// Fixture class for the <see cref="AddFavoriteRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddFavoriteRequestFixture
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
    /// Creates a new <see cref="AddFavoriteRequest"/> instance with randomized test data.
    /// </summary>
    /// <param name="mediaKind">Optional kind of the media item to favorite.</param>
    /// <param name="mediaItemId">Optional Id of the media item to favorite.</param>
    /// <param name="includeMediaKind">Whether the media kind should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeMediaItemId">Whether the media item Id should be included, or forced to <see langword="null"/>.</param>
    /// <returns>A configured <see cref="AddFavoriteRequest"/> instance.</returns>
    public AddFavoriteRequest Create(
        FavoriteMediaKind? mediaKind = null,
        Guid? mediaItemId = null,
        bool includeMediaKind = true,
        bool includeMediaItemId = true)
    {
        return new AddFavoriteRequest(
            MediaKind: includeMediaKind ? (mediaKind ?? _faker.PickRandom(_supportedMediaKinds)) : null,
            MediaItemId: includeMediaItemId ? (mediaItemId ?? _faker.Random.Guid()) : null
        );
    }

    /// <summary>
    /// Creates multiple <see cref="AddFavoriteRequest"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="AddFavoriteRequest"/> instances.</returns>
    public List<AddFavoriteRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
