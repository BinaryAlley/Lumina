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
/// Fixture class for the <see cref="AddScheduledJobRequest"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddScheduledJobRequestFixture
{
    /// <summary>
    /// Creates a random valid request to add a scheduled job.
    /// </summary>
    /// <param name="name">Optional. The name of the scheduled job.</param>
    /// <param name="taskType">Optional. The type of the task executed by the scheduled job.</param>
    /// <param name="scheduleType">Optional. The type of the schedule of the scheduled job.</param>
    /// <param name="intervalMinutes">Optional. The interval in minutes of the schedule.</param>
    /// <param name="hour">Optional. The hour of the schedule.</param>
    /// <param name="minute">Optional. The minute of the schedule.</param>
    /// <returns>The created request.</returns>
    public AddScheduledJobRequest Create(
        string? name = null,
        ScheduledTaskType? taskType = null,
        ScheduleType? scheduleType = null,
        int? intervalMinutes = null,
        int? hour = null,
        int? minute = null)
    {
        return new Faker<AddScheduledJobRequest>()
            .CustomInstantiator(f =>
            {
                ScheduleType resolvedScheduleType = scheduleType ?? f.PickRandom<ScheduleType>();
                if (resolvedScheduleType == ScheduleType.WithIntervalInMinutes)
                {
                    return new AddScheduledJobRequest(
                        name ?? f.Commerce.ProductName(),
                        taskType ?? f.PickRandom<ScheduledTaskType>(),
                        resolvedScheduleType,
                        intervalMinutes ?? f.Random.Int(1, 1440),
                        null,
                        null);
                }
                return new AddScheduledJobRequest(
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
    /// Creates a list of <see cref="AddScheduledJobRequest"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<AddScheduledJobRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
