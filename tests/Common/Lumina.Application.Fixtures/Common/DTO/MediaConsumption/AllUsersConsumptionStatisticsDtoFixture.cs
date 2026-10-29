#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DTO.MediaConsumption;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Common.DTO.MediaConsumption;

/// <summary>
/// Fixture class for the <see cref="AllUsersConsumptionStatisticsDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class AllUsersConsumptionStatisticsDtoFixture
{
    private readonly ConsumptionTotalsDtoFixture _consumptionTotalsDtoFixture = new();
    private readonly ConsumptionStatisticsDtoFixture _consumptionStatisticsDtoFixture = new();

    /// <summary>
    /// Creates a random valid <see cref="AllUsersConsumptionStatisticsDto"/>.
    /// </summary>
    /// <param name="total">Optional. The combined statistics of every user. Defaults to a random set of totals.</param>
    /// <param name="users">Optional. The statistics of each user. Defaults to a single random user summary.</param>
    /// <param name="includeUsers">Whether the user summaries should be included, or forced to an empty list.</param>
    /// <returns>The created <see cref="AllUsersConsumptionStatisticsDto"/>.</returns>
    public AllUsersConsumptionStatisticsDto Create(
        ConsumptionTotalsDto? total = null,
        IReadOnlyList<ConsumptionStatisticsDto>? users = null,
        bool includeUsers = true)
    {
        IReadOnlyList<ConsumptionStatisticsDto> resolvedUsers = includeUsers
            ? users ?? [_consumptionStatisticsDtoFixture.Create()]
            : [];

        return new AllUsersConsumptionStatisticsDto(
            total ?? _consumptionTotalsDtoFixture.Create(),
            resolvedUsers);
    }

    /// <summary>
    /// Creates a list of <see cref="AllUsersConsumptionStatisticsDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="AllUsersConsumptionStatisticsDto"/> instances.</returns>
    public List<AllUsersConsumptionStatisticsDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
