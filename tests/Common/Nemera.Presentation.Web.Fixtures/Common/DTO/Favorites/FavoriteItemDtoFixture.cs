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
/// Fixture class for the <see cref="FavoriteItemDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class FavoriteItemDtoFixture
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
    /// Creates a new <see cref="FavoriteItemDto"/> instance with randomized test data.
    /// </summary>
    /// <param name="mediaKind">Optional kind of the favorited media item.</param>
    /// <param name="mediaItemId">Optional Id of the favorited media item.</param>
    /// <param name="isAvailable">Optional value indicating whether the media item still exists and can be accessed by the user.</param>
    /// <param name="libraryId">Optional Id of the media library that contains the media item.</param>
    /// <param name="title">Optional title of the media item.</param>
    /// <param name="collectionId">Optional Id of the collection that contains the media item.</param>
    /// <param name="collectionTitle">Optional title of the collection that contains the media item.</param>
    /// <param name="creatorId">Optional Id of the creator of the media item.</param>
    /// <param name="creatorName">Optional name of the creator of the media item.</param>
    /// <param name="coverPath">Optional path of the cover art of the media item.</param>
    /// <param name="includeCoverPath">Whether the cover path should be included, or forced to <see langword="null"/>.</param>
    /// <returns>A configured <see cref="FavoriteItemDto"/> instance.</returns>
    public FavoriteItemDto Create(
        FavoriteMediaKind? mediaKind = null,
        Guid? mediaItemId = null,
        bool? isAvailable = null,
        Guid? libraryId = null,
        string? title = null,
        Guid? collectionId = null,
        string? collectionTitle = null,
        Guid? creatorId = null,
        string? creatorName = null,
        string? coverPath = null,
        bool includeCoverPath = true)
    {
        return new FavoriteItemDto(
            MediaKind: mediaKind ?? _faker.PickRandom(_supportedMediaKinds),
            MediaItemId: mediaItemId ?? _faker.Random.Guid(),
            IsAvailable: isAvailable ?? _faker.Random.Bool(),
            LibraryId: libraryId ?? _faker.Random.Guid(),
            Title: title ?? _faker.Lorem.Sentence(3),
            CollectionId: collectionId ?? _faker.Random.Guid(),
            CollectionTitle: collectionTitle ?? _faker.Lorem.Sentence(3),
            CreatorId: creatorId ?? _faker.Random.Guid(),
            CreatorName: creatorName ?? _faker.Name.FullName(),
            CoverPath: includeCoverPath ? (coverPath ?? $"covers/{_faker.Random.AlphaNumeric(12)}.jpg") : null
        );
    }

    /// <summary>
    /// Creates multiple <see cref="FavoriteItemDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="FavoriteItemDto"/> instances.</returns>
    public List<FavoriteItemDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
