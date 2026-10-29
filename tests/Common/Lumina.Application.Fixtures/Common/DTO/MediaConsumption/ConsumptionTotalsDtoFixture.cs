#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Common.DTO.MediaConsumption;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Common.DTO.MediaConsumption;

/// <summary>
/// Fixture class for the <see cref="ConsumptionTotalsDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class ConsumptionTotalsDtoFixture
{
    private const int MAXIMUM_DURATION_MILLISECONDS = 36_000_000;

    private readonly Faker _faker = new();
    private readonly MediaKindConsumptionStatisticsDtoFixture _mediaKindConsumptionStatisticsDtoFixture = new();

    /// <summary>
    /// Creates a random valid <see cref="ConsumptionTotalsDto"/>.
    /// </summary>
    /// <param name="totalConsumedDurationMilliseconds">Optional. The total consumed duration, in milliseconds. Defaults to a bounded random duration.</param>
    /// <param name="countedSessionCount">Optional. The number of sessions that count towards statistics.</param>
    /// <param name="completedSessionCount">Optional. The number of sessions that completed.</param>
    /// <param name="stoppedSessionCount">Optional. The number of sessions that stopped before they completed.</param>
    /// <param name="completionRate">Optional. The completion rate. Defaults to a bounded random fraction between zero and one.</param>
    /// <param name="mediaKindSummaries">Optional. The statistics broken down by media kind. Defaults to a single random summary.</param>
    /// <param name="includeMediaKindSummaries">Whether the media kind summaries should be included, or forced to an empty list.</param>
    /// <returns>The created <see cref="ConsumptionTotalsDto"/>.</returns>
    public ConsumptionTotalsDto Create(
        long? totalConsumedDurationMilliseconds = null,
        int? countedSessionCount = null,
        int? completedSessionCount = null,
        int? stoppedSessionCount = null,
        double? completionRate = null,
        IReadOnlyList<MediaKindConsumptionStatisticsDto>? mediaKindSummaries = null,
        bool includeMediaKindSummaries = true)
    {
        IReadOnlyList<MediaKindConsumptionStatisticsDto> resolvedMediaKindSummaries = includeMediaKindSummaries
            ? mediaKindSummaries ?? [_mediaKindConsumptionStatisticsDtoFixture.Create()]
            : [];

        return new ConsumptionTotalsDto(
            totalConsumedDurationMilliseconds ?? _faker.Random.Long(0, MAXIMUM_DURATION_MILLISECONDS),
            countedSessionCount ?? _faker.Random.Int(0, 100),
            completedSessionCount ?? _faker.Random.Int(0, 100),
            stoppedSessionCount ?? _faker.Random.Int(0, 100),
            completionRate ?? _faker.Random.Double(0, 1),
            resolvedMediaKindSummaries);
    }

    /// <summary>
    /// Creates a list of <see cref="ConsumptionTotalsDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="ConsumptionTotalsDto"/> instances.</returns>
    public List<ConsumptionTotalsDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
