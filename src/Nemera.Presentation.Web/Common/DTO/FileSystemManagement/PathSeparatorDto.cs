#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Nemera.Presentation.Web.Common.DTO.FileSystemManagement;

/// <summary>
/// Data transfer object for a file system path separator.
/// </summary>
/// <param name="Separator">The system's path separator.</param>
[DebuggerDisplay("Separator: {Separator}")]
public record PathSeparatorDto(
    string Separator
);
