#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
using System.Text.Json.Serialization;
#endregion

namespace Lumina.Contracts.Requests.FileSystemManagement.Path;

/// <summary>
/// Represents a request to split a file system path.
/// </summary>
/// <param name="Path">The file system path for which to get the path segments. Required.</param>
[DebuggerDisplay("Path: {Path}")]
public record SplitPathRequest(
    string? Path
);
