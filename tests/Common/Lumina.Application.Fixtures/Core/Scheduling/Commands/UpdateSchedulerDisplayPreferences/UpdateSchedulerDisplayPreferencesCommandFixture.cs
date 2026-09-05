#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Core.Scheduling.Commands.UpdateSchedulerDisplayPreferences;
using Lumina.Domain.SharedKernel.Common.Enums.Scheduling;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.Scheduling.Commands.UpdateSchedulerDisplayPreferences;

/// <summary>
/// Fixture class for the <see cref="UpdateSchedulerDisplayPreferencesCommand"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateSchedulerDisplayPreferencesCommandFixture
{
    /// <summary>
    /// Creates a random valid command to update the display preferences of the scheduler page of the current user.
    /// </summary>
    /// <param name="jobTypeFilter">Optional. The type of the scheduled job tasks whose executions are shown.</param>
    /// <param name="displayTimeSpan">Optional. The displayed time span.</param>
    /// <param name="displayTimeUnit">Optional. The unit of the displayed time span.</param>
    /// <returns>The created command.</returns>
    public UpdateSchedulerDisplayPreferencesCommand Create(
        ScheduledTaskType? jobTypeFilter = null,
        int? displayTimeSpan = null,
        SchedulerDisplayTimeUnit? displayTimeUnit = null)
    {
        return new Faker<UpdateSchedulerDisplayPreferencesCommand>()
            .CustomInstantiator(f => new UpdateSchedulerDisplayPreferencesCommand(
                jobTypeFilter ?? (f.Random.Bool() ? f.PickRandom<ScheduledTaskType>() : null),
                displayTimeSpan ?? f.Random.Int(1, 24 * 60),
                displayTimeUnit ?? f.PickRandom<SchedulerDisplayTimeUnit>()))
            .Generate();
    }

    /// <summary>
    /// Creates a list of <see cref="UpdateSchedulerDisplayPreferencesCommand"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<UpdateSchedulerDisplayPreferencesCommand> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
