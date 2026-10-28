#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.MediaConsumption;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.DTO.MediaConsumption;

/// <summary>
/// Fixture class for the <see cref="AllUsersConsumptionStatisticsDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class AllUsersConsumptionStatisticsDtoFixture
{
    private readonly ConsumptionTotalsDtoFixture _consumptionTotalsDtoFixture = new();
    private readonly ConsumptionSummaryDtoFixture _consumptionSummaryDtoFixture = new();

    /// <summary>
    /// Creates a new <see cref="AllUsersConsumptionStatisticsDto"/> instance with randomized test data.
    /// </summary>
    /// <param name="total">Optional combined consumption statistics of every user.</param>
    /// <param name="users">Optional consumption statistics of each user.</param>
    /// <param name="includeUsers">Whether the per user statistics should be included, or forced to an empty collection.</param>
    /// <returns>A configured <see cref="AllUsersConsumptionStatisticsDto"/> instance.</returns>
    public AllUsersConsumptionStatisticsDto Create(
        ConsumptionTotalsDto? total = null,
        IReadOnlyList<ConsumptionSummaryDto>? users = null,
        bool includeUsers = true)
    {
        return new AllUsersConsumptionStatisticsDto(
            Total: total ?? _consumptionTotalsDtoFixture.Create(),
            Users: includeUsers ? (users ?? [.. _consumptionSummaryDtoFixture.CreateMany(2)]) : []
        );
    }

    /// <summary>
    /// Creates multiple <see cref="AllUsersConsumptionStatisticsDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="AllUsersConsumptionStatisticsDto"/> instances.</returns>
    public List<AllUsersConsumptionStatisticsDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
