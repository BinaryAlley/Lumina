#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
using System.Text.Json.Serialization;
#endregion

namespace Lumina.Contracts.Requests.FileSystemManagement.Path;

/// <summary>
/// Represents a request to check the existence of a file system path.
/// </summary>
/// <param name="Path">The file system path to check the exitence of. Required.</param>
/// <param name="IncludeHiddenElements">Whether to include hidden elements in the search results, or not. Optional.</param>
[DebuggerDisplay("Path: {Path}, IncludeHiddenElements: {IncludeHiddenElements}")]
public record CheckPathExistsRequest(
    string? Path,
    bool IncludeHiddenElements
);
