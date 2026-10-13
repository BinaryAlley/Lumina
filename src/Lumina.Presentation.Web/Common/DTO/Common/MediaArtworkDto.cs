#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Enums.MediaLibrary;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.DTO.Common;

/// <summary>
/// Data transfer object for a stored piece of artwork of a media item.
/// </summary>
[DebuggerDisplay("Type: {Type}, Ordinal: {Ordinal}, Path: {Path}")]
public class MediaArtworkDto
{
    /// <summary>
    /// Gets or sets the type of the artwork, like the cover or the back of the media item.
    /// </summary>
    public ArtworkType Type { get; set; }

    /// <summary>
    /// Gets or sets the ordinal of the artwork within its type, so that multiple artworks of the same type can be ordered.
    /// </summary>
    public int Ordinal { get; set; }

    /// <summary>
    /// Gets or sets the relative path of the stored artwork file.
    /// </summary>
    public string Path { get; set; } = string.Empty;
}
