#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.DTO.MediaLibrary.Management;

/// <summary>
/// Data transfer object for a media library content location.
/// </summary>
[DebuggerDisplay("Path: {Path}")]
public record LibraryContentLocationDto(
    string Path
);
