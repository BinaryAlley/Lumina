#region ========================================================================= USING =====================================================================================
using Nemera.Domain.Common.Events;
using Nemera.Domain.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate.Events;
using Nemera.Domain.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate.ValueObjects;
using Nemera.Domain.Fixtures.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate.Events;
using Nemera.Domain.Fixtures.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate.ValueObjects;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Nemera.Domain.UnitTests.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate.Events;

/// <summary>
/// Contains unit tests for the <see cref="ScheduledJobCycleStoppedDomainEvent"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ScheduledJobCycleStoppedDomainEventTests
{
    private readonly ScheduledJobIdFixture _scheduledJobIdFixture = new();
    private readonly ScheduledJobCycleStoppedDomainEventFixture _scheduledJobCycleStoppedDomainEventFixture = new();

    [Fact]
    public void Constructor_WhenCalled_ShouldSetAllProperties()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        ScheduledJobId scheduledJobId = _scheduledJobIdFixture.Create();
        DateTime occurredOnUtc = DateTime.UtcNow;

        // Act
        ScheduledJobCycleStoppedDomainEvent domainEvent = _scheduledJobCycleStoppedDomainEventFixture.Create(id, scheduledJobId, occurredOnUtc);

        // Assert
        Assert.Equal(id, domainEvent.Id);
        Assert.Equal(scheduledJobId, domainEvent.ScheduledJobId);
        Assert.Equal(occurredOnUtc, domainEvent.OccurredOnUtc);
        Assert.IsType<IDomainEvent>(domainEvent, exactMatch: false);
    }
}
