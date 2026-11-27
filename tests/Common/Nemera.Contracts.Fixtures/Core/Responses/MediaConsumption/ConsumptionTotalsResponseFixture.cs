#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.Responses.MediaConsumption;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Responses.MediaConsumption;

/// <summary>
/// Fixture class for the <see cref="ConsumptionTotalsResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class ConsumptionTotalsResponseFixture
{
    private const double MAXIMUM_DURATION_SECONDS = 36_000;

    private readonly Faker _faker = new();
    private readonly MediaKindConsumptionSummaryResponseFixture _mediaKindConsumptionSummaryResponseFixture = new();

    /// <summary>
    /// Creates a random valid <see cref="ConsumptionTotalsResponse"/>.
    /// </summary>
    /// <param name="totalConsumedDurationInSeconds">Optional. The total consumed duration, in seconds. Defaults to a bounded random duration.</param>
    /// <param name="countedSessionCount">Optional. The number of sessions that count towards statistics.</param>
    /// <param name="completedSessionCount">Optional. The number of sessions that completed.</param>
    /// <param name="stoppedSessionCount">Optional. The number of sessions that stopped before they completed.</param>
    /// <param name="completionRate">Optional. The completion rate. Defaults to a bounded random fraction between zero and one.</param>
    /// <param name="mediaKindSummaries">Optional. The statistics broken down by media kind. Defaults to a single random summary.</param>
    /// <param name="includeMediaKindSummaries">Whether the media kind summaries should be included, or forced to an empty list.</param>
    /// <returns>The created <see cref="ConsumptionTotalsResponse"/>.</returns>
    public ConsumptionTotalsResponse Create(
        double? totalConsumedDurationInSeconds = null,
        int? countedSessionCount = null,
        int? completedSessionCount = null,
        int? stoppedSessionCount = null,
        double? completionRate = null,
        IReadOnlyList<MediaKindConsumptionSummaryResponse>? mediaKindSummaries = null,
        bool includeMediaKindSummaries = true)
    {
        IReadOnlyList<MediaKindConsumptionSummaryResponse> resolvedMediaKindSummaries = includeMediaKindSummaries
            ? mediaKindSummaries ?? [_mediaKindConsumptionSummaryResponseFixture.Create()]
            : [];

        return new ConsumptionTotalsResponse(
            totalConsumedDurationInSeconds ?? _faker.Random.Double(0, MAXIMUM_DURATION_SECONDS),
            countedSessionCount ?? _faker.Random.Int(0, 100),
            completedSessionCount ?? _faker.Random.Int(0, 100),
            stoppedSessionCount ?? _faker.Random.Int(0, 100),
            completionRate ?? _faker.Random.Double(0, 1),
            resolvedMediaKindSummaries);
    }

    /// <summary>
    /// Creates a list of <see cref="ConsumptionTotalsResponse"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="ConsumptionTotalsResponse"/> instances.</returns>
    public List<ConsumptionTotalsResponse> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
