#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Responses.MediaConsumption;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Responses.MediaConsumption;

/// <summary>
/// Fixture class for the <see cref="AllUsersConsumptionStatisticsResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class AllUsersConsumptionStatisticsResponseFixture
{
    private readonly ConsumptionTotalsResponseFixture _consumptionTotalsResponseFixture = new();
    private readonly ConsumptionSummaryResponseFixture _consumptionSummaryResponseFixture = new();

    /// <summary>
    /// Creates a random valid <see cref="AllUsersConsumptionStatisticsResponse"/>.
    /// </summary>
    /// <param name="total">Optional. The combined statistics of every user. Defaults to a random set of totals.</param>
    /// <param name="users">Optional. The statistics of each user. Defaults to a single random user summary.</param>
    /// <param name="includeUsers">Whether the user summaries should be included, or forced to an empty list.</param>
    /// <returns>The created <see cref="AllUsersConsumptionStatisticsResponse"/>.</returns>
    public AllUsersConsumptionStatisticsResponse Create(
        ConsumptionTotalsResponse? total = null,
        IReadOnlyList<ConsumptionSummaryResponse>? users = null,
        bool includeUsers = true)
    {
        IReadOnlyList<ConsumptionSummaryResponse> resolvedUsers = includeUsers
            ? users ?? [_consumptionSummaryResponseFixture.Create()]
            : [];

        return new AllUsersConsumptionStatisticsResponse(
            total ?? _consumptionTotalsResponseFixture.Create(),
            resolvedUsers);
    }

    /// <summary>
    /// Creates a list of <see cref="AllUsersConsumptionStatisticsResponse"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="AllUsersConsumptionStatisticsResponse"/> instances.</returns>
    public List<AllUsersConsumptionStatisticsResponse> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
