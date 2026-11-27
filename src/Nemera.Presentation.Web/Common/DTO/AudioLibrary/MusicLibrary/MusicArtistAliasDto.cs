#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.DTO.AudioLibrary.MusicLibrary;

/// <summary>
/// Data transfer object for an alternative name of a music artist.
/// </summary>
[DebuggerDisplay("Name: {Name}")]
public class MusicArtistAliasDto
{
    /// <summary>
    /// Gets or sets the name of the alias.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the sort name of the alias, if applicable.
    /// </summary>
    public string? SortName { get; set; }

    /// <summary>
    /// Gets or sets the type of the alias, if applicable.
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// Gets or sets the locale the alias is used in, if applicable.
    /// </summary>
    public string? Locale { get; set; }

    /// <summary>
    /// Gets or sets whether this is the primary alias of the artist.
    /// </summary>
    public bool IsPrimary { get; set; }

    /// <summary>
    /// Gets or sets the date the alias began being used, if applicable.
    /// </summary>
    public DateOnly? BeginDate { get; set; }

    /// <summary>
    /// Gets or sets the date the alias stopped being used, if applicable.
    /// </summary>
    public DateOnly? EndDate { get; set; }

    /// <summary>
    /// Gets or sets whether the alias is no longer used.
    /// </summary>
    public bool IsEnded { get; set; }
}
