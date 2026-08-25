#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.Common;
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.WrittenContentLibrary;

/// <summary>
/// Fixture class for the <see cref="WrittenContentMetadataDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class WrittenContentMetadataDtoFixture
{
    /// <summary>
    /// Creates a <see cref="WrittenContentMetadataDto"/> with the requested values, leaving unspecified properties as <see langword="null"/>.
    /// </summary>
    /// <param name="title">Optional. The title of the written content.</param>
    /// <param name="originalTitle">Optional. The original title of the written content.</param>
    /// <param name="description">Optional. A brief description or summary of the written content.</param>
    /// <param name="releaseInfo">Optional. The release information of the written content.</param>
    /// <param name="genres">Optional. The genres associated with the written content.</param>
    /// <param name="tags">Optional. The tags that describe or categorize the written content.</param>
    /// <param name="language">Optional. The language in which the written content is written.</param>
    /// <param name="originalLanguage">Optional. The original language of the written content.</param>
    /// <param name="publisher">Optional. The name of the publisher of the written content.</param>
    /// <param name="pageCount">Optional. The number of pages in the written content.</param>
    /// <returns>The created <see cref="WrittenContentMetadataDto"/>.</returns>
    public WrittenContentMetadataDto Create(
        string? title = null,
        string? originalTitle = null,
        string? description = null,
        ReleaseInfoDto? releaseInfo = null,
        List<GenreDto>? genres = null,
        List<TagDto>? tags = null,
        LanguageInfoDto? language = null,
        LanguageInfoDto? originalLanguage = null,
        string? publisher = null,
        int? pageCount = null)
    {
        return new WrittenContentMetadataDto(
            title,
            originalTitle,
            description,
            releaseInfo,
            genres,
            tags,
            language,
            originalLanguage,
            publisher,
            pageCount);
    }

    /// <summary>
    /// Creates a list of <see cref="WrittenContentMetadataDto"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<WrittenContentMetadataDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
