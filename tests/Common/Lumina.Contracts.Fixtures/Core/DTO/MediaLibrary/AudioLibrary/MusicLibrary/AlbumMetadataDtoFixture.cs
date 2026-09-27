#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.DTO.Common;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Contracts.Fixtures.Core.DTO.Common;
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

    /// <summary>
    /// Creates a random valid <see cref="AlbumMetadataDto"/>.
    /// </summary>
    /// <param name="title">Optional. The title of the album.</param>
    /// <param name="originalTitle">Optional. The original title of the album.</param>
    /// <param name="description">Optional. A brief description or summary of the album.</param>
    /// <param name="releaseInfo">Optional. The release information of the album.</param>
    /// <param name="language">Optional. The language of the album.</param>
    /// <param name="originalLanguage">Optional. The original language of the album.</param>
    /// <param name="tags">Optional. The tags that describe or categorize the album.</param>
    /// <param name="genres">Optional. The genres associated with the album.</param>
    /// <param name="releaseType">Optional. The type of the release.</param>
    /// <param name="releaseStatus">Optional. The status of the release.</param>
    /// <param name="totalDiscs">Optional. The number of discs of the release.</param>
    /// <param name="totalTracks">Optional. The number of tracks of the release.</param>
    /// <param name="includeTitle">Whether the title should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeOriginalTitle">Whether the original title should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeDescription">Whether the description should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeReleaseInfo">Whether the release information should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeLanguage">Whether the language should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeOriginalLanguage">Whether the original language should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeTags">Whether the tags should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeGenres">Whether the genres should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeReleaseType">Whether the release type should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeReleaseStatus">Whether the release status should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeTotalDiscs">Whether the total number of discs should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeTotalTracks">Whether the total number of tracks should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="AlbumMetadataDto"/>.</returns>
    public AlbumMetadataDto Create(
        string? title = null,
        string? originalTitle = null,
        string? description = null,
        ReleaseInfoDto? releaseInfo = null,
        LanguageInfoDto? language = null,
        LanguageInfoDto? originalLanguage = null,
        List<TagDto>? tags = null,
        List<GenreDto>? genres = null,
        MusicReleaseType? releaseType = null,
        MusicReleaseStatus? releaseStatus = null,
        int? totalDiscs = null,
        int? totalTracks = null,
        bool includeTitle = true,
        bool includeOriginalTitle = true,
        bool includeDescription = true,
        bool includeReleaseInfo = true,
        bool includeLanguage = true,
        bool includeOriginalLanguage = true,
        bool includeTags = true,
        bool includeGenres = true,
        bool includeReleaseType = true,
        bool includeReleaseStatus = true,
        bool includeTotalDiscs = true,
        bool includeTotalTracks = true)
    {
        return new AlbumMetadataDto(
            includeTitle ? (title ?? _faker.Music.Genre()) : null,
            includeOriginalTitle ? (originalTitle ?? _faker.Music.Genre()) : null,
            includeDescription ? (description ?? _faker.Lorem.Paragraph()) : null,
            includeReleaseInfo ? (releaseInfo ?? GenerateReleaseInfo()) : null,
            includeLanguage ? (language ?? _languageInfoDtoFixture.Create()) : null,
            includeOriginalLanguage ? (originalLanguage ?? _languageInfoDtoFixture.Create()) : null,
            includeTags ? (tags ?? GenerateDistinctTags()) : null,
            includeGenres ? (genres ?? GenerateDistinctGenres()) : null,
            includeReleaseType ? (releaseType ?? _faker.PickRandom<MusicReleaseType>()) : null,
            includeReleaseStatus ? (releaseStatus ?? _faker.PickRandom<MusicReleaseStatus>()) : null,
            includeTotalDiscs ? (totalDiscs ?? _faker.Random.Int(1, 3)) : null,
            includeTotalTracks ? (totalTracks ?? _faker.Random.Int(1, 30)) : null);
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
