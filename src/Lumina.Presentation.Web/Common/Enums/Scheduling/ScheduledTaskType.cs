namespace Lumina.Presentation.Web.Common.Enums.Scheduling;

/// <summary>
/// Enumeration for the types of tasks a scheduled job can execute.
/// </summary>
public enum ScheduledTaskType
{
    /// <summary>
    /// The scheduled job scans all enabled media libraries.
    /// </summary>
    ScanMediaLibraries,

    /// <summary>
    /// The scheduled job cleans the temporary directory of the application.
    /// </summary>
    CleanTemporaryFiles,

    /// <summary>
    /// The scheduled job repairs the installed themes whose files are missing.
    /// </summary>
    RepairThemes,

    /// <summary>
    /// The scheduled job cleans the execution history of the scheduled jobs, keeping only the recent executions.
    /// </summary>
    CleanScheduledJobExecutionHistory
}
