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
/// Fixture class for the <see cref="ScheduledJobResponse"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ScheduledJobResponseFixture
{
    /// <summary>
    /// Creates a random valid <see cref="ScheduledJobResponse"/>.
    /// </summary>
    /// <param name="id">Optional. The Id of the scheduled job.</param>
    /// <param name="name">Optional. The name of the scheduled job.</param>
    /// <param name="taskType">Optional. The type of the task executed by the scheduled job.</param>
    /// <param name="scheduleType">Optional. The type of the schedule of the scheduled job.</param>
    /// <param name="intervalMinutes">Optional. The interval in minutes of the schedule.</param>
    /// <param name="hour">Optional. The hour of the schedule.</param>
    /// <param name="minute">Optional. The minute of the schedule.</param>
    /// <param name="status">Optional. The status of the scheduled job.</param>
    /// <param name="lastStartedOnUtc">Optional. The last start time of the scheduled job.</param>
    /// <param name="lastCompletedOnUtc">Optional. The last completion time of the scheduled job.</param>
    /// <returns>The created <see cref="ScheduledJobResponse"/>.</returns>
    public ScheduledJobResponse Create(
        Guid? id = null,
        string? name = null,
        ScheduledTaskType? taskType = null,
        ScheduleType? scheduleType = null,
        int? intervalMinutes = null,
        int? hour = null,
        int? minute = null,
        ScheduledJobStatus? status = null,
        DateTime? lastStartedOnUtc = null,
        DateTime? lastCompletedOnUtc = null)
    {
        return new Faker<ScheduledJobResponse>()
            .CustomInstantiator(f =>
            {
                ScheduleType resolvedScheduleType = scheduleType ?? f.PickRandom<ScheduleType>();
                if (resolvedScheduleType == ScheduleType.WithIntervalInMinutes)
                {
                    return new ScheduledJobResponse(
                        id ?? Guid.NewGuid(),
                        name ?? f.Commerce.ProductName(),
                        taskType ?? f.PickRandom<ScheduledTaskType>(),
                        resolvedScheduleType,
                        intervalMinutes ?? f.Random.Int(1, 1440),
                        null,
                        null,
                        status ?? f.PickRandom<ScheduledJobStatus>(),
                        lastStartedOnUtc,
                        lastCompletedOnUtc);
                }
                return new ScheduledJobResponse(
                    id ?? Guid.NewGuid(),
                    name ?? f.Commerce.ProductName(),
                    taskType ?? f.PickRandom<ScheduledTaskType>(),
                    resolvedScheduleType,
                    null,
                    hour ?? f.Random.Int(0, 23),
                    minute ?? f.Random.Int(0, 59),
                    status ?? f.PickRandom<ScheduledJobStatus>(),
                    lastStartedOnUtc,
                    lastCompletedOnUtc);
            })
            .Generate();
    }

    /// <summary>
    /// Creates a list of <see cref="ScheduledJobResponse"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="ScheduledJobResponse"/> instances.</returns>
    public List<ScheduledJobResponse> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
