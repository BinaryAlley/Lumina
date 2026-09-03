#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Scheduling;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Scheduling;
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
/// Handler for the domain event raised when the execution cycle of a scheduled job is started.
/// </summary>
public class ScheduledJobCycleStartedDomainEventHandler : IDomainEventHandler<ScheduledJobCycleStartedDomainEvent>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IScheduledJobNotifier _scheduledJobNotifier;
    private readonly IScheduledJobScheduler _scheduledJobScheduler;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScheduledJobCycleStartedDomainEventHandler"/> class.
    /// </summary>
    /// <param name="scheduledJobNotifier">Injected service for notifying the scheduled job changes to SignalR clients.</param>
    /// <param name="scheduledJobScheduler">Injected service that schedules and executes the tasks of scheduled jobs.</param>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    public ScheduledJobCycleStartedDomainEventHandler(IScheduledJobNotifier scheduledJobNotifier, IScheduledJobScheduler scheduledJobScheduler, IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _scheduledJobNotifier = scheduledJobNotifier;
        _scheduledJobScheduler = scheduledJobScheduler;
    }

    /// <summary>
    /// Handles the event raised when the execution cycle of a scheduled job is started.
    /// </summary>
    /// <param name="domainEvent">The domain event to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public async ValueTask HandleAsync(ScheduledJobCycleStartedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        // Start the execution cycle in the scheduler service, which runs the task of the scheduled job once immediately and then on its schedule.
        await _scheduledJobScheduler.StartCycleAsync(domainEvent.ScheduledJobId, cancellationToken).ConfigureAwait(false);

        Result<IEnumerable<ScheduledJobEntity>> getScheduledJobsResult = await _unitOfWork.ScheduledJobRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
        if (getScheduledJobsResult.IsFailure)
            throw new EventualConsistencyException(getScheduledJobsResult.FirstError, getScheduledJobsResult.Errors);
        IReadOnlyList<ScheduledJobResponse> scheduledJobResponses = [.. getScheduledJobsResult.Value.Select(scheduledJob => scheduledJob.ToResponse())];
        await _scheduledJobNotifier.SendScheduledJobsAsync(scheduledJobResponses, cancellationToken).ConfigureAwait(false);
    }
}
