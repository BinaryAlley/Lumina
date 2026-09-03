#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.Scheduling;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Requests.Scheduling;

/// <summary>
/// Represents a request to add a scheduled job.
/// </summary>
/// <param name="Name">The name of the scheduled job. Required.</param>
/// <param name="TaskType">The type of the task executed by the scheduled job. Required.</param>
/// <param name="ScheduleType">The type of the schedule of the scheduled job. Required.</param>
/// <param name="IntervalMinutes">The interval in minutes of the schedule, required when <paramref name="ScheduleType"/> is <see cref="ScheduleType.WithIntervalInMinutes"/>.</param>
/// <param name="Hour">The hour of the schedule, required when <paramref name="ScheduleType"/> is <see cref="ScheduleType.DailyAtHourAndMinute"/>.</param>
/// <param name="Minute">The minute of the schedule, required when <paramref name="ScheduleType"/> is <see cref="ScheduleType.DailyAtHourAndMinute"/>.</param>
[DebuggerDisplay("Name: {Name}")]
public record AddScheduledJobRequest(
    string Name,
    ScheduledTaskType TaskType,
    ScheduleType ScheduleType,
    int? IntervalMinutes,
    int? Hour,
    int? Minute
);
