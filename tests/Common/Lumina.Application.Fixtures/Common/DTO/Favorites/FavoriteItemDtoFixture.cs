#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Favorites;
using Lumina.Application.Common.DTO.Favorites;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Favorites;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Common.DTO.Favorites;

/// <summary>
/// Fixture class for the <see cref="FavoriteItemDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class FavoriteItemDtoFixture
{
    private readonly FavoriteEntityFixture _favoriteEntityFixture = new();
    private readonly FavoriteItemDisplayDtoFixture _favoriteItemDisplayDtoFixture = new();

    /// <summary>
    /// Creates a random valid <see cref="FavoriteItemDto"/>.
    /// </summary>
    /// <param name="favorite">Optional. The stored favorite.</param>
    /// <param name="display">Optional. The display data of the referenced media item.</param>
    /// <param name="includeDisplay">Whether the display data should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="FavoriteItemDto"/>.</returns>
    public FavoriteItemDto Create(
        FavoriteEntity? favorite = null,
        FavoriteItemDisplayDto? display = null,
        bool includeDisplay = true)
    {
        return new FavoriteItemDto(
            favorite ?? _favoriteEntityFixture.Create(),
            includeDisplay ? (display ?? _favoriteItemDisplayDtoFixture.Create()) : null);
    }

    /// <summary>
    /// Creates a list of <see cref="FavoriteItemDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="FavoriteItemDto"/> instances.</returns>
    public List<FavoriteItemDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
