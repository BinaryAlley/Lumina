#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
using System.Text.Json.Serialization;
#endregion

namespace Lumina.Contracts.Requests.FileSystemManagement.Directories;

/// <summary>
/// Represents a request to get the directories of a file system path.
/// </summary>
/// <param name="Path">The file system path for which to get the directory tree. Required.</param>
/// <param name="IncludeHiddenElements">Whether to include hidden file system elements or not. Optional.</param>
[DebuggerDisplay("Path: {Path}")]
public record GetTreeDirectoriesRequest(
    string? Path,
    bool IncludeHiddenElements
);
