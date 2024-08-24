#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Models.Common;
#endregion

namespace Lumina.Application.Common.Models.Books;

/// <summary>
/// Represents written content metadata.
/// </summary>
public record WrittenContentMetadataDto(
    string Title,
    string? OriginalTitle,
    string? Description,
    ReleaseInfoDto ReleaseInfo,
    List<GenreDto> Genres,
    List<TagDto> Tags,
    LanguageInfoDto? Language,
    LanguageInfoDto? OriginalLanguage,
    string? Publisher, 
    int? PageCount
);