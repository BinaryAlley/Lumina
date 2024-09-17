#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Responses.FileManagement;

/// <summary>
/// Represents a response to the inquiry about the system's path separator.
/// </summary>
/// <param name="Separator">The system's path separator.</param>
[DebuggerDisplay("{Separator}")]
public record PathSeparatorResponse(
    string Separator
);