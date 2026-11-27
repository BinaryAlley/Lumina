#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.Responses.Favorites;
using Lumina.Domain.SharedKernel.Common.Enums.Favorites;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Responses.Favorites;

/// <summary>
/// Fixture class for the <see cref="FavoriteItemResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class FavoriteItemResponseFixture
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
    /// Creates a random valid <see cref="FavoriteItemResponse"/>.
    /// </summary>
    /// <param name="mediaKind">Optional. The kind of the favorited media item. Defaults to a random supported kind.</param>
    /// <param name="mediaItemId">Optional. The Id of the favorited media item.</param>
    /// <param name="isAvailable">Optional. Whether the media item is available.</param>
    /// <param name="libraryId">Optional. The Id of the media library that contains the media item.</param>
    /// <param name="title">Optional. The title of the media item.</param>
    /// <param name="collectionId">Optional. The Id of the collection that contains the media item.</param>
    /// <param name="collectionTitle">Optional. The title of the collection that contains the media item.</param>
    /// <param name="creatorId">Optional. The Id of the creator of the media item.</param>
    /// <param name="creatorName">Optional. The name of the creator of the media item.</param>
    /// <param name="coverPath">Optional. The path of the cover art of the media item.</param>
    /// <param name="includeCoverPath">Whether the cover path should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="FavoriteItemResponse"/>.</returns>
    public FavoriteItemResponse Create(
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
        return new FavoriteItemResponse(
            mediaKind ?? _faker.Random.ArrayElement(_supportedMediaKinds),
            mediaItemId ?? _faker.Random.Guid(),
            isAvailable ?? _faker.Random.Bool(),
            libraryId ?? _faker.Random.Guid(),
            title ?? _faker.Lorem.Sentence(3),
            collectionId ?? _faker.Random.Guid(),
            collectionTitle ?? _faker.Lorem.Sentence(3),
            creatorId ?? _faker.Random.Guid(),
            creatorName ?? _faker.Name.FullName(),
            includeCoverPath ? (coverPath ?? $"/covers/{_faker.Random.Guid()}.jpg") : null);
    }

    /// <summary>
    /// Creates a list of <see cref="FavoriteItemResponse"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="FavoriteItemResponse"/> instances.</returns>
    public List<FavoriteItemResponse> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
