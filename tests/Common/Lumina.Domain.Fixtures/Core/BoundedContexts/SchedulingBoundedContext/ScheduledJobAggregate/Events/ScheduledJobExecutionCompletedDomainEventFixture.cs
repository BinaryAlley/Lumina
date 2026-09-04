#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate.Events;
using Lumina.Domain.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.Scheduling;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.Fixtures.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate.Events;

/// <summary>
/// Fixture class for the <see cref="ScheduledJobExecutionCompletedDomainEvent"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class ScheduledJobExecutionCompletedDomainEventFixture
{
    private readonly ScheduledJobIdFixture _scheduledJobIdFixture = new();

    /// <summary>
    /// Creates a random valid <see cref="ScheduledJobExecutionCompletedDomainEvent"/>.
    /// </summary>
    /// <param name="id">Optional. The Id of the domain event.</param>
    /// <param name="scheduledJobId">Optional. The unique identifier of the scheduled job whose task completed its execution.</param>
    /// <param name="runId">Optional. The unique identifier of the current execution of the task of the scheduled job.</param>
    /// <param name="taskType">The type of the task that completed its execution.</param>
    /// <param name="isCycleRun">Whether the execution was triggered by the execution cycle of the scheduled job.</param>
    /// <param name="completedOnUtc">Optional. The date and time when the execution completed.</param>
    /// <returns>The created <see cref="ScheduledJobExecutionCompletedDomainEvent"/>.</returns>
    public ScheduledJobExecutionCompletedDomainEvent Create(
        Guid? id = null,
        ScheduledJobId? scheduledJobId = null,
        Guid? runId = null,
        ScheduledTaskType taskType = ScheduledTaskType.ScanMediaLibraries,
        bool isCycleRun = false,
        DateTime? completedOnUtc = null)
    {
        return new ScheduledJobExecutionCompletedDomainEvent(
            id ?? Guid.NewGuid(),
            scheduledJobId ?? _scheduledJobIdFixture.Create(),
            runId ?? Guid.NewGuid(),
            taskType,
            isCycleRun,
            completedOnUtc ?? DateTime.UtcNow);
    }
}
