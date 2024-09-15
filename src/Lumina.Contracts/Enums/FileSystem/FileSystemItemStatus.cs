namespace Lumina.Contracts.Enums.FileSystem;

/// <summary>
/// Enumeration for file system item statuses types.
/// </summary>
public enum FileSystemItemStatus
{
    /// <summary>
    /// Indicates that the file system item is accessible.
    /// </summary>
    Accessible,

    /// <summary>
    /// Indicates that the file system item is inaccessible due to permissions or other restrictions.
    /// </summary>
    Inaccessible,

    /// <summary>
    /// Indicates that the status of the file system item could not be determined due to an unknown error.
    /// </summary>
    UnknownError
}