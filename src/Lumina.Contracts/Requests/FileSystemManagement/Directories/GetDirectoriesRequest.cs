#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
using System.Text.Json.Serialization;
#endregion

namespace Lumina.Contracts.Requests.FileSystemManagement.Directories;

/// <summary>
/// Represents a request to get the directories of a file system path.
/// </summary>
/// <param name="Path">The file system path for which to get the directories.</param>
/// <param name="IncludeHiddenElements">Whether to include hidden file system elements or not.</param>
[DebuggerDisplay("Path: {Path}")]
public record GetDirectoriesRequest(
    string? Path,
    bool IncludeHiddenElements
);
