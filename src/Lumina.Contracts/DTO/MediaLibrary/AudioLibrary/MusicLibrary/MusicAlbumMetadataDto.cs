#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.Common;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.DTO.MediaLibrary.AudioLibrary.MusicLibrary;

/// <summary>
/// Data transfer object for the metadata of an album, as used by the API.
/// </summary>
/// <remarks>
/// The identifiers, the contributors and the ratings of the album are kept alongside this object, as siblings, rather than inside it.
/// </remarks>
/// <param name="Title">The title of the album. Required.</param>
/// <param name="OriginalTitle">The original title of the album, if different from the current title. Optional.</param>
/// <param name="Description">A brief description or summary of the album. Optional.</param>
/// <param name="Disambiguation">The disambiguation comment of the album, used to distinguish albums with the same title. Optional.</param>
/// <param name="ReleaseInfo">The release information, including release date and other relevant details. Required.</param>
/// <param name="Language">The language of the album. Optional.</param>
/// <param name="OriginalLanguage">The original language of the album, if it has been translated. Optional.</param>
/// <param name="Tags">The list of tags that further describe or categorize the album. Required.</param>
/// <param name="Genres">The list of genres associated with the album. Required.</param>
/// <param name="Script">The script used by the language of the release of the album. Optional.</param>
/// <param name="ReleaseTypes">The set of types of the release. Optional.</param>
/// <param name="ReleaseStatus">The status of the release. Optional.</param>
/// <param name="TotalDiscs">The number of discs of the release. Optional.</param>
/// <param name="TotalTracks">The number of tracks of the release. Required.</param>
/// <param name="ReleaseTitle">The title of the specific release (edition) the album files match, if different from the release group title. Optional.</param>
[DebuggerDisplay("Title: {Title}")]
public record MusicAlbumMetadataDto(
    string? Title,
    string? OriginalTitle,
    string? Description,
    string? Disambiguation,
    ReleaseInfoDto? ReleaseInfo,
    LanguageInfoDto? Language,
    LanguageInfoDto? OriginalLanguage,
    List<TagDto>? Tags,
    List<GenreDto>? Genres,
    string? Script,
    List<MusicReleaseType>? ReleaseTypes,
    MusicReleaseStatus? ReleaseStatus,
    int? TotalDiscs,
    int? TotalTracks,
    string? ReleaseTitle = null
);
