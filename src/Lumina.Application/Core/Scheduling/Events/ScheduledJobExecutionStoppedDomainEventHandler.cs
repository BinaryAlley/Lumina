#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Scheduling;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Mapping.Scheduling;
using Lumina.Application.Core.Scheduling.Notifications;
using Lumina.Contracts.Responses.Scheduling;
using Lumina.Domain.Common.Events;
using Lumina.Domain.Common.Exceptions;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate.Events;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Core.Scheduling.Events;

/// <summary>
/// Handler for the domain event raised when the execution of the task of a scheduled job is stopped while it is running.
/// </summary>
public class ScheduledJobExecutionStoppedDomainEventHandler : IDomainEventHandler<ScheduledJobExecutionStoppedDomainEvent>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IScheduledJobNotifier _scheduledJobNotifier;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScheduledJobExecutionStoppedDomainEventHandler"/> class.
    /// </summary>
    /// <param name="scheduledJobNotifier">Injected service for notifying the scheduled job changes to SignalR clients.</param>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    public ScheduledJobExecutionStoppedDomainEventHandler(IScheduledJobNotifier scheduledJobNotifier, IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _scheduledJobNotifier = scheduledJobNotifier;
    }

    /// <summary>
    /// Handles the event raised when the execution of the task of a scheduled job is stopped while it is running.
    /// </summary>
    /// <param name="domainEvent">The domain event to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public async ValueTask HandleAsync(ScheduledJobExecutionStoppedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        // Close the execution history row of the stopped execution, if it exists.
        Result<ScheduledJobExecutionEntity?> getOpenExecutionResult = await _unitOfWork.ScheduledJobExecutionRepository
            .GetOpenByScheduledJobIdAsync(domainEvent.ScheduledJobId.Value, cancellationToken).ConfigureAwait(false);
        if (getOpenExecutionResult.IsFailure)
            throw new EventualConsistencyException(getOpenExecutionResult.FirstError, getOpenExecutionResult.Errors);
        if (getOpenExecutionResult.Value is not null)
        {
            ScheduledJobExecutionEntity execution = getOpenExecutionResult.Value;
            ScheduledJobExecutionEntity updatedExecution = new()
            {
                Id = execution.Id,
                ScheduledJobId = execution.ScheduledJobId,
                TaskType = execution.TaskType,
                IsCycleRun = execution.IsCycleRun,
                StartedOnUtc = execution.StartedOnUtc,
                CompletedOnUtc = domainEvent.OccurredOnUtc,
                CreatedOnUtc = execution.CreatedOnUtc,
                CreatedBy = execution.CreatedBy,
                UpdatedOnUtc = execution.UpdatedOnUtc,
                UpdatedBy = execution.UpdatedBy
            };
            Result<Updated> updateExecutionResult = await _unitOfWork.ScheduledJobExecutionRepository.UpdateAsync(updatedExecution, cancellationToken).ConfigureAwait(false);
            if (updateExecutionResult.IsFailure)
                throw new EventualConsistencyException(updateExecutionResult.FirstError, updateExecutionResult.Errors);
            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        // Notify the SignalR clients about the current list of scheduled jobs.
        Result<IEnumerable<ScheduledJobEntity>> getScheduledJobsResult = await _unitOfWork.ScheduledJobRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
        if (getScheduledJobsResult.IsFailure)
            throw new EventualConsistencyException(getScheduledJobsResult.FirstError, getScheduledJobsResult.Errors);
        IReadOnlyList<ScheduledJobResponse> scheduledJobResponses = [.. getScheduledJobsResult.Value.Select(scheduledJob => scheduledJob.ToResponse())];
        await _scheduledJobNotifier.SendScheduledJobsAsync(scheduledJobResponses, cancellationToken).ConfigureAwait(false);
    }
}
