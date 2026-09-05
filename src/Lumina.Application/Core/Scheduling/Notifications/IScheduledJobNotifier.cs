#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Responses.Scheduling;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Core.Scheduling.Notifications;

/// <summary>
/// Interface for the class used to notify SignalR clients about the scheduled jobs and their executions.
/// </summary>
public interface IScheduledJobNotifier
{
    /// <summary>
    /// Notifies the SignalR clients about the current list of scheduled jobs.
    /// </summary>
    /// <param name="scheduledJobs">The current list of scheduled jobs.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    Task SendScheduledJobsAsync(IReadOnlyList<ScheduledJobResponse> scheduledJobs, CancellationToken cancellationToken);

    /// <summary>
    /// Notifies the SignalR clients about an execution of the task of a scheduled job that started.
    /// </summary>
    /// <param name="execution">The execution of the task of the scheduled job that started.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    Task SendScheduledJobExecutionStartedAsync(ScheduledJobExecutionResponse execution, CancellationToken cancellationToken);

    /// <summary>
    /// Notifies the SignalR clients about an execution of the task of a scheduled job that completed.
    /// </summary>
    /// <param name="execution">The execution of the task of the scheduled job that completed.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    Task SendScheduledJobExecutionCompletedAsync(ScheduledJobExecutionResponse execution, CancellationToken cancellationToken);
}
