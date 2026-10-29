#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Common.DTO.Favorites;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Common.DTO.Favorites;

/// <summary>
/// Fixture class for the <see cref="FavoriteItemDisplayDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class FavoriteItemDisplayDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="FavoriteItemDisplayDto"/>.
    /// </summary>
    /// <param name="libraryId">Optional. The Id of the media library that contains the media item.</param>
    /// <param name="title">Optional. The title of the media item.</param>
    /// <param name="collectionId">Optional. The Id of the collection that contains the media item.</param>
    /// <param name="collectionTitle">Optional. The title of the collection that contains the media item.</param>
    /// <param name="creatorId">Optional. The Id of the creator of the media item.</param>
    /// <param name="creatorName">Optional. The name of the creator of the media item.</param>
    /// <param name="coverPath">Optional. The path of the cover art of the media item.</param>
    /// <param name="includeCoverPath">Whether the cover path should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="FavoriteItemDisplayDto"/>.</returns>
    public FavoriteItemDisplayDto Create(
        Guid? libraryId = null,
        string? title = null,
        Guid? collectionId = null,
        string? collectionTitle = null,
        Guid? creatorId = null,
        string? creatorName = null,
        string? coverPath = null,
        bool includeCoverPath = true)
    {
        return new FavoriteItemDisplayDto(
            libraryId ?? _faker.Random.Guid(),
            title ?? _faker.Lorem.Sentence(3),
            collectionId ?? _faker.Random.Guid(),
            collectionTitle ?? _faker.Lorem.Sentence(3),
            creatorId ?? _faker.Random.Guid(),
            creatorName ?? _faker.Name.FullName(),
            includeCoverPath ? (coverPath ?? $"/covers/{_faker.Random.Guid()}.jpg") : null);
    }

    /// <summary>
    /// Creates a list of <see cref="FavoriteItemDisplayDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="FavoriteItemDisplayDto"/> instances.</returns>
    public List<FavoriteItemDisplayDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
