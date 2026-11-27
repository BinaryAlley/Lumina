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
/// Fixture class for the <see cref="ConsumptionRecordDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class ConsumptionRecordDtoFixture
{
    private readonly Faker _faker = new();
    private readonly MediaPositionDtoFixture _mediaPositionDtoFixture = new();

    /// <summary>
    /// Creates a new <see cref="ConsumptionRecordDto"/> instance with randomized test data.
    /// </summary>
    /// <param name="id">Optional Id of the consumption record.</param>
    /// <param name="libraryId">Optional Id of the media library that contains the consumed media item.</param>
    /// <param name="mediaItemId">Optional Id of the consumed media item.</param>
    /// <param name="mediaKind">Optional kind of media that was consumed.</param>
    /// <param name="status">Optional state of the consumption session.</param>
    /// <param name="startedOnUtc">Optional date and time when the session started.</param>
    /// <param name="lastActivityOnUtc">Optional date and time when the session last received activity.</param>
    /// <param name="endedOnUtc">Optional date and time when the session completed or stopped.</param>
    /// <param name="consumedDurationInSeconds">Optional active duration consumed by the session, in seconds.</param>
    /// <param name="lastPosition">Optional last known position for the media item.</param>
    /// <param name="lastProgressFraction">Optional latest reported progress fraction of the media item.</param>
    /// <param name="consecutivePagesRead">Optional number of consecutive pages read in the session.</param>
    /// <param name="includeEndedOnUtc">Whether the end date should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeLastPosition">Whether the last position should be included, or forced to <see langword="null"/>.</param>
    /// <returns>A configured <see cref="ConsumptionRecordDto"/> instance.</returns>
    public ConsumptionRecordDto Create(
        Guid? id = null,
        Guid? libraryId = null,
        Guid? mediaItemId = null,
        MediaKind? mediaKind = null,
        ConsumptionStatus? status = null,
        DateTime? startedOnUtc = null,
        DateTime? lastActivityOnUtc = null,
        DateTime? endedOnUtc = null,
        double? consumedDurationInSeconds = null,
        MediaPositionDto? lastPosition = null,
        double? lastProgressFraction = null,
        int? consecutivePagesRead = null,
        bool includeEndedOnUtc = true,
        bool includeLastPosition = true)
    {
        DateTime resolvedStartedOnUtc = DateTime.SpecifyKind(startedOnUtc ?? DateTime.UtcNow.AddDays(-_faker.Random.Int(1, 30)), DateTimeKind.Utc);
        DateTime resolvedLastActivityOnUtc = DateTime.SpecifyKind(lastActivityOnUtc ?? resolvedStartedOnUtc.AddSeconds(_faker.Random.Int(0, 3600)), DateTimeKind.Utc);
        DateTime resolvedEndedOnUtc = DateTime.SpecifyKind(endedOnUtc ?? resolvedLastActivityOnUtc.AddSeconds(_faker.Random.Int(0, 3600)), DateTimeKind.Utc);
        return new ConsumptionRecordDto(
            Id: id ?? _faker.Random.Guid(),
            LibraryId: libraryId ?? _faker.Random.Guid(),
            MediaItemId: mediaItemId ?? _faker.Random.Guid(),
            MediaKind: mediaKind ?? _faker.PickRandom(MediaKind.MusicTrack, MediaKind.PodcastEpisode, MediaKind.Audiobook, MediaKind.Movie, MediaKind.TvEpisode, MediaKind.Book, MediaKind.Photo, MediaKind.Video),
            Status: status ?? _faker.PickRandom(ConsumptionStatus.InProgress, ConsumptionStatus.Completed, ConsumptionStatus.Stopped),
            StartedOnUtc: resolvedStartedOnUtc,
            LastActivityOnUtc: resolvedLastActivityOnUtc,
            EndedOnUtc: includeEndedOnUtc ? resolvedEndedOnUtc : null,
            ConsumedDurationInSeconds: consumedDurationInSeconds ?? _faker.Random.Double(0, 100000),
            LastPosition: includeLastPosition ? (lastPosition ?? _mediaPositionDtoFixture.Create()) : null,
            LastProgressFraction: lastProgressFraction ?? _faker.Random.Double(0, 1),
            ConsecutivePagesRead: consecutivePagesRead ?? _faker.Random.Int(0, 1000)
        );
    }

    /// <summary>
    /// Creates multiple <see cref="ConsumptionRecordDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="ConsumptionRecordDto"/> instances.</returns>
    public List<ConsumptionRecordDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
