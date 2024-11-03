#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Entities.Common;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Entities.MediaLibrary.WrittenContentLibrary;

/// <summary>
/// Repository entity for written content metadata.
/// </summary>
/// <param name="Title">The title of the written content.</param>
/// <param name="OriginalTitle">The original title of the written content, if different from the current title.</param>
/// <param name="Description">Gets a brief description or summary of the written content.</param>
/// <param name="ReleaseInfo">The release information, including release date and other relevant details.</param>
/// <param name="Genres">The list of genres associated with the written content.</param>
/// <param name="Tags">The list of tags that further describe or categorize the written content.</param>
/// <param name="Language">The language in which the written content is written.</param>
/// <param name="OriginalLanguage">The original language of the written content, if it has been translated.</param>
/// <param name="Publisher">The name of the publisher of the written content.</param>
/// <param name="PageCount">The number of pages in the written content.</param>
[DebuggerDisplay("Title: {Title}")]
public record WrittenContentMetadataEntity(
    string? Title,
    string? OriginalTitle,
    string? Description,
    ReleaseInfoEntity? ReleaseInfo,
    List<GenreEntity>? Genres,
    List<TagEntity>? Tags,
    LanguageInfoEntity? Language,
    LanguageInfoEntity? OriginalLanguage,
    string? Publisher,
    int? PageCount
);
