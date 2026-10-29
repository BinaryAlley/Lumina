#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.Requests.MediaConsumption;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.Requests.MediaConsumption;

/// <summary>
/// Fixture class for the <see cref="CompleteConsumptionRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class CompleteConsumptionRequestFixture
{
    private readonly Faker _faker = new();
    private readonly MediaPositionRequestFixture _mediaPositionRequestFixture = new();

    /// <summary>
    /// Creates a new <see cref="CompleteConsumptionRequest"/> instance with randomized test data.
    /// </summary>
    /// <param name="position">Optional final position reported for the media item.</param>
    /// <param name="progressFraction">Optional fraction of the media item that the client reached, between 0 and 1.</param>
    /// <param name="consumedDurationInSeconds">Optional active duration consumed by the session, in seconds.</param>
    /// <param name="consecutivePagesRead">Optional number of consecutive pages read in the session.</param>
    /// <param name="includePosition">Whether the position should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeConsecutivePagesRead">Whether the number of consecutive pages read should be included, or forced to <see langword="null"/>.</param>
    /// <returns>A configured <see cref="CompleteConsumptionRequest"/> instance.</returns>
    public CompleteConsumptionRequest Create(
        MediaPositionRequest? position = null,
        double? progressFraction = null,
        double? consumedDurationInSeconds = null,
        int? consecutivePagesRead = null,
        bool includePosition = true,
        bool includeConsecutivePagesRead = true)
    {
        return new CompleteConsumptionRequest(
            Position: includePosition ? (position ?? _mediaPositionRequestFixture.Create()) : null,
            ProgressFraction: progressFraction ?? _faker.Random.Double(0, 1),
            ConsumedDurationInSeconds: consumedDurationInSeconds ?? _faker.Random.Double(0, 100000),
            ConsecutivePagesRead: includeConsecutivePagesRead ? (consecutivePagesRead ?? _faker.Random.Int(0, 1000)) : null
        );
    }

    /// <summary>
    /// Creates multiple <see cref="CompleteConsumptionRequest"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="CompleteConsumptionRequest"/> instances.</returns>
    public List<CompleteConsumptionRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
