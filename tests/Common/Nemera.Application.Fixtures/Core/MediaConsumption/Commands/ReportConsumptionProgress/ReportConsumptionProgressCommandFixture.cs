#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Core.MediaConsumption.Commands.ReportConsumptionProgress;
using Lumina.Domain.SharedKernel.Common.Enums.MediaConsumption;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.MediaConsumption.Commands.ReportConsumptionProgress;

/// <summary>
/// Fixture class for the <see cref="ReportConsumptionProgressCommand"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ReportConsumptionProgressCommandFixture
{
    private const double MAXIMUM_OFFSET_SECONDS = 7_200;
    private const double MAXIMUM_DURATION_SECONDS = 36_000;

    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="ReportConsumptionProgressCommand"/>.
    /// </summary>
    /// <param name="consumptionRecordId">Optional. The Id of the consumption record, taken from the route.</param>
    /// <param name="positionKind">Optional. The name of the kind of the reported position. Defaults to a time offset.</param>
    /// <param name="positionTimeOffsetInSeconds">Optional. The reported position, in seconds, when it is a time offset. Defaults to a bounded non-negative offset for a time offset position.</param>
    /// <param name="positionLocation">Optional. The reported opaque locator, when the position is not a time offset. Defaults to a random word for a location position.</param>
    /// <param name="progressFraction">Optional. The reported progress fraction. Defaults to a bounded random fraction between zero and one.</param>
    /// <param name="consumedDurationInSeconds">Optional. The active duration consumed by the session, in seconds. Defaults to a bounded random duration.</param>
    /// <param name="consecutivePagesRead">Optional. The number of consecutive pages read in the current session. Defaults to zero.</param>
    /// <param name="includeConsumptionRecordId">Whether the consumption record Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includePositionKind">Whether the position kind should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includePositionTimeOffsetInSeconds">Whether the time offset should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includePositionLocation">Whether the location should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeProgressFraction">Whether the progress fraction should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeConsumedDurationInSeconds">Whether the consumed duration should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="ReportConsumptionProgressCommand"/>.</returns>
    public ReportConsumptionProgressCommand Create(
        string? consumptionRecordId = null,
        string? positionKind = null,
        double? positionTimeOffsetInSeconds = null,
        string? positionLocation = null,
        double? progressFraction = null,
        double? consumedDurationInSeconds = null,
        int consecutivePagesRead = 0,
        bool includeConsumptionRecordId = true,
        bool includePositionKind = true,
        bool includePositionTimeOffsetInSeconds = true,
        bool includePositionLocation = true,
        bool includeProgressFraction = true,
        bool includeConsumedDurationInSeconds = true)
    {
        string? resolvedConsumptionRecordId = includeConsumptionRecordId ? consumptionRecordId ?? _faker.Random.Guid().ToString() : null;
        string? resolvedPositionKind = includePositionKind ? positionKind ?? nameof(MediaPositionKind.TimeOffset) : null;
        bool isTimeOffset = resolvedPositionKind == nameof(MediaPositionKind.TimeOffset);
        bool isLocation = resolvedPositionKind == nameof(MediaPositionKind.Location);
        double? resolvedPositionTimeOffsetInSeconds = includePositionTimeOffsetInSeconds
            ? positionTimeOffsetInSeconds ?? (isTimeOffset ? _faker.Random.Double(0, MAXIMUM_OFFSET_SECONDS) : (double?)null)
            : null;
        string? resolvedPositionLocation = includePositionLocation
            ? positionLocation ?? (isLocation ? _faker.Lorem.Word() : (string?)null)
            : null;
        double? resolvedProgressFraction = includeProgressFraction ? progressFraction ?? _faker.Random.Double(0, 1) : null;
        double? resolvedConsumedDurationInSeconds = includeConsumedDurationInSeconds ? consumedDurationInSeconds ?? _faker.Random.Double(0, MAXIMUM_DURATION_SECONDS) : null;

        return new ReportConsumptionProgressCommand(
            resolvedConsumptionRecordId,
            resolvedPositionKind,
            resolvedPositionTimeOffsetInSeconds,
            resolvedPositionLocation,
            resolvedProgressFraction,
            resolvedConsumedDurationInSeconds,
            consecutivePagesRead);
    }

    /// <summary>
    /// Creates a list of <see cref="ReportConsumptionProgressCommand"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="ReportConsumptionProgressCommand"/> instances.</returns>
    public List<ReportConsumptionProgressCommand> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
