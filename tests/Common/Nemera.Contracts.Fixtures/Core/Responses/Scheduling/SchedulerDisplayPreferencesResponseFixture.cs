#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.Responses.Scheduling;
using Lumina.Domain.SharedKernel.Common.Enums.Scheduling;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Responses.Scheduling;

/// <summary>
/// Fixture class for the <see cref="SchedulerDisplayPreferencesResponse"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class SchedulerDisplayPreferencesResponseFixture
{
    /// <summary>
    /// Creates a random valid <see cref="SchedulerDisplayPreferencesResponse"/>.
    /// </summary>
    /// <param name="userId">Optional. The Id of the user that owns the display preferences.</param>
    /// <param name="jobTypeFilter">Optional. The type of the scheduled job tasks whose executions are shown.</param>
    /// <param name="displayTimeSpan">Optional. The displayed time span.</param>
    /// <param name="displayTimeUnit">Optional. The unit of the displayed time span.</param>
    /// <returns>The created <see cref="SchedulerDisplayPreferencesResponse"/>.</returns>
    public SchedulerDisplayPreferencesResponse Create(
        Guid? userId = null,
        ScheduledTaskType? jobTypeFilter = null,
        int? displayTimeSpan = null,
        SchedulerDisplayTimeUnit? displayTimeUnit = null)
    {
        return new Faker<SchedulerDisplayPreferencesResponse>()
            .CustomInstantiator(f => new SchedulerDisplayPreferencesResponse(
                userId ?? Guid.NewGuid(),
                jobTypeFilter ?? (f.Random.Bool() ? f.PickRandom<ScheduledTaskType>() : null),
                displayTimeSpan ?? f.Random.Int(1, 24 * 60),
                displayTimeUnit ?? f.PickRandom<SchedulerDisplayTimeUnit>()))
            .Generate();
    }

    /// <summary>
    /// Creates a list of <see cref="SchedulerDisplayPreferencesResponse"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="SchedulerDisplayPreferencesResponse"/> instances.</returns>
    public List<SchedulerDisplayPreferencesResponse> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
