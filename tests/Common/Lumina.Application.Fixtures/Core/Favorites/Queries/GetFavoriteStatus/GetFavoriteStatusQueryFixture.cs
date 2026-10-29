#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Core.Favorites.Queries.GetFavoriteStatus;
using Lumina.Domain.SharedKernel.Common.Enums.Favorites;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.Favorites.Queries.GetFavoriteStatus;

/// <summary>
/// Fixture class for the <see cref="GetFavoriteStatusQuery"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetFavoriteStatusQueryFixture
{
    private const int DEFAULT_MEDIA_ITEM_COUNT = 3;

    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="GetFavoriteStatusQuery"/>.
    /// </summary>
    /// <param name="mediaKind">Optional. The kind of the media items to check. Defaults to a supported kind.</param>
    /// <param name="mediaItemIds">Optional. The unique identifiers of the media items to check. Defaults to a bounded list of random Ids.</param>
    /// <param name="includeMediaKind">Whether the media kind should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeMediaItemIds">Whether the media item Ids should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="GetFavoriteStatusQuery"/>.</returns>
    public GetFavoriteStatusQuery Create(
        FavoriteMediaKind? mediaKind = null,
        IReadOnlyList<Guid>? mediaItemIds = null,
        bool includeMediaKind = true,
        bool includeMediaItemIds = true)
    {
        return new GetFavoriteStatusQuery(
            includeMediaKind ? (mediaKind ?? FavoriteMediaKind.MusicTrack) : null,
            includeMediaItemIds ? (mediaItemIds ?? CreateRandomMediaItemIds()) : null);
    }

    /// <summary>
    /// Creates a list of <see cref="GetFavoriteStatusQuery"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="GetFavoriteStatusQuery"/> instances.</returns>
    public List<GetFavoriteStatusQuery> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }

    /// <summary>
    /// Creates a bounded list of random media item Ids.
    /// </summary>
    /// <returns>The created list of media item Ids.</returns>
    private List<Guid> CreateRandomMediaItemIds()
    {
        return [.. Enumerable.Range(0, DEFAULT_MEDIA_ITEM_COUNT).Select(_ => _faker.Random.Guid())];
    }
}
