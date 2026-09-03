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
    CleanTemporaryFiles
}
