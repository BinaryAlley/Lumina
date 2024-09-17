#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Responses.FileManagement;

/// <summary>
/// Represents a response containing the result of splitting a file system path.
/// </summary>
/// <param name="PathSegments">The returned path segments.</param>
[DebuggerDisplay("{PathSegments}")]
public record SplitPathResponse(
    string[] PathSegments
);