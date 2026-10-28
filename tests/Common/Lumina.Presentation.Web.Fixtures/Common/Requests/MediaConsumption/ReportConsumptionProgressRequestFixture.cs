#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.Requests.MediaConsumption;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.Requests.MediaConsumption;

/// <summary>
/// Fixture class for the <see cref="ReportConsumptionProgressRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class ReportConsumptionProgressRequestFixture
{
    private readonly Faker _faker = new();
    private readonly MediaPositionRequestFixture _mediaPositionRequestFixture = new();

    /// <summary>
    /// Creates a new <see cref="ReportConsumptionProgressRequest"/> instance with randomized test data.
    /// </summary>
    /// <param name="position">Optional last position reported for the media item.</param>
    /// <param name="progressFraction">Optional fraction of the media item that the client reached, between 0 and 1.</param>
    /// <param name="consumedDurationInSeconds">Optional active duration consumed by the session, in seconds.</param>
    /// <param name="consecutivePagesRead">Optional number of consecutive pages read in the current session.</param>
    /// <param name="includePosition">Whether the position should be included, or forced to <see langword="null"/>.</param>
    /// <returns>A configured <see cref="ReportConsumptionProgressRequest"/> instance.</returns>
    public ReportConsumptionProgressRequest Create(
        MediaPositionRequest? position = null,
        double? progressFraction = null,
        double? consumedDurationInSeconds = null,
        int? consecutivePagesRead = null,
        bool includePosition = true)
    {
        return new ReportConsumptionProgressRequest(
            Position: includePosition ? (position ?? _mediaPositionRequestFixture.Create()) : null,
            ProgressFraction: progressFraction ?? _faker.Random.Double(0, 1),
            ConsumedDurationInSeconds: consumedDurationInSeconds ?? _faker.Random.Double(0, 100000),
            ConsecutivePagesRead: consecutivePagesRead ?? _faker.Random.Int(0, 1000)
        );
    }

    /// <summary>
    /// Creates multiple <see cref="ReportConsumptionProgressRequest"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="ReportConsumptionProgressRequest"/> instances.</returns>
    public List<ReportConsumptionProgressRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
