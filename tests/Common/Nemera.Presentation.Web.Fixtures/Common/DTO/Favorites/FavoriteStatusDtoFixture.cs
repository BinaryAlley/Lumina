#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.DTO.Favorites;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.DTO.Favorites;

/// <summary>
/// Fixture class for the <see cref="FavoriteStatusDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class FavoriteStatusDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a new <see cref="FavoriteStatusDto"/> instance with randomized test data.
    /// </summary>
    /// <param name="favoritedMediaItemIds">Optional Ids of the media items that are favorites of the current user.</param>
    /// <param name="mediaItemCount">Optional number of random favorited media item Ids to generate, between 0 and 5.</param>
    /// <returns>A configured <see cref="FavoriteStatusDto"/> instance.</returns>
    public FavoriteStatusDto Create(
        List<Guid>? favoritedMediaItemIds = null,
        int? mediaItemCount = null)
    {
        int resolvedCount = mediaItemCount ?? _faker.Random.Int(0, 5);
        return new FavoriteStatusDto(
            FavoritedMediaItemIds: favoritedMediaItemIds ?? [.. Enumerable.Range(0, resolvedCount).Select(_ => _faker.Random.Guid())]
        );
    }

    /// <summary>
    /// Creates multiple <see cref="FavoriteStatusDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="FavoriteStatusDto"/> instances.</returns>
    public List<FavoriteStatusDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
