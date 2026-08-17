#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.Requests.FileSystemManagement.Path;

/// <summary>
/// Represents a request to get the root of a file system path.
/// </summary>
/// <param name="Path">The file system path for which to get the root. Required.</param>
[DebuggerDisplay("Path: {Path}")]
public record GetPathRootRequest(
    string? Path
);
