#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.DTO.MediaConsumption;
using Lumina.Presentation.Web.Common.Enums.MediaConsumption;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.DTO.MediaConsumption;

/// <summary>
/// Fixture class for the <see cref="MediaKindConsumptionSummaryDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class MediaKindConsumptionSummaryDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a new <see cref="MediaKindConsumptionSummaryDto"/> instance with randomized test data.
    /// </summary>
    /// <param name="mediaKind">Optional kind of media the statistics belong to.</param>
    /// <param name="totalConsumedDurationInSeconds">Optional total consumed duration, in seconds.</param>
    /// <param name="countedSessionCount">Optional number of counted sessions.</param>
    /// <param name="completedSessionCount">Optional number of completed sessions.</param>
    /// <param name="stoppedSessionCount">Optional number of stopped sessions.</param>
    /// <param name="completionRate">Optional fraction of the counted sessions that completed.</param>
    /// <returns>A configured <see cref="MediaKindConsumptionSummaryDto"/> instance.</returns>
    public MediaKindConsumptionSummaryDto Create(
        MediaKind? mediaKind = null,
        double? totalConsumedDurationInSeconds = null,
        int? countedSessionCount = null,
        int? completedSessionCount = null,
        int? stoppedSessionCount = null,
        double? completionRate = null)
    {
        return new MediaKindConsumptionSummaryDto(
            MediaKind: mediaKind ?? _faker.PickRandom(MediaKind.MusicTrack, MediaKind.PodcastEpisode, MediaKind.Audiobook, MediaKind.Movie, MediaKind.TvEpisode, MediaKind.Book, MediaKind.Photo, MediaKind.Video),
            TotalConsumedDurationInSeconds: totalConsumedDurationInSeconds ?? _faker.Random.Double(0, 100000),
            CountedSessionCount: countedSessionCount ?? _faker.Random.Int(0, 1000),
            CompletedSessionCount: completedSessionCount ?? _faker.Random.Int(0, 1000),
            StoppedSessionCount: stoppedSessionCount ?? _faker.Random.Int(0, 1000),
            CompletionRate: completionRate ?? _faker.Random.Double(0, 1)
        );
    }

    /// <summary>
    /// Creates multiple <see cref="MediaKindConsumptionSummaryDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="MediaKindConsumptionSummaryDto"/> instances.</returns>
    public List<MediaKindConsumptionSummaryDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
