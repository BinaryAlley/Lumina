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
/// Fixture class for the <see cref="FavoriteResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class FavoriteResponseFixture
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
    /// Creates a random valid <see cref="FavoriteResponse"/>.
    /// </summary>
    /// <param name="id">Optional. The Id of the favorite.</param>
    /// <param name="mediaKind">Optional. The kind of the favorited media item. Defaults to a random supported kind.</param>
    /// <param name="mediaItemId">Optional. The Id of the favorited media item.</param>
    /// <param name="favoritedOnUtc">Optional. The date and time when the media item was favorited. Defaults to a moment in the past.</param>
    /// <returns>The created <see cref="FavoriteResponse"/>.</returns>
    public FavoriteResponse Create(
        Guid? id = null,
        FavoriteMediaKind? mediaKind = null,
        Guid? mediaItemId = null,
        DateTime? favoritedOnUtc = null)
    {
        return new FavoriteResponse(
            id ?? _faker.Random.Guid(),
            mediaKind ?? _faker.Random.ArrayElement(_supportedMediaKinds),
            mediaItemId ?? _faker.Random.Guid(),
            favoritedOnUtc ?? DateTime.UtcNow.AddDays(-_faker.Random.Int(1, 30)).AddMinutes(-_faker.Random.Int(0, 59)));
    }

    /// <summary>
    /// Creates a list of <see cref="FavoriteResponse"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="FavoriteResponse"/> instances.</returns>
    public List<FavoriteResponse> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
