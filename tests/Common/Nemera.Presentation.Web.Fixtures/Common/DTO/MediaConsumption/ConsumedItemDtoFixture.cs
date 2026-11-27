#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.DTO.MediaConsumption;
using Lumina.Presentation.Web.Common.Enums.MediaConsumption;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.DTO.MediaConsumption;

/// <summary>
/// Fixture class for the <see cref="ConsumedItemDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class ConsumedItemDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a new <see cref="ConsumedItemDto"/> instance with randomized test data.
    /// </summary>
    /// <param name="mediaKind">Optional kind of the consumed media item.</param>
    /// <param name="mediaItemId">Optional Id of the consumed media item.</param>
    /// <param name="libraryId">Optional Id of the media library that contains the media item.</param>
    /// <param name="isAvailable">Optional value indicating whether the media item is still available.</param>
    /// <param name="title">Optional title of the media item.</param>
    /// <param name="collectionTitle">Optional title of the collection that contains the media item.</param>
    /// <param name="creatorName">Optional name of the creator of the media item.</param>
    /// <param name="coverPath">Optional path of the cover art of the media item.</param>
    /// <param name="lastConsumedOnUtc">Optional date and time when the media item was last consumed.</param>
    /// <param name="countedSessionCount">Optional number of counted sessions.</param>
    /// <param name="completedSessionCount">Optional number of completed sessions.</param>
    /// <param name="consumedDurationInSeconds">Optional consumed duration, in seconds.</param>
    /// <param name="includeCoverPath">Whether the cover path should be included, or forced to <see langword="null"/>.</param>
    /// <returns>A configured <see cref="ConsumedItemDto"/> instance.</returns>
    public ConsumedItemDto Create(
        MediaKind? mediaKind = null,
        Guid? mediaItemId = null,
        Guid? libraryId = null,
        bool? isAvailable = null,
        string? title = null,
        string? collectionTitle = null,
        string? creatorName = null,
        string? coverPath = null,
        DateTime? lastConsumedOnUtc = null,
        int? countedSessionCount = null,
        int? completedSessionCount = null,
        double? consumedDurationInSeconds = null,
        bool includeCoverPath = true)
    {
        return new ConsumedItemDto(
            MediaKind: mediaKind ?? _faker.PickRandom(MediaKind.MusicTrack, MediaKind.PodcastEpisode, MediaKind.Audiobook, MediaKind.Movie, MediaKind.TvEpisode, MediaKind.Book, MediaKind.Photo, MediaKind.Video),
            MediaItemId: mediaItemId ?? _faker.Random.Guid(),
            LibraryId: libraryId ?? _faker.Random.Guid(),
            IsAvailable: isAvailable ?? _faker.Random.Bool(),
            Title: title ?? _faker.Lorem.Sentence(),
            CollectionTitle: collectionTitle ?? _faker.Lorem.Sentence(),
            CreatorName: creatorName ?? _faker.Name.FullName(),
            CoverPath: includeCoverPath ? (coverPath ?? _faker.System.FilePath()) : null,
            LastConsumedOnUtc: lastConsumedOnUtc ?? _faker.Date.Between(new DateTime(2024, 1, 1), new DateTime(2025, 12, 31)),
            CountedSessionCount: countedSessionCount ?? _faker.Random.Int(0, 1000),
            CompletedSessionCount: completedSessionCount ?? _faker.Random.Int(0, 1000),
            ConsumedDurationInSeconds: consumedDurationInSeconds ?? _faker.Random.Double(0, 100000)
        );
    }

    /// <summary>
    /// Creates multiple <see cref="ConsumedItemDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="ConsumedItemDto"/> instances.</returns>
    public List<ConsumedItemDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
