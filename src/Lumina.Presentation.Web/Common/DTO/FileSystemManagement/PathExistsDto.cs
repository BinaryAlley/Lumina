#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.DTO.FileSystemManagement;

/// <summary>
/// Data transfer object for a response to the inquiry about the existence of a file system path.
/// </summary>
/// <param name="Exists">Whether a file system path exists or not.</param>
[DebuggerDisplay("Exists: {Exists}")]
public record PathExistsDto(
    bool Exists
);
