#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.Common;
using Lumina.Presentation.Web.Common.Enums.AudioLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.DTO.AudioLibrary.MusicLibrary;

/// <summary>
/// Data transfer object for the metadata of a music artist.
/// </summary>
[DebuggerDisplay("Name: {Name}")]
public class MusicArtistMetadataDto
{
    /// <summary>
    /// Gets or sets the name of the artist.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the sort name of the artist, a variant of the name used when sorting artists, if applicable.
    /// </summary>
    public string? SortName { get; set; }

    /// <summary>
    /// Gets or sets the disambiguation comment of the artist, used to distinguish artists with the same name, if applicable.
    /// </summary>
    public string? Disambiguation { get; set; }

    /// <summary>
    /// Gets or sets the type of the artist, if applicable.
    /// </summary>
    public MusicArtistType? Type { get; set; }

    /// <summary>
    /// Gets or sets the gender of the artist, applicable only to artists that are persons or characters, if applicable.
    /// </summary>
    public MusicArtistGender? Gender { get; set; }

    /// <summary>
    /// Gets or sets the ISO 3166-1 alpha-2 code of the country the artist is associated with, if applicable.
    /// </summary>
    public string? Country { get; set; }

    /// <summary>
    /// Gets or sets the area the artist is primarily identified with, if applicable.
    /// </summary>
    public MusicAreaDto? Area { get; set; }

    /// <summary>
    /// Gets or sets the area the artist began in, if applicable.
    /// </summary>
    public MusicAreaDto? BeginArea { get; set; }

    /// <summary>
    /// Gets or sets the area the artist ended in, if applicable.
    /// </summary>
    public MusicAreaDto? EndArea { get; set; }

    /// <summary>
    /// Gets or sets the date the artist started existing, if applicable.
    /// </summary>
    public DateOnly? LifeSpanBegin { get; set; }

    /// <summary>
    /// Gets or sets the date the artist stopped existing, if applicable.
    /// </summary>
    public DateOnly? LifeSpanEnd { get; set; }

    /// <summary>
    /// Gets or sets whether the artist no longer exists.
    /// </summary>
    public bool IsEnded { get; set; }

    /// <summary>
    /// Gets or sets the list of genres associated with the artist.
    /// </summary>
    public List<GenreDto>? Genres { get; set; }

    /// <summary>
    /// Gets or sets the list of tags that further describe or categorize the artist.
    /// </summary>
    public List<TagDto>? Tags { get; set; }

    /// <summary>
    /// Gets or sets the list of alternative names of the artist.
    /// </summary>
    public List<MusicArtistAliasDto>? Aliases { get; set; }
}
