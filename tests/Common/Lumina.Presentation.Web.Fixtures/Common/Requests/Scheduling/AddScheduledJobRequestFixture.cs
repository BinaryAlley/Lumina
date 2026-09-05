#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.Enums.Scheduling;
using Lumina.Presentation.Web.Common.Requests.Scheduling;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.Requests.Scheduling;

/// <summary>
/// Fixture class for generating <see cref="AddScheduledJobRequest"/> test data.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddScheduledJobRequestFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a new <see cref="AddScheduledJobRequest"/> instance with randomized test data.
    /// </summary>
    /// <param name="name">Optional name of the scheduled job.</param>
    /// <param name="taskType">Optional type of the task executed by the scheduled job.</param>
    /// <param name="scheduleType">Optional type of the schedule of the scheduled job.</param>
    /// <param name="intervalMinutes">Optional interval in minutes of the schedule, when the schedule type is <see cref="ScheduleType.WithIntervalInMinutes"/>.</param>
    /// <param name="hour">Optional hour of the schedule, when the schedule type is <see cref="ScheduleType.DailyAtHourAndMinute"/>.</param>
    /// <param name="minute">Optional minute of the schedule, when the schedule type is <see cref="ScheduleType.DailyAtHourAndMinute"/>.</param>
    /// <returns>A configured <see cref="AddScheduledJobRequest"/> instance.</returns>
    public AddScheduledJobRequest Create(
        string? name = null,
        ScheduledTaskType? taskType = null,
        ScheduleType? scheduleType = null,
        int? intervalMinutes = null,
        int? hour = null,
        int? minute = null)
    {
        ScheduleType resolvedScheduleType = scheduleType ?? _faker.PickRandom<ScheduleType>();
        return new AddScheduledJobRequest(
            Name: name ?? _faker.Name.JobTitle(),
            TaskType: taskType ?? _faker.PickRandom<ScheduledTaskType>(),
            ScheduleType: resolvedScheduleType,
            IntervalMinutes: intervalMinutes ?? (resolvedScheduleType == ScheduleType.WithIntervalInMinutes ? _faker.Random.Int(1, 1440) : null),
            Hour: hour ?? (resolvedScheduleType == ScheduleType.DailyAtHourAndMinute ? _faker.Random.Int(0, 23) : null),
            Minute: minute ?? (resolvedScheduleType == ScheduleType.DailyAtHourAndMinute ? _faker.Random.Int(0, 59) : null));
    }

    /// <summary>
    /// Creates multiple <see cref="AddScheduledJobRequest"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="AddScheduledJobRequest"/> instances.</returns>
    public List<AddScheduledJobRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
