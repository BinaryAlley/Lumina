#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Enums.Scheduling;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.DTO.Scheduling;

/// <summary>
/// Data transfer object for an execution of the task of a scheduled job.
/// </summary>
/// <param name="Id">The unique identifier of the execution.</param>
/// <param name="ScheduledJobId">The unique identifier of the scheduled job whose task was executed.</param>
/// <param name="TaskType">The type of the task that was executed.</param>
/// <param name="IsCycleRun"><see langword="true"/> when the execution was triggered by the execution cycle of the scheduled job.</param>
/// <param name="StartedOnUtc">The date and time when the execution started.</param>
/// <param name="CompletedOnUtc">The optional date and time when the execution completed.</param>
[DebuggerDisplay("Id: {Id}, ScheduledJobId: {ScheduledJobId}")]
public record ScheduledJobExecutionDto(
    Guid Id,
    Guid ScheduledJobId,
    ScheduledTaskType TaskType,
    bool IsCycleRun,
    DateTime StartedOnUtc,
    DateTime? CompletedOnUtc
);
