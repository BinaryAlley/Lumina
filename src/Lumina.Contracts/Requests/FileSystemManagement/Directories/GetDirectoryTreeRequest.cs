#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
using System.Text.Json.Serialization;
#endregion

namespace Lumina.Contracts.Requests.FileSystemManagement.Directories;

/// <summary>
/// Represents a request to get the tree of directories that make up a file system path.
/// </summary>
/// <param name="Path">The file system path for which to get the directory tree.</param>
/// <param name="IncludeFiles">Whether to include files along the directories or not.</param>
/// <param name="IncludeHiddenElements">Whether to include hidden file system elements or not.</param>
[DebuggerDisplay("Path: {Path}")]
public record GetDirectoryTreeRequest(
    string? Path,
    bool IncludeFiles,
    bool IncludeHiddenElements
);
