#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.Requests.MediaConsumption;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Requests.MediaConsumption;

/// <summary>
/// Fixture class for the <see cref="StopConsumptionRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class StopConsumptionRequestFixture
{
    private const double MAXIMUM_DURATION_SECONDS = 36_000;

    private readonly Faker _faker = new();
    private readonly MediaPositionRequestFixture _mediaPositionRequestFixture = new();

    /// <summary>
    /// Creates a random valid <see cref="StopConsumptionRequest"/>.
    /// </summary>
    /// <param name="position">Optional. The final position reported for the media item.</param>
    /// <param name="includePosition">Whether the position should be included, or forced to <see langword="null"/>.</param>
    /// <param name="progressFraction">Optional. The fraction of the media item that the client reached. Defaults to a bounded random fraction between zero and one.</param>
    /// <param name="includeProgressFraction">Whether the progress fraction should be included, or forced to <see langword="null"/>.</param>
    /// <param name="consumedDurationInSeconds">Optional. The active duration consumed by the session, in seconds. Defaults to a bounded random duration.</param>
    /// <param name="includeConsumedDurationInSeconds">Whether the consumed duration should be included, or forced to <see langword="null"/>.</param>
    /// <param name="consecutivePagesRead">Optional. The number of consecutive pages read in the session.</param>
    /// <param name="includeConsecutivePagesRead">Whether the optional consecutive pages argument should be supplied at all. When <see langword="false"/>, the argument is omitted, so the record default applies.</param>
    /// <returns>The created <see cref="StopConsumptionRequest"/>.</returns>
    public StopConsumptionRequest Create(
        MediaPositionRequest? position = null,
        bool includePosition = true,
        double? progressFraction = null,
        bool includeProgressFraction = true,
        double? consumedDurationInSeconds = null,
        bool includeConsumedDurationInSeconds = true,
        int? consecutivePagesRead = null,
        bool includeConsecutivePagesRead = true)
    {
        MediaPositionRequest? resolvedPosition = includePosition ? position ?? _mediaPositionRequestFixture.Create() : null;
        double? resolvedProgressFraction = includeProgressFraction ? progressFraction ?? _faker.Random.Double(0, 1) : null;
        double? resolvedConsumedDurationInSeconds = includeConsumedDurationInSeconds ? consumedDurationInSeconds ?? _faker.Random.Double(0, MAXIMUM_DURATION_SECONDS) : null;
        if (!includeConsecutivePagesRead)
            return new StopConsumptionRequest(resolvedPosition, resolvedProgressFraction, resolvedConsumedDurationInSeconds);
        return new StopConsumptionRequest(resolvedPosition, resolvedProgressFraction, resolvedConsumedDurationInSeconds, consecutivePagesRead);
    }

    /// <summary>
    /// Creates a list of <see cref="StopConsumptionRequest"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="StopConsumptionRequest"/> instances.</returns>
    public List<StopConsumptionRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
