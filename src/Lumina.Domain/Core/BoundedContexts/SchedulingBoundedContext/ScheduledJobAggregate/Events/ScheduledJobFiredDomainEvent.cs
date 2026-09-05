#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Events;
using Lumina.Domain.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate.ValueObjects;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate.Events;

/// <summary>
/// Domain event raised when the task of a scheduled job is fired once.
/// </summary>
/// <param name="Id">The unique identifier of the domain event.</param>
/// <param name="ScheduledJobId">The unique identifier of the scheduled job whose task was fired.</param>
/// <param name="OccurredOnUtc">The date and time when the domain event occurred.</param>
[DebuggerDisplay("Id: {ScheduledJobId.Value}")]
public record ScheduledJobFiredDomainEvent(
    Guid Id,
    ScheduledJobId ScheduledJobId,
    DateTime OccurredOnUtc
) : IDomainEvent;
