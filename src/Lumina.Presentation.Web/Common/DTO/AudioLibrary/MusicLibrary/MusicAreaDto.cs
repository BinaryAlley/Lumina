#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.DTO.AudioLibrary.MusicLibrary;

/// <summary>
/// Data transfer object for a MusicBrainz area, the geographic area a music artist is associated with.
/// </summary>
[DebuggerDisplay("Name: {Name}")]
public class MusicAreaDto
{
    /// <summary>
    /// Gets or sets the MusicBrainz identifier of the area.
    /// </summary>
    public Guid? MusicBrainzAreaId { get; set; }

    /// <summary>
    /// Gets or sets the name of the area.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the sort name of the area, if applicable.
    /// </summary>
    public string? SortName { get; set; }

    /// <summary>
    /// Gets or sets the disambiguation comment of the area, if applicable.
    /// </summary>
    public string? Disambiguation { get; set; }

    /// <summary>
    /// Gets or sets the MusicBrainz type of the area, if applicable.
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// Gets or sets the ISO 3166 code of the area, if applicable.
    /// </summary>
    public string? Iso3166Code { get; set; }
}
