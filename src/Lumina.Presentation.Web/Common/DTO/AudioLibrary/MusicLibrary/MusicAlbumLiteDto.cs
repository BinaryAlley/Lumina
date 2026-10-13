#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Enums.AudioLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.DTO.AudioLibrary.MusicLibrary;

/// <summary>
/// Data transfer object for a lightweight album element, containing only the properties needed by the client for card-style navigation.
/// </summary>
[DebuggerDisplay("Title: {Title}")]
public class MusicAlbumLiteDto
{
    /// <summary>
    /// Gets or sets the Id of the album.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the title of the album.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the number of tracks of the release.
    /// </summary>
    public int TotalTracks { get; set; }

    /// <summary>
    /// Gets or sets the path of the image representing the album, if available.
    /// </summary>
    public string? CoverPath { get; set; }

    /// <summary>
    /// Gets or sets the release year of the album (the re-release year, if available, or the original release year), if known.
    /// </summary>
    public int? ReleaseYear { get; set; }

    /// <summary>
    /// Gets or sets the set of types of the release.
    /// </summary>
    public List<MusicReleaseType>? ReleaseTypes { get; set; }
}
