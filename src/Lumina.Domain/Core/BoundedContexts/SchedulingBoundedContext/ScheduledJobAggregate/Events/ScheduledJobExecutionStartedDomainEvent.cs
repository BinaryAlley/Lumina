#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Events;
using Lumina.Domain.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.Scheduling;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate.Events;

/// <summary>
/// Domain event raised when the task of a scheduled job starts its execution.
/// </summary>
/// <param name="Id">The unique identifier of the domain event.</param>
/// <param name="ScheduledJobId">The unique identifier of the scheduled job whose task started executing.</param>
/// <param name="RunId">The unique identifier of the current execution of the task of the scheduled job.</param>
/// <param name="TaskType">The type of the task that started executing.</param>
/// <param name="IsCycleRun"><see langword="true"/> when the execution was triggered by the execution cycle of the scheduled job, <see langword="false"/> when it was a one time execution.</param>
/// <param name="StartedOnUtc">The date and time when the execution started.</param>
[DebuggerDisplay("Id: {ScheduledJobId.Value}, RunId: {RunId}")]
public record ScheduledJobExecutionStartedDomainEvent(
    Guid Id,
    ScheduledJobId ScheduledJobId,
    Guid RunId,
    ScheduledTaskType TaskType,
    bool IsCycleRun,
    DateTime StartedOnUtc
) : IDomainEvent
{
    /// <summary>
    /// Gets the date and time when the domain event occurred.
    /// </summary>
    public DateTime OccurredOnUtc => StartedOnUtc;
}
