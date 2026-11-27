#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.DTO.Favorites;
using Lumina.Presentation.Web.Common.Enums.Favorites;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.DTO.Favorites;

/// <summary>
/// Fixture class for the <see cref="FavoriteDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class FavoriteDtoFixture
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
    /// Creates a new <see cref="FavoriteDto"/> instance with randomized test data.
    /// </summary>
    /// <param name="id">Optional Id of the favorite.</param>
    /// <param name="mediaKind">Optional kind of the favorited media item.</param>
    /// <param name="mediaItemId">Optional Id of the favorited media item.</param>
    /// <param name="favoritedOnUtc">Optional date and time when the media item was favorited.</param>
    /// <returns>A configured <see cref="FavoriteDto"/> instance.</returns>
    public FavoriteDto Create(
        Guid? id = null,
        FavoriteMediaKind? mediaKind = null,
        Guid? mediaItemId = null,
        DateTime? favoritedOnUtc = null)
    {
        return new FavoriteDto(
            Id: id ?? _faker.Random.Guid(),
            MediaKind: mediaKind ?? _faker.PickRandom(_supportedMediaKinds),
            MediaItemId: mediaItemId ?? _faker.Random.Guid(),
            FavoritedOnUtc: favoritedOnUtc ?? _faker.Date.Between(new DateTime(2024, 1, 1), new DateTime(2026, 12, 31))
        );
    }

    /// <summary>
    /// Creates multiple <see cref="FavoriteDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="FavoriteDto"/> instances.</returns>
    public List<FavoriteDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
