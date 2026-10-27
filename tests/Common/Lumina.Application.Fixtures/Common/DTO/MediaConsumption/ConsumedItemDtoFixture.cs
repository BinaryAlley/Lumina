#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Common.DTO.MediaConsumption;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Common.DTO.MediaConsumption;

/// <summary>
/// Fixture class for the <see cref="ConsumedItemDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class ConsumedItemDtoFixture
{
    private const int MAXIMUM_DURATION_MILLISECONDS = 36_000_000;

    private readonly Faker _faker = new();
    private readonly MediaKind[] _supportedMediaKinds =
    [
        MediaKind.MusicTrack,
        MediaKind.PodcastEpisode,
        MediaKind.Audiobook,
        MediaKind.Movie,
        MediaKind.TvEpisode,
        MediaKind.Book,
        MediaKind.Photo,
        MediaKind.Video
    ];

    /// <summary>
    /// Creates a random valid <see cref="ConsumedItemDto"/>.
    /// </summary>
    /// <param name="mediaKind">Optional. The kind of the consumed media item. Defaults to a random supported kind.</param>
    /// <param name="mediaItemId">Optional. The Id of the consumed media item.</param>
    /// <param name="libraryId">Optional. The Id of the media library that contains the media item.</param>
    /// <param name="isAvailable">Optional. Whether the media item is still available.</param>
    /// <param name="title">Optional. The title of the media item.</param>
    /// <param name="collectionTitle">Optional. The title of the collection that contains the media item.</param>
    /// <param name="creatorName">Optional. The name of the creator of the media item.</param>
    /// <param name="coverPath">Optional. The path of the cover art of the media item.</param>
    /// <param name="includeCoverPath">Whether the cover path should be included, or forced to <see langword="null"/>.</param>
    /// <param name="lastConsumedOnUtc">Optional. The date and time when the media item was last consumed.</param>
    /// <param name="countedSessionCount">Optional. The number of consumption sessions that count towards statistics.</param>
    /// <param name="completedSessionCount">Optional. The number of consumption sessions that completed.</param>
    /// <param name="consumedDurationMilliseconds">Optional. The active duration consumed in the media item, in milliseconds. Defaults to a bounded random duration.</param>
    /// <returns>The created <see cref="ConsumedItemDto"/>.</returns>
    public ConsumedItemDto Create(
        MediaKind? mediaKind = null,
        Guid? mediaItemId = null,
        Guid? libraryId = null,
        bool? isAvailable = null,
        string? title = null,
        string? collectionTitle = null,
        string? creatorName = null,
        string? coverPath = null,
        bool includeCoverPath = true,
        DateTime? lastConsumedOnUtc = null,
        int? countedSessionCount = null,
        int? completedSessionCount = null,
        long? consumedDurationMilliseconds = null)
    {
        return new ConsumedItemDto(
            mediaKind ?? _faker.Random.ArrayElement(_supportedMediaKinds),
            mediaItemId ?? _faker.Random.Guid(),
            libraryId ?? _faker.Random.Guid(),
            isAvailable ?? _faker.Random.Bool(),
            title ?? _faker.Commerce.ProductName(),
            collectionTitle ?? _faker.Commerce.Department(),
            creatorName ?? _faker.Name.FullName(),
            includeCoverPath ? coverPath ?? _faker.System.FilePath() : null,
            lastConsumedOnUtc ?? DateTime.UtcNow.AddHours(-_faker.Random.Int(1, 720)),
            countedSessionCount ?? _faker.Random.Int(0, 100),
            completedSessionCount ?? _faker.Random.Int(0, 100),
            consumedDurationMilliseconds ?? _faker.Random.Long(0, MAXIMUM_DURATION_MILLISECONDS));
    }

    /// <summary>
    /// Creates a list of <see cref="ConsumedItemDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="ConsumedItemDto"/> instances.</returns>
    public List<ConsumedItemDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
