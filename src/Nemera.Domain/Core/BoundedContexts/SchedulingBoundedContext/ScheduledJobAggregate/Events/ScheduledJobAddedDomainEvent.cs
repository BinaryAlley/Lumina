#region ========================================================================= USING =====================================================================================
using Nemera.Domain.Common.Events;
using Nemera.Domain.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate.ValueObjects;
using System;
using System.Diagnostics;
#endregion

namespace Nemera.Domain.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate.Events;

/// <summary>
/// Domain event raised when a scheduled job is added.
/// </summary>
/// <param name="Id">The unique identifier of the domain event.</param>
/// <param name="ScheduledJobId">The unique identifier of the scheduled job that was added.</param>
/// <param name="OccurredOnUtc">The date and time when the domain event occurred.</param>
[DebuggerDisplay("Id: {ScheduledJobId.Value}")]
public record ScheduledJobAddedDomainEvent(
    Guid Id,
    ScheduledJobId ScheduledJobId,
    DateTime OccurredOnUtc
) : IDomainEvent;
