#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.Responses.Favorites;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Responses.Favorites;

/// <summary>
/// Fixture class for the <see cref="FavoriteStatusResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class FavoriteStatusResponseFixture
{
    private const int DEFAULT_FAVORITED_COUNT = 3;

    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="FavoriteStatusResponse"/>.
    /// </summary>
    /// <param name="favoritedMediaItemIds">Optional. The Ids of the media items that are favorites. Defaults to a bounded list of random Ids.</param>
    /// <returns>The created <see cref="FavoriteStatusResponse"/>.</returns>
    public FavoriteStatusResponse Create(IReadOnlyList<Guid>? favoritedMediaItemIds = null)
    {
        return new FavoriteStatusResponse(
            favoritedMediaItemIds ?? CreateRandomMediaItemIds());
    }

    /// <summary>
    /// Creates a list of <see cref="FavoriteStatusResponse"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="FavoriteStatusResponse"/> instances.</returns>
    public List<FavoriteStatusResponse> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }

    /// <summary>
    /// Creates a bounded list of random media item Ids.
    /// </summary>
    /// <returns>The created list of media item Ids.</returns>
    private List<Guid> CreateRandomMediaItemIds()
    {
        return [.. Enumerable.Range(0, DEFAULT_FAVORITED_COUNT).Select(_ => _faker.Random.Guid())];
    }
}
