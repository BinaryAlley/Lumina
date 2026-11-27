#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.DataAccess.Entities.Scheduling;
using Nemera.Application.Common.DataAccess.UoW;
using Nemera.Application.Common.DTO.Filtering;
using Nemera.Application.Common.DTO.Pagination;
using Nemera.Application.Common.Mapping.Scheduling;
using Nemera.Application.Core.Scheduling.Notifications;
using Nemera.Contracts.Responses.Scheduling;
using Nemera.Domain.Common.Errors;
using Nemera.Domain.Common.Events;
using Nemera.Domain.Common.Exceptions;
using Nemera.Domain.Common.Primitives;
using Nemera.Domain.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate.Events;
using Nemera.Domain.SharedKernel.Common.Enums.Scheduling;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Nemera.Application.Core.Scheduling.Events;

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
        Result<ScheduledJobEntity?> getScheduledJobResult = await _unitOfWork.ScheduledJobRepository.GetByIdAsync(domainEvent.ScheduledJobId.Value, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (getScheduledJobResult.IsFailure)
            throw new EventualConsistencyException(getScheduledJobResult.FirstError, getScheduledJobResult.Errors);
        if (getScheduledJobResult.Value is null)
            throw new EventualConsistencyException(Errors.Scheduling.ScheduledJobNotFound);
        ScheduledJobEntity scheduledJob = getScheduledJobResult.Value;

        // A cycle run returns the scheduled job to its active status, a one time execution completes it.
        ScheduledJobStatus status = domainEvent.IsCycleRun ? ScheduledJobStatus.Active : ScheduledJobStatus.Completed;
        ScheduledJobEntity updatedScheduledJob = scheduledJob.ToUpdatedRepositoryEntity(status, scheduledJob.LastStartedOnUtc, domainEvent.CompletedOnUtc);
        Result<Updated> updateScheduledJobResult = await _unitOfWork.ScheduledJobRepository.UpdateAsync(updatedScheduledJob, cancellationToken).ConfigureAwait(false);
        if (updateScheduledJobResult.IsFailure)
            throw new EventualConsistencyException(updateScheduledJobResult.FirstError, updateScheduledJobResult.Errors);

        // Mark the execution of the task of the scheduled job as completed.
        Result<ScheduledJobExecutionEntity?> getExecutionResult = await _unitOfWork.ScheduledJobExecutionRepository.GetByIdAsync(domainEvent.RunId, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (getExecutionResult.IsFailure)
            throw new EventualConsistencyException(getExecutionResult.FirstError, getExecutionResult.Errors);
        if (getExecutionResult.Value is null)
            throw new EventualConsistencyException(Errors.Scheduling.ScheduledJobExecutionNotFound);
        ScheduledJobExecutionEntity execution = getExecutionResult.Value;
        ScheduledJobExecutionEntity updatedExecution = execution.ToUpdatedRepositoryEntity(domainEvent.CompletedOnUtc);
        Result<Updated> updateExecutionResult = await _unitOfWork.ScheduledJobExecutionRepository.UpdateAsync(updatedExecution, cancellationToken).ConfigureAwait(false);
        if (updateExecutionResult.IsFailure)
            throw new EventualConsistencyException(updateExecutionResult.FirstError, updateExecutionResult.Errors);

        Result<Success> saveChangesResult = await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        if (saveChangesResult.IsFailure)
            throw new EventualConsistencyException(saveChangesResult.FirstError, saveChangesResult.Errors);

        // Notify the SignalR clients about the current scheduled jobs and the completed execution.
        await NotifyCurrentScheduledJobsAsync(cancellationToken).ConfigureAwait(false);
        await _scheduledJobNotifier.SendScheduledJobExecutionCompletedAsync(updatedExecution.ToResponse(), cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Notifies the SignalR clients about the current list of scheduled jobs.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    private async Task NotifyCurrentScheduledJobsAsync(CancellationToken cancellationToken)
    {
        Result<PaginatedResultDto<ScheduledJobEntity>> getScheduledJobsResult = await _unitOfWork.ScheduledJobRepository.GetAllAsync<BaseFilterDto>(cancellationToken: cancellationToken).ConfigureAwait(false);
        if (getScheduledJobsResult.IsFailure)
            throw new EventualConsistencyException(getScheduledJobsResult.FirstError, getScheduledJobsResult.Errors);
        IReadOnlyList<ScheduledJobResponse> scheduledJobResponses = [.. getScheduledJobsResult.Value.Data.Select(scheduledJob => scheduledJob.ToResponse())];
        await _scheduledJobNotifier.SendScheduledJobsAsync(scheduledJobResponses, cancellationToken).ConfigureAwait(false);
    }
}
