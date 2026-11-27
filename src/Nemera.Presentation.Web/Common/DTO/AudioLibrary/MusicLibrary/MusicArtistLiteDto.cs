#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.DTO.AudioLibrary.MusicLibrary;

/// <summary>
/// Data transfer object for a lightweight music artist element, containing only the properties needed by the client for card-style navigation.
/// </summary>
[DebuggerDisplay("Name: {Name}")]
public class MusicArtistLiteDto
{
    /// <summary>
    /// Gets or sets the Id of the artist.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the artist.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the path of the image representing the artist, if available.
    /// </summary>
    public string? CoverPath { get; set; }
}
