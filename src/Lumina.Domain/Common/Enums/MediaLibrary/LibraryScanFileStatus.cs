namespace Lumina.Domain.Common.Enums.MediaLibrary;

/// <summary>
/// Enumeration for the statuses of the files processed during a media library scan.
/// </summary>
public enum LibraryScanFileStatus
{
    /// <summary>
    /// The file contents have not been changed.
    /// </summary>
    Unchanged,

    /// <summary>
    /// The file contents have been changed.
    /// </summary>
    Modified, 

    /// <summary>
    /// The file has been added.
    /// </summary>
    New, 

    /// <summary>
    /// The file has been deleted.
    /// </summary>
    Deleted
}
