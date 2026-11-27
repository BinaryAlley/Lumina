#region ========================================================================= USING =====================================================================================
using Nemera.Domain.Common.Events;
using Nemera.Domain.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate.Events;
using Nemera.Domain.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate.ValueObjects;
using Nemera.Domain.Fixtures.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate.Events;
using Nemera.Domain.Fixtures.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate.ValueObjects;
using Nemera.Domain.SharedKernel.Common.Enums.Scheduling;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Nemera.Domain.UnitTests.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate.Events;

/// <summary>
/// Contains unit tests for the <see cref="ScheduledJobExecutionStartedDomainEvent"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ScheduledJobExecutionStartedDomainEventTests
{
    private readonly ScheduledJobIdFixture _scheduledJobIdFixture = new();
    private readonly ScheduledJobExecutionStartedDomainEventFixture _scheduledJobExecutionStartedDomainEventFixture = new();

    [Fact]
    public void Constructor_WhenCalled_ShouldSetAllProperties()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        ScheduledJobId scheduledJobId = _scheduledJobIdFixture.Create();
        Guid runId = Guid.NewGuid();
        ScheduledTaskType taskType = ScheduledTaskType.ScanMediaLibraries;
        bool isCycleRun = true;
        DateTime startedOnUtc = DateTime.UtcNow;

        // Act
        ScheduledJobExecutionStartedDomainEvent domainEvent = _scheduledJobExecutionStartedDomainEventFixture.Create(id, scheduledJobId, runId, taskType, isCycleRun, startedOnUtc);

        // Assert
        Assert.Equal(id, domainEvent.Id);
        Assert.Equal(scheduledJobId, domainEvent.ScheduledJobId);
        Assert.Equal(runId, domainEvent.RunId);
        Assert.Equal(taskType, domainEvent.TaskType);
        Assert.Equal(isCycleRun, domainEvent.IsCycleRun);
        Assert.Equal(startedOnUtc, domainEvent.StartedOnUtc);
        Assert.Equal(startedOnUtc, domainEvent.OccurredOnUtc);
        Assert.IsType<IDomainEvent>(domainEvent, exactMatch: false);
    }
}
