#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.Requests.Favorites;
using Lumina.Domain.SharedKernel.Common.Enums.Favorites;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Requests.Favorites;

/// <summary>
/// Fixture class for the <see cref="AddFavoriteRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddFavoriteRequestFixture
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
    /// Creates a random valid <see cref="AddFavoriteRequest"/>.
    /// </summary>
    /// <param name="mediaKind">Optional. The kind of the media item to favorite. Defaults to a random supported kind.</param>
    /// <param name="mediaItemId">Optional. The Id of the media item to favorite.</param>
    /// <param name="includeMediaKind">Whether the media kind should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeMediaItemId">Whether the media item Id should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="AddFavoriteRequest"/>.</returns>
    public AddFavoriteRequest Create(
        FavoriteMediaKind? mediaKind = null,
        Guid? mediaItemId = null,
        bool includeMediaKind = true,
        bool includeMediaItemId = true)
    {
        return new AddFavoriteRequest(
            includeMediaKind ? (mediaKind ?? _faker.Random.ArrayElement(_supportedMediaKinds)) : null,
            includeMediaItemId ? (mediaItemId ?? _faker.Random.Guid()) : null);
    }

    /// <summary>
    /// Creates a list of <see cref="AddFavoriteRequest"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="AddFavoriteRequest"/> instances.</returns>
    public List<AddFavoriteRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
