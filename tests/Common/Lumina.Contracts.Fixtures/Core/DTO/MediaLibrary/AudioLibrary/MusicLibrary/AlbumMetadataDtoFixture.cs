#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.DTO.Common;
using Lumina.Contracts.DTO.MediaContributors;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Contracts.Fixtures.Core.DTO.Common;
using Lumina.Contracts.Fixtures.Core.DTO.MediaContributors;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.AudioLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.AudioLibrary.MusicLibrary;

/// <summary>
/// Fixture class for the <see cref="AlbumMetadataDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class AlbumMetadataDtoFixture
{
    private const int MINIMUM_RELEASE_YEAR = 1900;
    private const int MAXIMUM_RELEASE_YEAR = 2026;

    private readonly Faker _faker = new();
    private readonly GenreDtoFixture _genreDtoFixture = new();
    private readonly TagDtoFixture _tagDtoFixture = new();
    private readonly LanguageInfoDtoFixture _languageInfoDtoFixture = new();
    private readonly ReleaseInfoDtoFixture _releaseInfoDtoFixture = new();
    private readonly MediaContributorDtoFixture _mediaContributorDtoFixture = new();
    private readonly AudioRatingDtoFixture _audioRatingDtoFixture = new();

    /// <summary>
    /// Creates a random valid <see cref="AlbumMetadataDto"/>.
    /// </summary>
    /// <param name="title">Optional. The title of the album.</param>
    /// <param name="originalTitle">Optional. The original title of the album.</param>
    /// <param name="releaseTitle">Optional. The title of the specific release (edition) of the album.</param>
    /// <param name="description">Optional. A brief description or summary of the album.</param>
    /// <param name="disambiguation">Optional. The disambiguation comment of the album.</param>
    /// <param name="releaseInfo">Optional. The release information of the album.</param>
    /// <param name="language">Optional. The language of the album.</param>
    /// <param name="originalLanguage">Optional. The original language of the album.</param>
    /// <param name="tags">Optional. The tags that describe or categorize the album.</param>
    /// <param name="genres">Optional. The genres associated with the album.</param>
    /// <param name="script">Optional. The script used by the language of the release of the album.</param>
    /// <param name="releaseTypes">Optional. The types of the release.</param>
    /// <param name="releaseStatus">Optional. The status of the release.</param>
    /// <param name="mediaFormat">Optional. The physical or digital medium of the album.</param>
    /// <param name="packaging">Optional. The outermost physical packaging of the album.</param>
    /// <param name="totalDiscs">Optional. The number of discs of the release.</param>
    /// <param name="totalTracks">Optional. The number of tracks of the release.</param>
    /// <param name="barcode">Optional. The barcode of the album.</param>
    /// <param name="catalogNumber">Optional. The catalog number of the album.</param>
    /// <param name="label">Optional. The name of the label that issued the album.</param>
    /// <param name="asin">Optional. The ASIN of the album.</param>
    /// <param name="musicBrainzReleaseId">Optional. The MusicBrainz identifier of the release.</param>
    /// <param name="musicBrainzReleaseGroupId">Optional. The MusicBrainz identifier of the release group.</param>
    /// <param name="musicBrainzReleaseArtistId">Optional. The MusicBrainz identifier of the release artist.</param>
    /// <param name="contributors">Optional. The media contributors that performed on the album.</param>
    /// <param name="ratings">Optional. The ratings of the album.</param>
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
    /// <param name="includeMediaFormat">Whether the media format should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includePackaging">Whether the packaging should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeTotalDiscs">Whether the total number of discs should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeTotalTracks">Whether the total number of tracks should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeBarcode">Whether the barcode should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeCatalogNumbers">Whether the catalog number should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeLabel">Whether the label should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeAsin">Whether the ASIN should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeMusicBrainzReleaseId">Whether the MusicBrainz release Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeMusicBrainzReleaseGroupId">Whether the MusicBrainz release group Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeMusicBrainzReleaseArtistId">Whether the MusicBrainz release artist Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeContributors">Whether the contributors should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeRatings">Whether the ratings should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="AlbumMetadataDto"/>.</returns>
    public AlbumMetadataDto Create(
        string? title = null,
        string? originalTitle = null,
        string? releaseTitle = null,
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
        MusicMediaFormat? mediaFormat = null,
        MusicReleasePackaging? packaging = null,
        int? totalDiscs = null,
        int? totalTracks = null,
        string? barcode = null,
        List<string>? catalogNumbers = null,
        string? label = null,
        string? asin = null,
        Guid? musicBrainzReleaseId = null,
        Guid? musicBrainzReleaseGroupId = null,
        Guid? musicBrainzReleaseArtistId = null,
        List<MediaContributorDto>? contributors = null,
        List<AudioRatingDto>? ratings = null,
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
        bool includeMediaFormat = true,
        bool includePackaging = true,
        bool includeTotalDiscs = true,
        bool includeTotalTracks = true,
        bool includeBarcode = true,
        bool includeCatalogNumbers = true,
        bool includeLabel = true,
        bool includeAsin = true,
        bool includeMusicBrainzReleaseId = true,
        bool includeMusicBrainzReleaseGroupId = true,
        bool includeMusicBrainzReleaseArtistId = true,
        bool includeContributors = true,
        bool includeRatings = true,
        bool includeReleaseTitle = true)
    {
        return new AlbumMetadataDto(
            includeTitle ? (title ?? _faker.Music.Genre()) : null,
            includeOriginalTitle ? (originalTitle ?? _faker.Music.Genre()) : null,
            includeDescription ? (description ?? _faker.Lorem.Paragraph()) : null,
            includeDisambiguation ? (disambiguation ?? _faker.Lorem.Sentence()) : null,
            includeReleaseInfo ? (releaseInfo ?? GenerateReleaseInfo()) : null,
            includeLanguage ? (language ?? _languageInfoDtoFixture.Create()) : null,
            includeOriginalLanguage ? (originalLanguage ?? _languageInfoDtoFixture.Create()) : null,
            includeTags ? (tags ?? GenerateDistinctTags()) : null,
            includeGenres ? (genres ?? GenerateDistinctGenres()) : null,
            includeScript ? (script ?? _faker.Random.String2(4)) : null,
            includeReleaseTypes ? (releaseTypes ?? [_faker.PickRandom<MusicReleaseType>()]) : null,
            includeReleaseStatus ? (releaseStatus ?? _faker.PickRandom<MusicReleaseStatus>()) : null,
            includeMediaFormat ? (mediaFormat ?? _faker.PickRandom<MusicMediaFormat>()) : null,
            includePackaging ? (packaging ?? _faker.PickRandom<MusicReleasePackaging>()) : null,
            includeTotalDiscs ? (totalDiscs ?? _faker.Random.Int(1, 3)) : null,
            includeTotalTracks ? (totalTracks ?? _faker.Random.Int(1, 30)) : null,
            includeBarcode ? (barcode ?? _faker.Random.String2(13, "0123456789")) : null,
            includeCatalogNumbers ? (catalogNumbers ?? [_faker.Random.AlphaNumeric(10)]) : null,
            includeLabel ? (label ?? _faker.Company.CompanyName()) : null,
            includeAsin ? (asin ?? _faker.Random.AlphaNumeric(10)) : null,
            includeMusicBrainzReleaseId ? (musicBrainzReleaseId ?? _faker.Random.Guid()) : null,
            includeMusicBrainzReleaseGroupId ? (musicBrainzReleaseGroupId ?? _faker.Random.Guid()) : null,
            includeMusicBrainzReleaseArtistId ? (musicBrainzReleaseArtistId ?? _faker.Random.Guid()) : null,
            includeContributors ? (contributors ?? _mediaContributorDtoFixture.CreateMany(_faker.Random.Int(1, 3))) : null,
            includeRatings ? (ratings ?? _audioRatingDtoFixture.CreateMany(_faker.Random.Int(1, 3))) : null,
            includeReleaseTitle ? (releaseTitle ?? _faker.Commerce.ProductName()) : null);
    }

    /// <summary>
    /// Creates a list of <see cref="AlbumMetadataDto"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<AlbumMetadataDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }

    /// <summary>
    /// Creates a random valid <see cref="ReleaseInfoDto"/> with consistent dates and years.
    /// </summary>
    /// <returns>The created <see cref="ReleaseInfoDto"/>.</returns>
    private ReleaseInfoDto GenerateReleaseInfo()
    {
        int originalReleaseYear = Random.Shared.Next(MINIMUM_RELEASE_YEAR, MAXIMUM_RELEASE_YEAR);
        int reReleaseYear = Random.Shared.Next(originalReleaseYear, MAXIMUM_RELEASE_YEAR);
        return _releaseInfoDtoFixture.Create(
            originalReleaseDate: new DateOnly(originalReleaseYear, 1, 1),
            originalReleaseYear: originalReleaseYear,
            reReleaseDate: new DateOnly(reReleaseYear, 1, 1),
            reReleaseYear: reReleaseYear,
            releaseCountry: _faker.PickRandom<ReleaseCountry>(),
            releaseVersion: _faker.Random.String2(_faker.Random.Number(1, 50)));
    }

    /// <summary>
    /// Creates two genres with distinct names, so that the storage deduplication never collapses them into one.
    /// </summary>
    /// <returns>The created genres.</returns>
    private List<GenreDto> GenerateDistinctGenres()
    {
        return
        [
            _genreDtoFixture.Create(name: CreateUniqueName()),
            _genreDtoFixture.Create(name: CreateUniqueName())
        ];
    }

    /// <summary>
    /// Creates two tags with distinct names, so that the storage deduplication never collapses them into one.
    /// </summary>
    /// <returns>The created tags.</returns>
    private List<TagDto> GenerateDistinctTags()
    {
        return
        [
            _tagDtoFixture.Create(name: CreateUniqueName()),
            _tagDtoFixture.Create(name: CreateUniqueName())
        ];
    }

    /// <summary>
    /// Creates a unique name, so that the generated genres and tags never collapse into one after storage deduplication.
    /// </summary>
    /// <returns>A unique name of at most 50 characters.</returns>
    private string CreateUniqueName()
    {
        string uniqueSuffix = Guid.NewGuid().ToString("N")[..12];
        int maximumWordLength = 50 - uniqueSuffix.Length - 1;
        string word = _faker.Lorem.Word();
        string truncatedWord = word.Length > maximumWordLength ? word[..maximumWordLength] : word;
        return $"{truncatedWord}-{uniqueSuffix}";
    }
}
