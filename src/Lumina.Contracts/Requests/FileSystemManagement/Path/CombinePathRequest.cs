#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
using System.Text.Json.Serialization;
#endregion

namespace Lumina.Contracts.Requests.FileSystemManagement.Path;

/// <summary>
/// Represents a request to get the root of a file system path.
/// </summary>
/// <param name="OriginalPath">The file system path to combine to.</param>
/// <param name="NewPath">The file system path to combine with.</param>
[DebuggerDisplay("OriginalPath: {OriginalPath}, NewPath: {NewPath}")]
public record CombinePathRequest(
    string? OriginalPath,
    string? NewPath
);
