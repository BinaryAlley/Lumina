#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.DTO.AudioLibrary.MusicLibrary;
using Lumina.Presentation.Web.Common.DTO.Common;
using Lumina.Presentation.Web.Common.Enums.AudioLibrary;
using Lumina.Presentation.Web.Fixtures.Common.DTO.Common;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.DTO.AudioLibrary.MusicLibrary;

/// <summary>
/// Fixture class for the <see cref="MusicAlbumMetadataDto"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MusicAlbumMetadataDtoFixture
{
    private readonly Faker _faker = new();
    private readonly ReleaseInfoDtoFixture _releaseInfoDtoFixture = new();
    private readonly LanguageInfoDtoFixture _languageInfoDtoFixture = new();
    private readonly TagDtoFixture _tagDtoFixture = new();
    private readonly GenreDtoFixture _genreDtoFixture = new();

    /// <summary>
    /// Creates a new <see cref="MusicAlbumMetadataDto"/> instance with randomized test data.
    /// </summary>
    /// <param name="title">Optional. The title of the album.</param>
    /// <param name="originalTitle">Optional. The original title of the album.</param>
    /// <param name="description">Optional. A brief description of the album.</param>
    /// <param name="disambiguation">Optional. The disambiguation comment of the album.</param>
    /// <param name="releaseInfo">Optional. The release information of the album.</param>
    /// <param name="language">Optional. The language of the album.</param>
    /// <param name="originalLanguage">Optional. The original language of the album.</param>
    /// <param name="tags">Optional. The tags that describe or categorize the album.</param>
    /// <param name="genres">Optional. The genres associated with the album.</param>
    /// <param name="script">Optional. The script used by the language of the release of the album.</param>
    /// <param name="releaseTypes">Optional. The set of types of the release.</param>
    /// <param name="releaseStatus">Optional. The status of the release.</param>
    /// <param name="totalDiscs">Optional. The number of discs of the release.</param>
    /// <param name="totalTracks">Optional. The number of tracks of the release.</param>
    /// <param name="releaseTitle">Optional. The title of the specific release the album files match.</param>
    /// <param name="includeTitle">Whether the title should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeOriginalTitle">Whether the original title should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeDescription">Whether the description should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeDisambiguation">Whether the disambiguation should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeReleaseInfo">Whether the release information should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeLanguage">Whether the language should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeOriginalLanguage">Whether the original language should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeTags">Whether the tags should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeGenres">Whether the genres should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeScript">Whether the script should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeReleaseTypes">Whether the release types should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeReleaseStatus">Whether the release status should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeTotalDiscs">Whether the total number of discs should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeTotalTracks">Whether the total number of tracks should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeReleaseTitle">Whether the release title should be included, or forced to <see langword="null"/>.</param>
    /// <returns>A configured <see cref="MusicAlbumMetadataDto"/> instance.</returns>
    public MusicAlbumMetadataDto Create(
        string? title = null,
        string? originalTitle = null,
        string? description = null,
        string? disambiguation = null,
        ReleaseInfoDto? releaseInfo = null,
        LanguageInfoDto? language = null,
        LanguageInfoDto? originalLanguage = null,
        List<TagDto>? tags = null,
        List<GenreDto>? genres = null,
        string? script = null,
        List<MusicReleaseType>? releaseTypes = null,
        MusicReleaseStatus? releaseStatus = null,
        int? totalDiscs = null,
        int? totalTracks = null,
        string? releaseTitle = null,
        bool includeTitle = true,
        bool includeOriginalTitle = true,
        bool includeDescription = true,
        bool includeDisambiguation = true,
        bool includeReleaseInfo = true,
        bool includeLanguage = true,
        bool includeOriginalLanguage = true,
        bool includeTags = true,
        bool includeGenres = true,
        bool includeScript = true,
        bool includeReleaseTypes = true,
        bool includeReleaseStatus = true,
        bool includeTotalDiscs = true,
        bool includeTotalTracks = true,
        bool includeReleaseTitle = true)
    {
        return new MusicAlbumMetadataDto
        {
            Title = includeTitle ? (title ?? _faker.Lorem.Sentence(3)) : null,
            OriginalTitle = includeOriginalTitle ? (originalTitle ?? _faker.Lorem.Sentence(3)) : null,
            Description = includeDescription ? (description ?? _faker.Lorem.Paragraph()) : null,
            Disambiguation = includeDisambiguation ? (disambiguation ?? _faker.Lorem.Word()) : null,
            ReleaseInfo = includeReleaseInfo ? (releaseInfo ?? _releaseInfoDtoFixture.Create()) : null,
            Language = includeLanguage ? (language ?? _languageInfoDtoFixture.Create()) : null,
            OriginalLanguage = includeOriginalLanguage ? (originalLanguage ?? _languageInfoDtoFixture.Create()) : null,
            Tags = includeTags ? (tags ?? [.. _tagDtoFixture.CreateMany(_faker.Random.Number(1, 3))]) : null,
            Genres = includeGenres ? (genres ?? [.. _genreDtoFixture.CreateMany(_faker.Random.Number(1, 3))]) : null,
            Script = includeScript ? (script ?? _faker.Lorem.Word()) : null,
            ReleaseTypes = includeReleaseTypes ? (releaseTypes ?? [.. Enumerable.Range(0, _faker.Random.Number(1, 3)).Select(_ => _faker.PickRandom<MusicReleaseType>())]) : null,
            ReleaseStatus = includeReleaseStatus ? (releaseStatus ?? _faker.PickRandom<MusicReleaseStatus>()) : null,
            TotalDiscs = includeTotalDiscs ? (totalDiscs ?? _faker.Random.Number(1, 4)) : null,
            TotalTracks = includeTotalTracks ? (totalTracks ?? _faker.Random.Number(1, 30)) : null,
            ReleaseTitle = includeReleaseTitle ? (releaseTitle ?? _faker.Lorem.Sentence(3)) : null
        };
    }

    /// <summary>
    /// Creates multiple <see cref="MusicAlbumMetadataDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="MusicAlbumMetadataDto"/> instances.</returns>
    public List<MusicAlbumMetadataDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
