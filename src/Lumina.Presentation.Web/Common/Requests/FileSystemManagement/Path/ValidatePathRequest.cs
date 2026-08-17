#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.Requests.FileSystemManagement.Path;

/// <summary>
/// Represents a request to check the validity of a file system path.
/// </summary>
/// <param name="Path">The file system path to validate. Required.</param>
[DebuggerDisplay("Path: {Path}")]
public record ValidatePathRequest(
    string? Path
);
