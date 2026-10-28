#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.DTO.MediaConsumption;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.DTO.MediaConsumption;

/// <summary>
/// Fixture class for the <see cref="ConsumptionSummaryDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class ConsumptionSummaryDtoFixture
{
    private readonly Faker _faker = new();
    private readonly MediaKindConsumptionSummaryDtoFixture _mediaKindConsumptionSummaryDtoFixture = new();

    /// <summary>
    /// Creates a new <see cref="ConsumptionSummaryDto"/> instance with randomized test data.
    /// </summary>
    /// <param name="userId">Optional Id of the user the statistics belong to.</param>
    /// <param name="totalConsumedDurationInSeconds">Optional total consumed duration, in seconds.</param>
    /// <param name="countedSessionCount">Optional number of counted sessions.</param>
    /// <param name="completedSessionCount">Optional number of completed sessions.</param>
    /// <param name="stoppedSessionCount">Optional number of stopped sessions.</param>
    /// <param name="completionRate">Optional fraction of the counted sessions that completed.</param>
    /// <param name="mediaKindSummaries">Optional consumption statistics broken down by media kind.</param>
    /// <param name="includeMediaKindSummaries">Whether the media kind summaries should be included, or forced to an empty collection.</param>
    /// <returns>A configured <see cref="ConsumptionSummaryDto"/> instance.</returns>
    public ConsumptionSummaryDto Create(
        Guid? userId = null,
        double? totalConsumedDurationInSeconds = null,
        int? countedSessionCount = null,
        int? completedSessionCount = null,
        int? stoppedSessionCount = null,
        double? completionRate = null,
        IReadOnlyList<MediaKindConsumptionSummaryDto>? mediaKindSummaries = null,
        bool includeMediaKindSummaries = true)
    {
        return new ConsumptionSummaryDto(
            UserId: userId ?? _faker.Random.Guid(),
            TotalConsumedDurationInSeconds: totalConsumedDurationInSeconds ?? _faker.Random.Double(0, 100000),
            CountedSessionCount: countedSessionCount ?? _faker.Random.Int(0, 1000),
            CompletedSessionCount: completedSessionCount ?? _faker.Random.Int(0, 1000),
            StoppedSessionCount: stoppedSessionCount ?? _faker.Random.Int(0, 1000),
            CompletionRate: completionRate ?? _faker.Random.Double(0, 1),
            MediaKindSummaries: includeMediaKindSummaries ? (mediaKindSummaries ?? [.. _mediaKindConsumptionSummaryDtoFixture.CreateMany(2)]) : []
        );
    }

    /// <summary>
    /// Creates multiple <see cref="ConsumptionSummaryDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="ConsumptionSummaryDto"/> instances.</returns>
    public List<ConsumptionSummaryDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
