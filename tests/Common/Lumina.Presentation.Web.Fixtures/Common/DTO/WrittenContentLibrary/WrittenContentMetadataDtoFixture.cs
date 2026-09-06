#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.DTO.Common;
using Lumina.Presentation.Web.Common.DTO.WrittenContentLibrary;
using Lumina.Presentation.Web.Fixtures.Common.DTO.Common;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.DTO.WrittenContentLibrary;

/// <summary>
/// Fixture class for generating <see cref="WrittenContentMetadataDto"/> test data.
/// </summary>
[ExcludeFromCodeCoverage]
public class WrittenContentMetadataDtoFixture
{
    private readonly Faker _faker = new();
    private readonly GenreDtoFixture _genreDtoFixture = new();
    private readonly TagDtoFixture _tagDtoFixture = new();
    private readonly LanguageInfoDtoFixture _languageInfoDtoFixture = new();
    private readonly ReleaseInfoDtoFixture _releaseInfoDtoFixture = new();

    /// <summary>
    /// Creates a new <see cref="WrittenContentMetadataDto"/> instance with randomized test data.
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
    /// <returns>A configured <see cref="WrittenContentMetadataDto"/> instance.</returns>
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
        return new WrittenContentMetadataDto
        {
            Title = title ?? _faker.Lorem.Sentence(3),
            OriginalTitle = originalTitle ?? _faker.Lorem.Sentence(3),
            Description = description ?? _faker.Lorem.Paragraph(),
            ReleaseInfo = releaseInfo ?? _releaseInfoDtoFixture.Create(),
            Genres = genres ?? [.. _genreDtoFixture.CreateMany(_faker.Random.Number(1, 3))],
            Tags = tags ?? [.. _tagDtoFixture.CreateMany(_faker.Random.Number(1, 3))],
            Language = language ?? _languageInfoDtoFixture.Create(),
            OriginalLanguage = originalLanguage ?? _languageInfoDtoFixture.Create(),
            Publisher = publisher ?? _faker.Company.CompanyName(),
            PageCount = pageCount ?? _faker.Random.Int(100, 1000)
        };
    }

    /// <summary>
    /// Creates multiple <see cref="WrittenContentMetadataDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="WrittenContentMetadataDto"/> instances.</returns>
    public List<WrittenContentMetadataDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
