namespace Lumina.Domain.Common.Enums;

/// <summary>
/// Enumeration for the types of file system elements.
/// </summary>
public enum FileSystemItemType
{
    /// <summary>
    /// Represents a drive, such as a hard disk, USB drive, or network drive.
    /// </summary>
    Drive,

    /// <summary>
    /// Represents a directory, which is a directory that can contain other directories or files.
    /// </summary>
    Directory,

    /// <summary>
    /// Represents a file, which is a document, executable, or other data stored on the file system.
    /// </summary>
    File
}