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
/// Fixture class for the <see cref="ScheduledJobEntity"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ScheduledJobEntityFixture
{
    /// <summary>
    /// Creates a random valid <see cref="ScheduledJobEntity"/>.
    /// </summary>
    /// <param name="id">Optional. The Id of the scheduled job.</param>
    /// <param name="name">Optional. The name of the scheduled job.</param>
    /// <param name="taskType">Optional. The type of the task executed by the scheduled job.</param>
    /// <param name="scheduleType">Optional. The type of the schedule of the scheduled job.</param>
    /// <param name="intervalMinutes">Optional. The interval in minutes of the schedule.</param>
    /// <param name="hour">Optional. The hour of the schedule.</param>
    /// <param name="minute">Optional. The minute of the schedule.</param>
    /// <param name="status">Optional. The status of the scheduled job.</param>
    /// <param name="ownerUserId">Optional. The Id of the user that owns the scheduled job.</param>
    /// <param name="lastStartedOnUtc">Optional. The last start time of the scheduled job.</param>
    /// <param name="lastCompletedOnUtc">Optional. The last completion time of the scheduled job.</param>
    /// <returns>The created <see cref="ScheduledJobEntity"/>.</returns>
    public ScheduledJobEntity Create(
        Guid? id = null,
        string? name = null,
        ScheduledTaskType? taskType = null,
        ScheduleType? scheduleType = null,
        int? intervalMinutes = null,
        int? hour = null,
        int? minute = null,
        ScheduledJobStatus? status = null,
        Guid? ownerUserId = null,
        DateTime? lastStartedOnUtc = null,
        DateTime? lastCompletedOnUtc = null)
    {
        return new Faker<ScheduledJobEntity>()
            .CustomInstantiator(f =>
            {
                ScheduleType resolvedScheduleType = scheduleType ?? f.PickRandom<ScheduleType>();
                if (resolvedScheduleType == ScheduleType.WithIntervalInMinutes)
                {
                    return new ScheduledJobEntity
                    {
                        Id = id ?? Guid.NewGuid(),
                        Name = name ?? f.Commerce.ProductName(),
                        TaskType = taskType ?? f.PickRandom<ScheduledTaskType>(),
                        ScheduleType = resolvedScheduleType,
                        IntervalMinutes = intervalMinutes ?? f.Random.Int(1, 1440),
                        Hour = null,
                        Minute = null,
                        Status = status ?? f.PickRandom<ScheduledJobStatus>(),
                        OwnerUserId = ownerUserId ?? Guid.NewGuid(),
                        LastStartedOnUtc = lastStartedOnUtc,
                        LastCompletedOnUtc = lastCompletedOnUtc,
                        CreatedOnUtc = DateTime.UtcNow,
                        CreatedBy = Guid.NewGuid(),
                        UpdatedOnUtc = null,
                        UpdatedBy = null
                    };
                }
                return new ScheduledJobEntity
                {
                    Id = id ?? Guid.NewGuid(),
                    Name = name ?? f.Commerce.ProductName(),
                    TaskType = taskType ?? f.PickRandom<ScheduledTaskType>(),
                    ScheduleType = resolvedScheduleType,
                    IntervalMinutes = null,
                    Hour = hour ?? f.Random.Int(0, 23),
                    Minute = minute ?? f.Random.Int(0, 59),
                    Status = status ?? f.PickRandom<ScheduledJobStatus>(),
                    OwnerUserId = ownerUserId ?? Guid.NewGuid(),
                    LastStartedOnUtc = lastStartedOnUtc,
                    LastCompletedOnUtc = lastCompletedOnUtc,
                    CreatedOnUtc = DateTime.UtcNow,
                    CreatedBy = Guid.NewGuid(),
                    UpdatedOnUtc = null,
                    UpdatedBy = null
                };
            })
            .Generate();
    }

    /// <summary>
    /// Creates a list of <see cref="ScheduledJobEntity"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="ScheduledJobEntity"/> instances.</returns>
    public List<ScheduledJobEntity> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
