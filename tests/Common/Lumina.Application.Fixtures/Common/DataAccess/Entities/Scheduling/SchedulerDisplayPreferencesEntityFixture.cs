#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Common.DataAccess.Entities.Scheduling;
using Lumina.Domain.SharedKernel.Common.Enums.Scheduling;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Common.DataAccess.Entities.Scheduling;

/// <summary>
/// Fixture class for the <see cref="SchedulerDisplayPreferencesEntity"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class SchedulerDisplayPreferencesEntityFixture
{
    /// <summary>
    /// Creates a random valid <see cref="SchedulerDisplayPreferencesEntity"/>.
    /// </summary>
    /// <param name="id">Optional. The Id of the display preferences.</param>
    /// <param name="userId">Optional. The Id of the user that owns the display preferences.</param>
    /// <param name="jobTypeFilter">Optional. The type of the scheduled job tasks whose executions are shown.</param>
    /// <param name="displayTimeSpan">Optional. The displayed time span.</param>
    /// <param name="displayTimeUnit">Optional. The unit of the displayed time span.</param>
    /// <returns>The created <see cref="SchedulerDisplayPreferencesEntity"/>.</returns>
    public SchedulerDisplayPreferencesEntity Create(
        Guid? id = null,
        Guid? userId = null,
        ScheduledTaskType? jobTypeFilter = null,
        int? displayTimeSpan = null,
        SchedulerDisplayTimeUnit? displayTimeUnit = null)
    {
        return new Faker<SchedulerDisplayPreferencesEntity>()
            .CustomInstantiator(f => new SchedulerDisplayPreferencesEntity
            {
                Id = id ?? Guid.NewGuid(),
                UserId = userId ?? Guid.NewGuid(),
                JobTypeFilter = jobTypeFilter ?? (f.Random.Bool() ? f.PickRandom<ScheduledTaskType>() : null),
                DisplayTimeSpan = displayTimeSpan ?? f.Random.Int(1, 24 * 60),
                DisplayTimeUnit = displayTimeUnit ?? f.PickRandom<SchedulerDisplayTimeUnit>(),
                CreatedOnUtc = DateTime.UtcNow,
                CreatedBy = Guid.NewGuid(),
                UpdatedOnUtc = null,
                UpdatedBy = null
            })
            .Generate();
    }

    /// <summary>
    /// Creates a list of <see cref="SchedulerDisplayPreferencesEntity"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="SchedulerDisplayPreferencesEntity"/> instances.</returns>
    public List<SchedulerDisplayPreferencesEntity> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
