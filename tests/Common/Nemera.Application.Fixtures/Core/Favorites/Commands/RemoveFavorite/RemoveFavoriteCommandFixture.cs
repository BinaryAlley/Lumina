#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Core.Favorites.Commands.RemoveFavorite;
using Lumina.Domain.SharedKernel.Common.Enums.Favorites;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.Favorites.Commands.RemoveFavorite;

/// <summary>
/// Fixture class for the <see cref="RemoveFavoriteCommand"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class RemoveFavoriteCommandFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="RemoveFavoriteCommand"/>.
    /// </summary>
    /// <param name="mediaKind">Optional. The name of the kind of the favorited media item. Defaults to the name of a supported kind.</param>
    /// <param name="mediaItemId">Optional. The unique identifier of the favorited media item.</param>
    /// <param name="includeMediaKind">Whether the media kind should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeMediaItemId">Whether the media item Id should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="RemoveFavoriteCommand"/>.</returns>
    public RemoveFavoriteCommand Create(
        string? mediaKind = null,
        string? mediaItemId = null,
        bool includeMediaKind = true,
        bool includeMediaItemId = true)
    {
        return new RemoveFavoriteCommand(
            includeMediaKind ? (mediaKind ?? nameof(FavoriteMediaKind.MusicTrack)) : null,
            includeMediaItemId ? (mediaItemId ?? _faker.Random.Guid().ToString()) : null);
    }

    /// <summary>
    /// Creates a list of <see cref="RemoveFavoriteCommand"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="RemoveFavoriteCommand"/> instances.</returns>
    public List<RemoveFavoriteCommand> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
