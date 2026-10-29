#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Common.DTO.MediaConsumption;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Common.DTO.MediaConsumption;

/// <summary>
/// Fixture class for the <see cref="MediaKindConsumptionStatisticsDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class MediaKindConsumptionStatisticsDtoFixture
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
    /// Creates a random valid <see cref="MediaKindConsumptionStatisticsDto"/>.
    /// </summary>
    /// <param name="mediaKind">Optional. The kind of media the statistics belong to. Defaults to a random supported kind.</param>
    /// <param name="totalConsumedDurationMilliseconds">Optional. The total consumed duration of the media kind, in milliseconds. Defaults to a bounded random duration.</param>
    /// <param name="countedSessionCount">Optional. The number of sessions that count towards statistics.</param>
    /// <param name="completedSessionCount">Optional. The number of sessions that completed.</param>
    /// <param name="stoppedSessionCount">Optional. The number of sessions that stopped before they completed.</param>
    /// <param name="completionRate">Optional. The completion rate. Defaults to a bounded random fraction between zero and one.</param>
    /// <returns>The created <see cref="MediaKindConsumptionStatisticsDto"/>.</returns>
    public MediaKindConsumptionStatisticsDto Create(
        MediaKind? mediaKind = null,
        long? totalConsumedDurationMilliseconds = null,
        int? countedSessionCount = null,
        int? completedSessionCount = null,
        int? stoppedSessionCount = null,
        double? completionRate = null)
    {
        return new MediaKindConsumptionStatisticsDto(
            mediaKind ?? _faker.Random.ArrayElement(_supportedMediaKinds),
            totalConsumedDurationMilliseconds ?? _faker.Random.Long(0, MAXIMUM_DURATION_MILLISECONDS),
            countedSessionCount ?? _faker.Random.Int(0, 100),
            completedSessionCount ?? _faker.Random.Int(0, 100),
            stoppedSessionCount ?? _faker.Random.Int(0, 100),
            completionRate ?? _faker.Random.Double(0, 1));
    }

    /// <summary>
    /// Creates a list of <see cref="MediaKindConsumptionStatisticsDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="MediaKindConsumptionStatisticsDto"/> instances.</returns>
    public List<MediaKindConsumptionStatisticsDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
