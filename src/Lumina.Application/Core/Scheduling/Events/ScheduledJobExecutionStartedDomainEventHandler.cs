#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Scheduling;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.DTO.Filtering;
using Lumina.Application.Common.DTO.Pagination;
using Lumina.Application.Common.Mapping.Scheduling;
using Lumina.Application.Core.Scheduling.Notifications;
using Lumina.Contracts.Responses.Scheduling;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Events;
using Lumina.Domain.Common.Exceptions;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate.Events;
using Lumina.Domain.SharedKernel.Common.Enums.Scheduling;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Core.Scheduling.Events;

/// <summary>
/// Handler for the domain event raised when the task of a scheduled job starts its execution.
/// </summary>
public class ScheduledJobExecutionStartedDomainEventHandler : IDomainEventHandler<ScheduledJobExecutionStartedDomainEvent>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IScheduledJobNotifier _scheduledJobNotifier;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScheduledJobExecutionStartedDomainEventHandler"/> class.
    /// </summary>
    /// <param name="scheduledJobNotifier">Injected service for notifying the scheduled job changes to SignalR clients.</param>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    public ScheduledJobExecutionStartedDomainEventHandler(IScheduledJobNotifier scheduledJobNotifier, IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _scheduledJobNotifier = scheduledJobNotifier;
    }

    /// <summary>
    /// Handles the event raised when the task of a scheduled job starts its execution.
    /// </summary>
    /// <param name="domainEvent">The domain event to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public async ValueTask HandleAsync(ScheduledJobExecutionStartedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        // Get the scheduled job from the repository.
        Result<ScheduledJobEntity?> getScheduledJobResult = await _unitOfWork.ScheduledJobRepository.GetByIdAsync(domainEvent.ScheduledJobId.Value, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (getScheduledJobResult.IsFailure)
            throw new EventualConsistencyException(getScheduledJobResult.FirstError, getScheduledJobResult.Errors);
        if (getScheduledJobResult.Value is null)
            throw new EventualConsistencyException(Errors.Scheduling.ScheduledJobNotFound);
        ScheduledJobEntity scheduledJob = getScheduledJobResult.Value;
        // The execution cycle is active when the scheduled job was in its active state before this execution started; the
        // status is read before the scheduled job is updated to its running state below.
        bool wasCycleActive = scheduledJob.Status == ScheduledJobStatus.Active;

        // Update the scheduled job with its running status and the start time of the execution.
        ScheduledJobEntity updatedScheduledJob = scheduledJob.ToUpdatedRepositoryEntity(ScheduledJobStatus.Running, domainEvent.StartedOnUtc, scheduledJob.LastCompletedOnUtc);
        Result<Updated> updateScheduledJobResult = await _unitOfWork.ScheduledJobRepository.UpdateAsync(updatedScheduledJob, cancellationToken).ConfigureAwait(false);
        if (updateScheduledJobResult.IsFailure)
            throw new EventualConsistencyException(updateScheduledJobResult.FirstError, updateScheduledJobResult.Errors);

        // Insert the execution of the task of the scheduled job into the repository.
        ScheduledJobExecutionEntity execution = new()
        {
            Id = domainEvent.RunId,
            ScheduledJobId = scheduledJob.Id,
            TaskType = scheduledJob.TaskType,
            IsCycleRun = domainEvent.IsCycleRun,
            WasCycleActive = wasCycleActive,
            StartedOnUtc = domainEvent.StartedOnUtc,
            CompletedOnUtc = null,
            CreatedOnUtc = scheduledJob.CreatedOnUtc,
            CreatedBy = scheduledJob.CreatedBy,
            UpdatedOnUtc = null,
            UpdatedBy = null
        };
        Result<Created> insertExecutionResult = await _unitOfWork.ScheduledJobExecutionRepository.InsertAsync(execution, cancellationToken).ConfigureAwait(false);
        if (insertExecutionResult.IsFailure)
            throw new EventualConsistencyException(insertExecutionResult.FirstError, insertExecutionResult.Errors);

        Result<Success> saveChangesResult = await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        if (saveChangesResult.IsFailure)
            throw new EventualConsistencyException(saveChangesResult.FirstError, saveChangesResult.Errors);

        // Notify the SignalR clients about the current scheduled jobs and the started execution.
        await NotifyCurrentScheduledJobsAsync(cancellationToken).ConfigureAwait(false);
        await _scheduledJobNotifier.SendScheduledJobExecutionStartedAsync(execution.ToResponse(), cancellationToken).ConfigureAwait(false);
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
