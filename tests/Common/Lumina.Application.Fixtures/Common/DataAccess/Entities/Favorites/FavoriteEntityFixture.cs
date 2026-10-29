#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Common.DataAccess.Entities.Favorites;
using Lumina.Domain.SharedKernel.Common.Enums.Favorites;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Common.DataAccess.Entities.Favorites;

/// <summary>
/// Fixture class for the <see cref="FavoriteEntity"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class FavoriteEntityFixture
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
    /// Creates a random valid <see cref="FavoriteEntity"/>.
    /// </summary>
    /// <param name="id">Optional. The Id of the favorite.</param>
    /// <param name="userId">Optional. The Id of the user that owns the favorite.</param>
    /// <param name="mediaKind">Optional. The kind of the favorited media item. Defaults to a random supported kind.</param>
    /// <param name="mediaItemId">Optional. The Id of the favorited media item.</param>
    /// <param name="createdOnUtc">Optional. The date and time when the favorite was created. Defaults to a moment in the past.</param>
    /// <param name="createdBy">Optional. The Id of the user that created the row.</param>
    /// <param name="updatedOnUtc">Optional. The date and time when the favorite was last updated. Defaults to a moment at or after the creation date.</param>
    /// <param name="updatedBy">Optional. The Id of the user that last updated the row.</param>
    /// <param name="includeUpdatedOnUtc">Whether the update time should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeUpdatedBy">Whether the updating user Id should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="FavoriteEntity"/>.</returns>
    public FavoriteEntity Create(
        Guid? id = null,
        Guid? userId = null,
        FavoriteMediaKind? mediaKind = null,
        Guid? mediaItemId = null,
        DateTime? createdOnUtc = null,
        Guid? createdBy = null,
        DateTime? updatedOnUtc = null,
        Guid? updatedBy = null,
        bool includeUpdatedOnUtc = true,
        bool includeUpdatedBy = true)
    {
        DateTime resolvedCreatedOnUtc = createdOnUtc ?? DateTime.UtcNow.AddDays(-_faker.Random.Int(1, 30)).AddMinutes(-_faker.Random.Int(0, 59));

        return new FavoriteEntity
        {
            Id = id ?? _faker.Random.Guid(),
            UserId = userId ?? _faker.Random.Guid(),
            MediaKind = mediaKind ?? _faker.Random.ArrayElement(_supportedMediaKinds),
            MediaItemId = mediaItemId ?? _faker.Random.Guid(),
            CreatedOnUtc = resolvedCreatedOnUtc,
            CreatedBy = createdBy ?? _faker.Random.Guid(),
            UpdatedOnUtc = includeUpdatedOnUtc ? (updatedOnUtc ?? resolvedCreatedOnUtc.AddMinutes(_faker.Random.Int(0, 120))) : null,
            UpdatedBy = includeUpdatedBy ? (updatedBy ?? _faker.Random.Guid()) : null
        };
    }

    /// <summary>
    /// Creates a list of <see cref="FavoriteEntity"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="FavoriteEntity"/> instances.</returns>
    public List<FavoriteEntity> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
