#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.Enums.Scheduling;
using Lumina.Presentation.Web.Common.Requests.Scheduling;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.Requests.Scheduling;

/// <summary>
/// Fixture class for generating <see cref="UpdateSchedulerDisplayPreferencesRequest"/> test data.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateSchedulerDisplayPreferencesRequestFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a new <see cref="UpdateSchedulerDisplayPreferencesRequest"/> instance with randomized test data.
    /// </summary>
    /// <param name="jobTypeFilter">Optional type of the scheduled job tasks whose executions are shown on the scheduler page.</param>
    /// <param name="displayTimeSpan">Optional time span, expressed in <paramref name="displayTimeUnit"/>, that the scheduler page shows.</param>
    /// <param name="displayTimeUnit">Optional unit in which the displayed time span of the scheduler page is expressed.</param>
    /// <returns>A configured <see cref="UpdateSchedulerDisplayPreferencesRequest"/> instance.</returns>
    public UpdateSchedulerDisplayPreferencesRequest Create(
        ScheduledTaskType? jobTypeFilter = null,
        int? displayTimeSpan = null,
        SchedulerDisplayTimeUnit? displayTimeUnit = null)
    {
        return new UpdateSchedulerDisplayPreferencesRequest(
            JobTypeFilter: jobTypeFilter,
            DisplayTimeSpan: displayTimeSpan ?? _faker.Random.Int(1, 30),
            DisplayTimeUnit: displayTimeUnit ?? _faker.PickRandom<SchedulerDisplayTimeUnit>());
    }

    /// <summary>
    /// Creates multiple <see cref="UpdateSchedulerDisplayPreferencesRequest"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="UpdateSchedulerDisplayPreferencesRequest"/> instances.</returns>
    public List<UpdateSchedulerDisplayPreferencesRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
