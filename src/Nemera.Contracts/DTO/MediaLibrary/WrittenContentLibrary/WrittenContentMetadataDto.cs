#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.Common;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary;

/// <summary>
/// Data transfer object for written content metadata.
/// </summary>
/// <param name="Title">The title of the written content. Required.</param>
/// <param name="OriginalTitle">The original title of the written content, if different from the current title. Optional.</param>
/// <param name="Description">Gets a brief description or summary of the written content. Optional.</param>
/// <param name="ReleaseInfo">The release information, including release date and other relevant details. Required.</param>
/// <param name="Genres">The list of genres associated with the written content. Required.</param>
/// <param name="Tags">The list of tags that further describe or categorize the written content. Required.</param>
/// <param name="Language">The language in which the written content is written. Optional.</param>
/// <param name="OriginalLanguage">The original language of the written content, if it has been translated. Optional.</param>
/// <param name="Publisher">The name of the publisher of the written content. Optional.</param>
/// <param name="PageCount">The number of pages in the written content. Optional.</param>
[DebuggerDisplay("Title: {Title}")]
public record WrittenContentMetadataDto(
    string? Title,
    string? OriginalTitle,
    string? Description,
    ReleaseInfoDto? ReleaseInfo,
    List<GenreDto>? Genres,
    List<TagDto>? Tags,
    LanguageInfoDto? Language,
    LanguageInfoDto? OriginalLanguage,
    string? Publisher,
    int? PageCount
);
