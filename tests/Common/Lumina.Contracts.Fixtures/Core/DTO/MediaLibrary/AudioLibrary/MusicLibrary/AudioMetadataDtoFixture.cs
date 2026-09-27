#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.DTO.Common;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Contracts.Fixtures.Core.DTO.Common;
using Lumina.Domain.SharedKernel.Common.Enums.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.AudioLibrary.MusicLibrary;

/// <summary>
/// Fixture class for the <see cref="AudioMetadataDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class AudioMetadataDtoFixture
{
    private const int MINIMUM_RELEASE_YEAR = 1900;
    private const int MAXIMUM_RELEASE_YEAR = 2026;

    private readonly Faker _faker = new();
    private readonly GenreDtoFixture _genreDtoFixture = new();
    private readonly TagDtoFixture _tagDtoFixture = new();
    private readonly LanguageInfoDtoFixture _languageInfoDtoFixture = new();
    private readonly ReleaseInfoDtoFixture _releaseInfoDtoFixture = new();

    /// <summary>
    /// Creates a random valid <see cref="AudioMetadataDto"/>.
    /// </summary>
    /// <param name="title">Optional. The title of the audio.</param>
    /// <param name="originalTitle">Optional. The original title of the audio.</param>
    /// <param name="description">Optional. A brief description or summary of the audio.</param>
    /// <param name="releaseInfo">Optional. The release information of the audio.</param>
    /// <param name="language">Optional. The language of the audio.</param>
    /// <param name="originalLanguage">Optional. The original language of the audio.</param>
    /// <param name="tags">Optional. The tags that describe or categorize the audio.</param>
    /// <param name="genres">Optional. The genres associated with the audio.</param>
    /// <param name="durationInSeconds">Optional. The duration of the audio, in seconds.</param>
    /// <param name="sampleRate">Optional. The sample rate of the audio, in Hz.</param>
    /// <param name="channels">Optional. The number of audio channels.</param>
    /// <param name="bitDepth">Optional. The bit depth of the audio.</param>
    /// <param name="audioCodec">Optional. The audio codec used.</param>
    /// <param name="bitrate">Optional. The bitrate of the audio, in kbps.</param>
    /// <param name="includeTitle">Whether the title should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeOriginalTitle">Whether the original title should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeDescription">Whether the description should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeReleaseInfo">Whether the release information should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeLanguage">Whether the language should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeOriginalLanguage">Whether the original language should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeTags">Whether the tags should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeGenres">Whether the genres should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeDurationInSeconds">Whether the duration should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeSampleRate">Whether the sample rate should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeChannels">Whether the channels should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeBitDepth">Whether the bit depth should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeAudioCodec">Whether the audio codec should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeBitrate">Whether the bitrate should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="AudioMetadataDto"/>.</returns>
    public AudioMetadataDto Create(
        string? title = null,
        string? originalTitle = null,
        string? description = null,
        ReleaseInfoDto? releaseInfo = null,
        LanguageInfoDto? language = null,
        LanguageInfoDto? originalLanguage = null,
        List<TagDto>? tags = null,
        List<GenreDto>? genres = null,
        int? durationInSeconds = null,
        int? sampleRate = null,
        int? channels = null,
        int? bitDepth = null,
        string? audioCodec = null,
        int? bitrate = null,
        bool includeTitle = true,
        bool includeOriginalTitle = true,
        bool includeDescription = true,
        bool includeReleaseInfo = true,
        bool includeLanguage = true,
        bool includeOriginalLanguage = true,
        bool includeTags = true,
        bool includeGenres = true,
        bool includeDurationInSeconds = true,
        bool includeSampleRate = true,
        bool includeChannels = true,
        bool includeBitDepth = true,
        bool includeAudioCodec = true,
        bool includeBitrate = true)
    {
        return new AudioMetadataDto(
            includeTitle ? (title ?? _faker.Music.Genre()) : null,
            includeOriginalTitle ? (originalTitle ?? _faker.Music.Genre()) : null,
            includeDescription ? (description ?? _faker.Lorem.Paragraph()) : null,
            includeReleaseInfo ? (releaseInfo ?? GenerateReleaseInfo()) : null,
            includeLanguage ? (language ?? _languageInfoDtoFixture.Create()) : null,
            includeOriginalLanguage ? (originalLanguage ?? _languageInfoDtoFixture.Create()) : null,
            includeTags ? (tags ?? GenerateDistinctTags()) : null,
            includeGenres ? (genres ?? GenerateDistinctGenres()) : null,
            includeDurationInSeconds ? (durationInSeconds ?? _faker.Random.Int(60, 7200)) : null,
            includeSampleRate ? (sampleRate ?? _faker.Random.Int(8000, 192000)) : null,
            includeChannels ? (channels ?? _faker.Random.Int(1, 8)) : null,
            includeBitDepth ? (bitDepth ?? _faker.Random.Int(8, 32)) : null,
            includeAudioCodec ? (audioCodec ?? _faker.Random.ArrayElement(["FLAC", "MP3", "AAC", "ALAC"])) : null,
            includeBitrate ? (bitrate ?? _faker.Random.Int(96, 1411)) : null);
    }

    /// <summary>
    /// Creates a list of <see cref="AudioMetadataDto"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<AudioMetadataDto> CreateMany(int count = 3)
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
