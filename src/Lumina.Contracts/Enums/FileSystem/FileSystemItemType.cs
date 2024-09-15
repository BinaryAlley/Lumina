namespace Lumina.Contracts.Enums.FileSystem;

/// <summary>
/// Enumeration for the types of file system elements.
/// </summary>
public enum FileSystemItemType
{
    /// <summary>
    /// Represents a file system root, which can be a drive on Windows, and a path separator on Unix.
    /// </summary>
    Root,

    /// <summary>
    /// Represents a directory, which is a directory that can contain other directories or files.
    /// </summary>
    Directory,

    /// <summary>
    /// Represents a file, which is a document, executable, or other data stored on the file system.
    /// </summary>
    File
}