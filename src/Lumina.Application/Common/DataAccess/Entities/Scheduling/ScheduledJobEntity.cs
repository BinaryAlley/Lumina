#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Common;
using Lumina.Domain.SharedKernel.Common.Enums.Scheduling;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Common.DataAccess.Entities.Scheduling;

/// <summary>
/// Repository entity for a scheduled job.
/// </summary>
[DebuggerDisplay("Id: {Id}, Status: {Status}")]
public class ScheduledJobEntity : IStorageEntity, IAuditableEntity
{
    /// <summary>
    /// Gets the Id of the scheduled job.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Gets the name of the scheduled job.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the type of the task executed by the scheduled job.
    /// </summary>
    public required ScheduledTaskType TaskType { get; init; }

    /// <summary>
    /// Gets the type of the schedule of the scheduled job.
    /// </summary>
    public required ScheduleType ScheduleType { get; init; }

    /// <summary>
    /// Gets the interval in minutes of the schedule of the scheduled job, when its <see cref="ScheduleType"/> is <see cref="ScheduleType.WithIntervalInMinutes"/>.
    /// </summary>
    public int? IntervalMinutes { get; init; }

    /// <summary>
    /// Gets the hour of the schedule of the scheduled job, when its <see cref="ScheduleType"/> is <see cref="ScheduleType.DailyAtHourAndMinute"/>.
    /// </summary>
    public int? Hour { get; init; }

    /// <summary>
    /// Gets the minute of the schedule of the scheduled job, when its <see cref="ScheduleType"/> is <see cref="ScheduleType.DailyAtHourAndMinute"/>.
    /// </summary>
    public int? Minute { get; init; }

    /// <summary>
    /// Gets the status of the scheduled job.
    /// </summary>
    public required ScheduledJobStatus Status { get; init; }

    /// <summary>
    /// Gets the Id of the user that owns the scheduled job.
    /// </summary>
    public required Guid OwnerUserId { get; init; }

    /// <summary>
    /// Gets the optional date and time when the task of the scheduled job last started its execution.
    /// </summary>
    public DateTime? LastStartedOnUtc { get; init; }

    /// <summary>
    /// Gets the optional date and time when the task of the scheduled job last completed its execution.
    /// </summary>
    public DateTime? LastCompletedOnUtc { get; init; }

    /// <summary>
    /// Gets or sets the time and date when the entity was added.
    /// </summary>
    public required DateTime CreatedOnUtc { get; set; }

    /// <summary>
    /// Gets or sets the Id of the user that created the entity.
    /// </summary>
    public required Guid CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the optional time and date when the entity was updated.
    /// </summary>
    public DateTime? UpdatedOnUtc { get; set; }

    /// <summary>
    /// Gets or sets the optional Id of the user that updated the entity.
    /// </summary>
    public required Guid? UpdatedBy { get; set; }
}
