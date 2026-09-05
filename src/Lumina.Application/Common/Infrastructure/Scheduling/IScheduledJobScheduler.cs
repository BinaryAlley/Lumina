#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate.ValueObjects;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Common.Infrastructure.Scheduling;

/// <summary>
/// Interface for the service that schedules and executes the tasks of scheduled jobs.
/// </summary>
public interface IScheduledJobScheduler
{
    /// <summary>
    /// Starts the execution cycle of the scheduled job identified by <paramref name="scheduledJobId"/>.
    /// </summary>
    /// <param name="scheduledJobId">The object representing the unique identifier of the scheduled job whose execution cycle is started.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task StartCycleAsync(ScheduledJobId scheduledJobId, CancellationToken cancellationToken);

    /// <summary>
    /// Stops the execution cycle of the scheduled job identified by <paramref name="scheduledJobId"/>.
    /// </summary>
    /// <param name="scheduledJobId">The object representing the unique identifier of the scheduled job whose execution cycle is stopped.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task StopCycleAsync(ScheduledJobId scheduledJobId, CancellationToken cancellationToken);

    /// <summary>
    /// Fires the task of the scheduled job identified by <paramref name="scheduledJobId"/> once, without affecting its execution cycle.
    /// </summary>
    /// <param name="scheduledJobId">The object representing the unique identifier of the scheduled job whose task is fired.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task RunOnceAsync(ScheduledJobId scheduledJobId, CancellationToken cancellationToken);
}
