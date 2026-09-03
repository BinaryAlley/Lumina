#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Scheduling;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Mapping.Scheduling;
using Lumina.Application.Core.Scheduling.Notifications;
using Lumina.Contracts.Responses.Scheduling;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Events;
using Lumina.Domain.Common.Exceptions;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate.Events;
using Lumina.Domain.SharedKernel.Common.Enums.Scheduling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Core.Scheduling.Events;

/// <summary>
/// Handler for the domain event raised when the task of a scheduled job completes its execution.
/// </summary>
public class ScheduledJobExecutionCompletedDomainEventHandler : IDomainEventHandler<ScheduledJobExecutionCompletedDomainEvent>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IScheduledJobNotifier _scheduledJobNotifier;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScheduledJobExecutionCompletedDomainEventHandler"/> class.
    /// </summary>
    /// <param name="scheduledJobNotifier">Injected service for notifying the scheduled job changes to SignalR clients.</param>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    public ScheduledJobExecutionCompletedDomainEventHandler(IScheduledJobNotifier scheduledJobNotifier, IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _scheduledJobNotifier = scheduledJobNotifier;
    }

    /// <summary>
    /// Handles the event raised when the task of a scheduled job completes its execution.
    /// </summary>
    /// <param name="domainEvent">The domain event to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public async ValueTask HandleAsync(ScheduledJobExecutionCompletedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        // Get the scheduled job from the repository.
        Result<ScheduledJobEntity?> getScheduledJobResult = await _unitOfWork.ScheduledJobRepository.GetByIdAsync(domainEvent.ScheduledJobId.Value, cancellationToken).ConfigureAwait(false);
        if (getScheduledJobResult.IsFailure)
            throw new EventualConsistencyException(getScheduledJobResult.FirstError, getScheduledJobResult.Errors);
        if (getScheduledJobResult.Value is null)
            throw new EventualConsistencyException(Errors.Scheduling.ScheduledJobNotFound);
        ScheduledJobEntity scheduledJob = getScheduledJobResult.Value;

        // A cycle run returns the scheduled job to its active status, a one time execution completes it.
        ScheduledJobStatus status = domainEvent.IsCycleRun ? ScheduledJobStatus.Active : ScheduledJobStatus.Completed;
        ScheduledJobEntity updatedScheduledJob = CreateUpdatedScheduledJob(scheduledJob, status, scheduledJob.LastStartedOnUtc, domainEvent.CompletedOnUtc);
        Result<Updated> updateScheduledJobResult = await _unitOfWork.ScheduledJobRepository.UpdateAsync(updatedScheduledJob, cancellationToken).ConfigureAwait(false);
        if (updateScheduledJobResult.IsFailure)
            throw new EventualConsistencyException(updateScheduledJobResult.FirstError, updateScheduledJobResult.Errors);

        // Mark the execution of the task of the scheduled job as completed.
        Result<ScheduledJobExecutionEntity?> getExecutionResult = await _unitOfWork.ScheduledJobExecutionRepository.GetByIdAsync(domainEvent.RunId, cancellationToken).ConfigureAwait(false);
        if (getExecutionResult.IsFailure)
            throw new EventualConsistencyException(getExecutionResult.FirstError, getExecutionResult.Errors);
        if (getExecutionResult.Value is null)
            throw new EventualConsistencyException(Errors.Scheduling.ScheduledJobExecutionNotFound);
        ScheduledJobExecutionEntity execution = getExecutionResult.Value;
        ScheduledJobExecutionEntity updatedExecution = CreateUpdatedExecution(execution, domainEvent.CompletedOnUtc);
        Result<Updated> updateExecutionResult = await _unitOfWork.ScheduledJobExecutionRepository.UpdateAsync(updatedExecution, cancellationToken).ConfigureAwait(false);
        if (updateExecutionResult.IsFailure)
            throw new EventualConsistencyException(updateExecutionResult.FirstError, updateExecutionResult.Errors);

        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        // Notify the SignalR clients about the current scheduled jobs and the completed execution.
        await NotifyCurrentScheduledJobsAsync(cancellationToken).ConfigureAwait(false);
        await _scheduledJobNotifier.SendScheduledJobExecutionCompletedAsync(updatedExecution.ToResponse(), cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Creates a copy of <paramref name="scheduledJob"/> with the provided status and timestamps.
    /// </summary>
    /// <param name="scheduledJob">The scheduled job to copy.</param>
    /// <param name="status">The status of the copy.</param>
    /// <param name="lastStartedOnUtc">The last start time of the copy.</param>
    /// <param name="lastCompletedOnUtc">The last completion time of the copy.</param>
    /// <returns>The copy of the scheduled job.</returns>
    private static ScheduledJobEntity CreateUpdatedScheduledJob(ScheduledJobEntity scheduledJob, ScheduledJobStatus status, DateTime? lastStartedOnUtc, DateTime? lastCompletedOnUtc)
    {
        return new ScheduledJobEntity
        {
            Id = scheduledJob.Id,
            Name = scheduledJob.Name,
            TaskType = scheduledJob.TaskType,
            ScheduleType = scheduledJob.ScheduleType,
            IntervalMinutes = scheduledJob.IntervalMinutes,
            Hour = scheduledJob.Hour,
            Minute = scheduledJob.Minute,
            Status = status,
            OwnerUserId = scheduledJob.OwnerUserId,
            LastStartedOnUtc = lastStartedOnUtc,
            LastCompletedOnUtc = lastCompletedOnUtc,
            CreatedOnUtc = scheduledJob.CreatedOnUtc,
            CreatedBy = scheduledJob.CreatedBy,
            UpdatedOnUtc = scheduledJob.UpdatedOnUtc,
            UpdatedBy = scheduledJob.UpdatedBy
        };
    }

    /// <summary>
    /// Creates a copy of <paramref name="execution"/> with the provided completion time.
    /// </summary>
    /// <param name="execution">The execution to copy.</param>
    /// <param name="completedOnUtc">The completion time of the copy.</param>
    /// <returns>The copy of the execution.</returns>
    private static ScheduledJobExecutionEntity CreateUpdatedExecution(ScheduledJobExecutionEntity execution, DateTime? completedOnUtc)
    {
        return new ScheduledJobExecutionEntity
        {
            Id = execution.Id,
            ScheduledJobId = execution.ScheduledJobId,
            TaskType = execution.TaskType,
            IsCycleRun = execution.IsCycleRun,
            StartedOnUtc = execution.StartedOnUtc,
            CompletedOnUtc = completedOnUtc,
            CreatedOnUtc = execution.CreatedOnUtc,
            CreatedBy = execution.CreatedBy,
            UpdatedOnUtc = execution.UpdatedOnUtc,
            UpdatedBy = execution.UpdatedBy
        };
    }

    /// <summary>
    /// Notifies the SignalR clients about the current list of scheduled jobs.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    private async Task NotifyCurrentScheduledJobsAsync(CancellationToken cancellationToken)
    {
        Result<IEnumerable<ScheduledJobEntity>> getScheduledJobsResult = await _unitOfWork.ScheduledJobRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
        if (getScheduledJobsResult.IsFailure)
            throw new EventualConsistencyException(getScheduledJobsResult.FirstError, getScheduledJobsResult.Errors);
        IReadOnlyList<ScheduledJobResponse> scheduledJobResponses = [.. getScheduledJobsResult.Value.Select(scheduledJob => scheduledJob.ToResponse())];
        await _scheduledJobNotifier.SendScheduledJobsAsync(scheduledJobResponses, cancellationToken).ConfigureAwait(false);
    }
}
