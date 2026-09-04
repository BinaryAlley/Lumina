#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate.Events;
using Lumina.Domain.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate.ValueObjects;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.Fixtures.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate.Events;

/// <summary>
/// Fixture class for the <see cref="ScheduledJobCycleStartedDomainEvent"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class ScheduledJobCycleStartedDomainEventFixture
{
    private readonly ScheduledJobIdFixture _scheduledJobIdFixture = new();

    /// <summary>
    /// Creates a random valid <see cref="ScheduledJobCycleStartedDomainEvent"/>.
    /// </summary>
    /// <param name="id">Optional. The Id of the domain event.</param>
    /// <param name="scheduledJobId">Optional. The unique identifier of the scheduled job whose execution cycle was started.</param>
    /// <param name="occurredOnUtc">Optional. The date and time when the domain event occurred.</param>
    /// <returns>The created <see cref="ScheduledJobCycleStartedDomainEvent"/>.</returns>
    public ScheduledJobCycleStartedDomainEvent Create(
        Guid? id = null,
        ScheduledJobId? scheduledJobId = null,
        DateTime? occurredOnUtc = null)
    {
        return new ScheduledJobCycleStartedDomainEvent(
            id ?? Guid.NewGuid(),
            scheduledJobId ?? _scheduledJobIdFixture.Create(),
            occurredOnUtc ?? DateTime.UtcNow);
    }
}
