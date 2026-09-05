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
/// Fixture class for generating <see cref="ScheduledJobDto"/> test data.
/// </summary>
[ExcludeFromCodeCoverage]
public class ScheduledJobDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a new <see cref="ScheduledJobDto"/> instance with randomized test data.
    /// </summary>
    /// <param name="id">Optional unique identifier of the scheduled job.</param>
    /// <param name="name">Optional name of the scheduled job.</param>
    /// <param name="taskType">Optional type of the task executed by the scheduled job.</param>
    /// <param name="scheduleType">Optional type of the schedule of the scheduled job.</param>
    /// <param name="intervalMinutes">Optional interval in minutes of the schedule of the scheduled job.</param>
    /// <param name="hour">Optional hour of the schedule of the scheduled job.</param>
    /// <param name="minute">Optional minute of the schedule of the scheduled job.</param>
    /// <param name="status">Optional status of the scheduled job.</param>
    /// <param name="lastStartedOnUtc">Optional date and time when the task of the scheduled job last started its execution.</param>
    /// <param name="lastCompletedOnUtc">Optional date and time when the task of the scheduled job last completed its execution.</param>
    /// <returns>A configured <see cref="ScheduledJobDto"/> instance.</returns>
    public ScheduledJobDto Create(
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
        ScheduleType resolvedScheduleType = scheduleType ?? _faker.PickRandom<ScheduleType>();
        DateTime? resolvedLastCompletedOnUtc = lastCompletedOnUtc ?? _faker.Date.Recent(10);
        return new ScheduledJobDto(
            Id: id ?? Guid.NewGuid(),
            Name: name ?? _faker.Name.JobTitle(),
            TaskType: taskType ?? _faker.PickRandom<ScheduledTaskType>(),
            ScheduleType: resolvedScheduleType,
            IntervalMinutes: intervalMinutes ?? (resolvedScheduleType == ScheduleType.WithIntervalInMinutes ? _faker.Random.Int(1, 1440) : null),
            Hour: hour ?? (resolvedScheduleType == ScheduleType.DailyAtHourAndMinute ? _faker.Random.Int(0, 23) : null),
            Minute: minute ?? (resolvedScheduleType == ScheduleType.DailyAtHourAndMinute ? _faker.Random.Int(0, 59) : null),
            Status: status ?? _faker.PickRandom<ScheduledJobStatus>(),
            LastStartedOnUtc: lastStartedOnUtc ?? _faker.Date.Recent(10),
            LastCompletedOnUtc: resolvedLastCompletedOnUtc);
    }

    /// <summary>
    /// Creates multiple <see cref="ScheduledJobDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="ScheduledJobDto"/> instances.</returns>
    public List<ScheduledJobDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
