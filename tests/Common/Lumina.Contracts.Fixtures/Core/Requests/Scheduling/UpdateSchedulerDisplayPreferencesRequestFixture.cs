#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.Requests.Scheduling;
using Lumina.Domain.SharedKernel.Common.Enums.Scheduling;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Requests.Scheduling;

/// <summary>
/// Fixture class for the <see cref="UpdateSchedulerDisplayPreferencesRequest"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateSchedulerDisplayPreferencesRequestFixture
{
    /// <summary>
    /// Creates a random valid request to update the display preferences of the scheduler page of the current user.
    /// </summary>
    /// <param name="jobTypeFilter">Optional. The type of the scheduled job tasks whose executions are shown.</param>
    /// <param name="displayTimeSpan">Optional. The displayed time span.</param>
    /// <param name="displayTimeUnit">Optional. The unit of the displayed time span.</param>
    /// <returns>The created request.</returns>
    public UpdateSchedulerDisplayPreferencesRequest Create(
        ScheduledTaskType? jobTypeFilter = null,
        int? displayTimeSpan = null,
        SchedulerDisplayTimeUnit? displayTimeUnit = null)
    {
        return new Faker<UpdateSchedulerDisplayPreferencesRequest>()
            .CustomInstantiator(f => new UpdateSchedulerDisplayPreferencesRequest(
                jobTypeFilter ?? (f.Random.Bool() ? f.PickRandom<ScheduledTaskType>() : null),
                displayTimeSpan ?? f.Random.Int(1, 24 * 60),
                displayTimeUnit ?? f.PickRandom<SchedulerDisplayTimeUnit>()))
            .Generate();
    }

    /// <summary>
    /// Creates a list of <see cref="UpdateSchedulerDisplayPreferencesRequest"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<UpdateSchedulerDisplayPreferencesRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
