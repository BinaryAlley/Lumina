#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Models.Common;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Models.WrittenContentLibrary;

/// <summary>
/// Represents written content metadata.
/// </summary>
/// <param name="Title">Gets the title of the written content.</param>
/// <param name="OriginalTitle">Gets the original title of the written content, if different from the current title.</param>
/// <param name="Description">Gets a brief description or summary of the written content.</param>
/// <param name="ReleaseInfo">Gets the release information, including release date and other relevant details.</param>
/// <param name="Genres">Gets the list of genres associated with the written content.</param>
/// <param name="Tags">Gets the list of tags that further describe or categorize the written content.</param>
/// <param name="Language">Gets the language in which the written content is written.</param>
/// <param name="OriginalLanguage">Gets the original language of the written content, if it has been translated.</param>
/// <param name="Publisher">Gets the name of the publisher of the written content.</param>
/// <param name="PageCount">Gets the number of pages in the written content.</param>
[DebuggerDisplay("{Title}")]
public record WrittenContentMetadataModel(
    string? Title,
    string? OriginalTitle,
    string? Description,
    ReleaseInfoModel? ReleaseInfo,
    List<GenreModel>? Genres,
    List<TagModel>? Tags,
    LanguageInfoModel? Language,
    LanguageInfoModel? OriginalLanguage,
    string? Publisher,
    int? PageCount
);