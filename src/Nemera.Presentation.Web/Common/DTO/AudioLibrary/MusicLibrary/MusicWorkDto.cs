#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.DTO.AudioLibrary.MusicLibrary;

/// <summary>
/// Data transfer object for a MusicBrainz work, the musical composition a track is a recording of.
/// </summary>
[DebuggerDisplay("Title: {Title}")]
public class MusicWorkDto
{
    /// <summary>
    /// Gets or sets the MusicBrainz identifier of the work.
    /// </summary>
    public Guid? MusicBrainzWorkId { get; set; }

    /// <summary>
    /// Gets or sets the title of the work.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the MusicBrainz type of the work, if applicable.
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// Gets or sets the list of languages of the work.
    /// </summary>
    public List<LanguageInfoDto>? Languages { get; set; }

    /// <summary>
    /// Gets or sets the list of ISWC (International Standard Musical Work Code) of the work.
    /// </summary>
    public List<string>? Iswcs { get; set; }
}
