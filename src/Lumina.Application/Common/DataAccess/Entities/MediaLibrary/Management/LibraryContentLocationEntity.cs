#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;

/// <summary>
/// Repository entity for a media library content location.
/// </summary>
[DebuggerDisplay("Path: {Path}")]
public class LibraryContentLocationEntity
{
    /// <summary>
    /// Gets or sets the path of the media library content location.
    /// </summary>
    public required string Path { get; set; }
}
