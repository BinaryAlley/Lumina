#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Nemera.Contracts.Responses.FileSystemManagement.Path;

/// <summary>
/// Represents a response to the inquiry about the system's path separator.
/// </summary>
/// <param name="Separator">The system's path separator.</param>
[DebuggerDisplay("Separator: {Separator}")]
public record PathSeparatorResponse(
    string Separator
);
