#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.Requests.FileSystemManagement.Directories;

/// <summary>
/// Represents a request to get the directories of a file system path.
/// </summary>
/// <param name="Path">The file system path for which to get the directories. Required.</param>
/// <param name="IncludeHiddenElements">Whether to include hidden file system elements or not. Optional.</param>
[DebuggerDisplay("Path: {Path}")]
public record GetDirectoriesRequest(
    string? Path,
    bool IncludeHiddenElements
);
