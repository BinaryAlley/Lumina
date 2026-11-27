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
/// Fixture class for the <see cref="GetFavoriteStatusRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetFavoriteStatusRequestFixture
{
    private const int DEFAULT_MEDIA_ITEM_COUNT = 3;

    private readonly Faker _faker = new();
    private readonly FavoriteMediaKind[] _supportedMediaKinds =
    [
        FavoriteMediaKind.MusicArtist,
        FavoriteMediaKind.MusicAlbum,
        FavoriteMediaKind.MusicTrack,
        FavoriteMediaKind.Book
    ];

    /// <summary>
    /// Creates a random valid <see cref="GetFavoriteStatusRequest"/>.
    /// </summary>
    /// <param name="mediaKind">Optional. The kind of the media items to check. Defaults to a random supported kind.</param>
    /// <param name="mediaItemIds">Optional. The Ids of the media items to check. Defaults to a bounded list of random Ids.</param>
    /// <param name="includeMediaKind">Whether the media kind should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeMediaItemIds">Whether the media item Ids should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="GetFavoriteStatusRequest"/>.</returns>
    public GetFavoriteStatusRequest Create(
        FavoriteMediaKind? mediaKind = null,
        List<Guid>? mediaItemIds = null,
        bool includeMediaKind = true,
        bool includeMediaItemIds = true)
    {
        return new GetFavoriteStatusRequest(
            includeMediaKind ? (mediaKind ?? _faker.Random.ArrayElement(_supportedMediaKinds)) : null,
            includeMediaItemIds ? (mediaItemIds ?? CreateRandomMediaItemIds()) : null);
    }

    /// <summary>
    /// Creates a list of <see cref="GetFavoriteStatusRequest"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="GetFavoriteStatusRequest"/> instances.</returns>
    public List<GetFavoriteStatusRequest> CreateMany(int count = 3)
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
