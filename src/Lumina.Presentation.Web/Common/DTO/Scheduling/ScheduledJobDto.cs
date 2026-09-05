#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Enums.Scheduling;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.DTO.Scheduling;

/// <summary>
/// Data transfer object for a scheduled job.
/// </summary>
/// <param name="Id">The unique identifier of the scheduled job.</param>
/// <param name="Name">The name of the scheduled job.</param>
/// <param name="TaskType">The type of the task executed by the scheduled job.</param>
/// <param name="ScheduleType">The type of the schedule of the scheduled job.</param>
/// <param name="IntervalMinutes">The interval in minutes of the schedule of the scheduled job.</param>
/// <param name="Hour">The hour of the schedule of the scheduled job.</param>
/// <param name="Minute">The minute of the schedule of the scheduled job.</param>
/// <param name="Status">The status of the scheduled job.</param>
/// <param name="LastStartedOnUtc">The optional date and time when the task of the scheduled job last started its execution.</param>
/// <param name="LastCompletedOnUtc">The optional date and time when the task of the scheduled job last completed its execution.</param>
[DebuggerDisplay("Id: {Id}, Name: {Name}, Status: {Status}")]
public record ScheduledJobDto(
    Guid Id,
    string Name,
    ScheduledTaskType TaskType,
    ScheduleType ScheduleType,
    int? IntervalMinutes,
    int? Hour,
    int? Minute,
    ScheduledJobStatus Status,
    DateTime? LastStartedOnUtc,
    DateTime? LastCompletedOnUtc
);
