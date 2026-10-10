#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.DTO.MediaLibrary.AudioLibrary.MusicLibrary;

/// <summary>
/// Data transfer object for a MusicBrainz work, the musical composition a track is a recording of.
/// </summary>
/// <remarks>
/// The contributors credited on the work are carried by the contributors of the metadata of the track.
/// </remarks>
/// <param name="MusicBrainzWorkId">The MusicBrainz identifier of the work.</param>
/// <param name="Title">The title of the work.</param>
/// <param name="Type">The MusicBrainz type of the work, if applicable.</param>
/// <param name="Languages">The list of languages of the work.</param>
/// <param name="Iswcs">The list of ISWC (International Standard Musical Work Code) of the work.</param>
[DebuggerDisplay("Title: {Title}")]
public sealed record MusicWorkDto(
    Guid? MusicBrainzWorkId,
    string? Title,
    string? Type,
    List<LanguageInfoDto>? Languages,
    List<string>? Iswcs
);
