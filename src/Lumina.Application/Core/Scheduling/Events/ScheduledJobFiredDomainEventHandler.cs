#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Scheduling;
using Lumina.Domain.Common.Events;
using Lumina.Domain.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate.Events;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Core.Scheduling.Events;

/// <summary>
/// Handler for the domain event raised when the task of a scheduled job is fired once.
/// </summary>
public class ScheduledJobFiredDomainEventHandler : IDomainEventHandler<ScheduledJobFiredDomainEvent>
{
    private readonly IScheduledJobScheduler _scheduledJobScheduler;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScheduledJobFiredDomainEventHandler"/> class.
    /// </summary>
    /// <param name="scheduledJobScheduler">Injected service that schedules and executes the tasks of scheduled jobs.</param>
    public ScheduledJobFiredDomainEventHandler(IScheduledJobScheduler scheduledJobScheduler)
    {
        _scheduledJobScheduler = scheduledJobScheduler;
    }

    /// <summary>
    /// Handles the event raised when the task of a scheduled job is fired once.
    /// </summary>
    /// <param name="domainEvent">The domain event to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public async ValueTask HandleAsync(ScheduledJobFiredDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        // Start a one time execution of the task of the scheduled job in the scheduler service.
        await _scheduledJobScheduler.RunOnceAsync(domainEvent.ScheduledJobId, cancellationToken).ConfigureAwait(false);
    }
}
