#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;

/// <summary>
/// Repository entity for a MusicBrainz area, the geographic area a music artist is associated with.
/// </summary>
/// <param name="MusicBrainzAreaId">The MusicBrainz identifier of the area.</param>
/// <param name="Name">The name of the area.</param>
/// <param name="SortName">The sort name of the area, if applicable.</param>
/// <param name="Disambiguation">The disambiguation comment of the area, if applicable.</param>
/// <param name="Type">The MusicBrainz type of the area, if applicable.</param>
/// <param name="Iso3166Code">The ISO 3166 code of the area, if applicable.</param>
[DebuggerDisplay("Name: {Name}")]
public record MusicAreaEntity(
    Guid MusicBrainzAreaId,
    string Name,
    string? SortName,
    string? Disambiguation,
    string? Type,
    string? Iso3166Code
);
