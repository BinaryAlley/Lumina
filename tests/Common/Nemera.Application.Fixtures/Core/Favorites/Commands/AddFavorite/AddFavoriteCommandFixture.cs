#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Core.Favorites.Commands.AddFavorite;
using Lumina.Domain.SharedKernel.Common.Enums.Favorites;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.Favorites.Commands.AddFavorite;

/// <summary>
/// Fixture class for the <see cref="AddFavoriteCommand"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddFavoriteCommandFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="AddFavoriteCommand"/>.
    /// </summary>
    /// <param name="mediaKind">Optional. The kind of the media item to favorite. Defaults to a supported kind.</param>
    /// <param name="mediaItemId">Optional. The unique identifier of the media item to favorite.</param>
    /// <param name="includeMediaKind">Whether the media kind should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeMediaItemId">Whether the media item Id should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="AddFavoriteCommand"/>.</returns>
    public AddFavoriteCommand Create(
        FavoriteMediaKind? mediaKind = null,
        Guid? mediaItemId = null,
        bool includeMediaKind = true,
        bool includeMediaItemId = true)
    {
        return new AddFavoriteCommand(
            includeMediaKind ? (mediaKind ?? FavoriteMediaKind.MusicTrack) : null,
            includeMediaItemId ? (mediaItemId ?? _faker.Random.Guid()) : null);
    }

    /// <summary>
    /// Creates a list of <see cref="AddFavoriteCommand"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="AddFavoriteCommand"/> instances.</returns>
    public List<AddFavoriteCommand> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
