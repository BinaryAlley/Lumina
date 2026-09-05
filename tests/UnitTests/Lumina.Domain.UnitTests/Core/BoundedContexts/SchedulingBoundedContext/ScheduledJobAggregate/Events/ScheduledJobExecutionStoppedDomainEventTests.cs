#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Events;
using Lumina.Domain.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate.Events;
using Lumina.Domain.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate.Events;
using Lumina.Domain.Fixtures.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate.ValueObjects;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate.Events;

/// <summary>
/// Contains unit tests for the <see cref="ScheduledJobExecutionStoppedDomainEvent"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ScheduledJobExecutionStoppedDomainEventTests
{
    private readonly ScheduledJobIdFixture _scheduledJobIdFixture = new();
    private readonly ScheduledJobExecutionStoppedDomainEventFixture _scheduledJobExecutionStoppedDomainEventFixture = new();

    [Fact]
    public void Constructor_WhenCalled_ShouldSetAllProperties()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        ScheduledJobId scheduledJobId = _scheduledJobIdFixture.Create();
        DateTime occurredOnUtc = DateTime.UtcNow;

        // Act
        ScheduledJobExecutionStoppedDomainEvent domainEvent = _scheduledJobExecutionStoppedDomainEventFixture.Create(id, scheduledJobId, occurredOnUtc);

        // Assert
        Assert.Equal(id, domainEvent.Id);
        Assert.Equal(scheduledJobId, domainEvent.ScheduledJobId);
        Assert.Equal(occurredOnUtc, domainEvent.OccurredOnUtc);
        Assert.IsType<IDomainEvent>(domainEvent, exactMatch: false);
    }
}
