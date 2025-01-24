namespace Lumina.Domain.Common.Enums.MediaLibrary;

/// <summary>
/// Enumeration for the statuses of the jobs ran on media libraries scan.
/// </summary>
public enum LibraryScanJobStatus
{
    /// <summary>
    /// Job is still pending.
    /// </summary>
    Pending,

    /// <summary>
    /// Job is running.
    /// </summary>
    Running,

    /// <summary>
    /// Job has finished successfully.
    /// </summary>
    Completed,

    /// <summary>
    /// Job has failed.
    /// </summary>
    Failed
}
