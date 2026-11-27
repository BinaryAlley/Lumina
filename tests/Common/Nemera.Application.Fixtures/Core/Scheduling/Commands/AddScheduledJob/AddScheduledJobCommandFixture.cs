#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Core.Scheduling.Commands.AddScheduledJob;
using Lumina.Domain.SharedKernel.Common.Enums.Scheduling;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.Scheduling.Commands.AddScheduledJob;

/// <summary>
/// Fixture class for the <see cref="AddScheduledJobCommand"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddScheduledJobCommandFixture
{
    /// <summary>
    /// Creates a random valid command to add a scheduled job.
    /// </summary>
    /// <param name="name">Optional. The name of the scheduled job.</param>
    /// <param name="taskType">Optional. The type of the task executed by the scheduled job.</param>
    /// <param name="scheduleType">Optional. The type of the schedule of the scheduled job.</param>
    /// <param name="intervalMinutes">Optional. The interval in minutes of the schedule.</param>
    /// <param name="hour">Optional. The hour of the schedule.</param>
    /// <param name="minute">Optional. The minute of the schedule.</param>
    /// <returns>The created command.</returns>
    public AddScheduledJobCommand Create(
        string? name = null,
        ScheduledTaskType? taskType = null,
        ScheduleType? scheduleType = null,
        int? intervalMinutes = null,
        int? hour = null,
        int? minute = null)
    {
        return new Faker<AddScheduledJobCommand>()
            .CustomInstantiator(f =>
            {
                ScheduleType resolvedScheduleType = scheduleType ?? f.PickRandom<ScheduleType>();
                if (resolvedScheduleType == ScheduleType.WithIntervalInMinutes)
                {
                    return new AddScheduledJobCommand(
                        name ?? f.Commerce.ProductName(),
                        taskType ?? f.PickRandom<ScheduledTaskType>(),
                        resolvedScheduleType,
                        intervalMinutes ?? f.Random.Int(1, 1440),
                        null,
                        null);
                }
                return new AddScheduledJobCommand(
                    name ?? f.Commerce.ProductName(),
                    taskType ?? f.PickRandom<ScheduledTaskType>(),
                    resolvedScheduleType,
                    null,
                    hour ?? f.Random.Int(0, 23),
                    minute ?? f.Random.Int(0, 59));
            })
            .Generate();
    }

    /// <summary>
    /// Creates a list of <see cref="AddScheduledJobCommand"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<AddScheduledJobCommand> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
