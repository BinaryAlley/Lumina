#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.Requests.FileSystemManagement.Path;

/// <summary>
/// Represents a request to split a file system path.
/// </summary>
/// <param name="Path">The file system path for which to get the path segments. Required.</param>
[DebuggerDisplay("Path: {Path}")]
public record SplitPathRequest(
    string? Path
);
