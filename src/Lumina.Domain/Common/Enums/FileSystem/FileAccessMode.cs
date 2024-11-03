namespace Lumina.Domain.Common.Enums.FileSystem;

/// <summary>
/// Enumeration for file access mode types.
/// </summary>
public enum FileAccessMode
{
    /// <summary>
    /// Represents the ability to list the contents of a directory.
    /// </summary>
    ListDirectory,

    /// <summary>
    /// Represents the ability to read file or directory properties.
    /// </summary>
    ReadProperties,

    /// <summary>
    /// Represents the ability to read the contents of a file.
    /// </summary>
    ReadContents,

    /// <summary>
    /// Represents the ability to write to a file or directory.
    /// </summary>
    Write,

    /// <summary>
    /// Represents the ability to execute a file.
    /// </summary>
    Execute,

    /// <summary>
    /// Represents the ability to delete a file or directory.
    /// </summary>
    Delete,
}