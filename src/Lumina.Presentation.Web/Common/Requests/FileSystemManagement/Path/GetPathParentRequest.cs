#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.Requests.FileSystemManagement.Path;

/// <summary>
/// Represents the request for retrieving the parent path of a file system path.
/// </summary>
/// <param name="Path">The path for which the parent path is retrieved.</param>
[DebuggerDisplay("Path: {Path}")]
public record GetPathParentRequest(
    string? Path
);
