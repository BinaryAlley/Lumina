#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.Common;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.DTO.MediaLibrary.AudioLibrary.MusicLibrary;

/// <summary>
/// Data transfer object for the metadata of an album.
/// </summary>
/// <param name="Title">The title of the album. Required.</param>
/// <param name="OriginalTitle">The original title of the album, if different from the current title. Optional.</param>
/// <param name="Description">A brief description or summary of the album. Optional.</param>
/// <param name="ReleaseInfo">The release information, including release date and other relevant details. Required.</param>
/// <param name="Language">The language of the album. Optional.</param>
/// <param name="OriginalLanguage">The original language of the album, if it has been translated. Optional.</param>
/// <param name="Tags">The list of tags that further describe or categorize the album. Required.</param>
/// <param name="Genres">The list of genres associated with the album. Required.</param>
/// <param name="ReleaseType">The type of the release. Optional.</param>
/// <param name="ReleaseStatus">The status of the release. Optional.</param>
/// <param name="TotalDiscs">The number of discs of the release. Optional.</param>
/// <param name="TotalTracks">The number of tracks of the release. Required.</param>
[DebuggerDisplay("Title: {Title}")]
public record AlbumMetadataDto(
    string? Title,
    string? OriginalTitle,
    string? Description,
    ReleaseInfoDto? ReleaseInfo,
    LanguageInfoDto? Language,
    LanguageInfoDto? OriginalLanguage,
    List<TagDto>? Tags,
    List<GenreDto>? Genres,
    MusicReleaseType? ReleaseType,
    MusicReleaseStatus? ReleaseStatus,
    int? TotalDiscs,
    int? TotalTracks
);
