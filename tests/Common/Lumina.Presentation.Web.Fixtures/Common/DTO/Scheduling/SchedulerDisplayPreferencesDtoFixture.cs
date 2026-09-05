#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.DTO.Scheduling;
using Lumina.Presentation.Web.Common.Enums.Scheduling;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.DTO.Scheduling;

/// <summary>
/// Fixture class for generating <see cref="SchedulerDisplayPreferencesDto"/> test data.
/// </summary>
[ExcludeFromCodeCoverage]
public class SchedulerDisplayPreferencesDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a new <see cref="SchedulerDisplayPreferencesDto"/> instance with randomized test data.
    /// </summary>
    /// <param name="userId">Optional unique identifier of the user that owns the display preferences.</param>
    /// <param name="jobTypeFilter">Optional type of the scheduled job tasks whose executions are shown on the scheduler page.</param>
    /// <param name="displayTimeSpan">Optional time span, expressed in <paramref name="displayTimeUnit"/>, that the scheduler page shows.</param>
    /// <param name="displayTimeUnit">Optional unit in which the displayed time span of the scheduler page is expressed.</param>
    /// <returns>A configured <see cref="SchedulerDisplayPreferencesDto"/> instance.</returns>
    public SchedulerDisplayPreferencesDto Create(
        Guid? userId = null,
        ScheduledTaskType? jobTypeFilter = null,
        int? displayTimeSpan = null,
        SchedulerDisplayTimeUnit? displayTimeUnit = null)
    {
        return new SchedulerDisplayPreferencesDto(
            UserId: userId ?? Guid.NewGuid(),
            JobTypeFilter: jobTypeFilter,
            DisplayTimeSpan: displayTimeSpan ?? _faker.Random.Int(1, 30),
            DisplayTimeUnit: displayTimeUnit ?? _faker.PickRandom<SchedulerDisplayTimeUnit>());
    }

    /// <summary>
    /// Creates multiple <see cref="SchedulerDisplayPreferencesDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="SchedulerDisplayPreferencesDto"/> instances.</returns>
    public List<SchedulerDisplayPreferencesDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
