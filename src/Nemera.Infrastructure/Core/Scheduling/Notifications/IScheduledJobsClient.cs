#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Responses.Scheduling;
using System.Collections.Generic;
using System.Threading.Tasks;
#endregion

namespace Lumina.Infrastructure.Core.Scheduling.Notifications;

/// <summary>
/// Interface for the SignalR client of the scheduled jobs hub.
/// </summary>
public interface IScheduledJobsClient
{
    /// <summary>
    /// Receives the current list of scheduled jobs.
    /// </summary>
    /// <param name="scheduledJobs">The current list of scheduled jobs.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task ReceiveScheduledJobsAsync(IReadOnlyList<ScheduledJobResponse> scheduledJobs);

    /// <summary>
    /// Receives an execution of the task of a scheduled job that started.
    /// </summary>
    /// <param name="execution">The execution that started.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task ScheduledJobExecutionStartedAsync(ScheduledJobExecutionResponse execution);

    /// <summary>
    /// Receives an execution of the task of a scheduled job that completed.
    /// </summary>
    /// <param name="execution">The execution that completed.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task ScheduledJobExecutionCompletedAsync(ScheduledJobExecutionResponse execution);
}
