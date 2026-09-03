#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.Scheduling.Notifications;
using Lumina.Contracts.Responses.Scheduling;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Infrastructure.Core.Scheduling.Notifications;

/// <summary>
/// Class used to notify SignalR clients about the scheduled jobs and their executions.
/// </summary>
public sealed class ScheduledJobNotifier : IScheduledJobNotifier
{
    private readonly IHubContext<ScheduledJobsHub, IScheduledJobsClient> _hubContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScheduledJobNotifier"/> class.
    /// </summary>
    /// <param name="hubContext">SignalR hub context abstraction.</param>
    public ScheduledJobNotifier(IHubContext<ScheduledJobsHub, IScheduledJobsClient> hubContext)
    {
        _hubContext = hubContext;
    }

    /// <summary>
    /// Notifies the SignalR clients about the current list of scheduled jobs.
    /// </summary>
    /// <param name="scheduledJobs">The current list of scheduled jobs.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public async Task SendScheduledJobsAsync(IReadOnlyList<ScheduledJobResponse> scheduledJobs, CancellationToken cancellationToken)
    {
        await _hubContext.Clients.All.ReceiveScheduledJobsAsync(scheduledJobs).ConfigureAwait(false);
    }

    /// <summary>
    /// Notifies the SignalR clients about an execution of the task of a scheduled job that started.
    /// </summary>
    /// <param name="execution">The execution of the task of the scheduled job that started.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public async Task SendScheduledJobExecutionStartedAsync(ScheduledJobExecutionResponse execution, CancellationToken cancellationToken)
    {
        await _hubContext.Clients.All.ScheduledJobExecutionStartedAsync(execution).ConfigureAwait(false);
    }

    /// <summary>
    /// Notifies the SignalR clients about an execution of the task of a scheduled job that completed.
    /// </summary>
    /// <param name="execution">The execution of the task of the scheduled job that completed.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public async Task SendScheduledJobExecutionCompletedAsync(ScheduledJobExecutionResponse execution, CancellationToken cancellationToken)
    {
        await _hubContext.Clients.All.ScheduledJobExecutionCompletedAsync(execution).ConfigureAwait(false);
    }
}
