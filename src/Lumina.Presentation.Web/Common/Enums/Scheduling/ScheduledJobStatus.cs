namespace Lumina.Presentation.Web.Common.Enums.Scheduling;

/// <summary>
/// Enumeration for the statuses of scheduled jobs.
/// </summary>
public enum ScheduledJobStatus
{
    /// <summary>
    /// The scheduled job was added, but its execution cycle was not started.
    /// </summary>
    Added,

    /// <summary>
    /// The execution cycle of the scheduled job is active, and the job is waiting for its next scheduled execution.
    /// </summary>
    Active,

    /// <summary>
    /// The task of the scheduled job is currently being executed.
    /// </summary>
    Running,

    /// <summary>
    /// The scheduled job finished its latest one time execution.
    /// </summary>
    Completed
}
