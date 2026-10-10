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
    private readonly MusicWorkDtoFixture _musicWorkDtoFixture = new();
    private readonly IsrcDtoFixture _isrcDtoFixture = new();
    private readonly MoodDtoFixture _moodDtoFixture = new();
    private readonly MediaContributorDtoFixture _mediaContributorDtoFixture = new();
    private readonly AudioRatingDtoFixture _audioRatingDtoFixture = new();

    /// <summary>
    /// Creates a random valid <see cref="AudioMetadataDto"/>.
    /// </summary>
    /// <param name="title">Optional. The title of the audio.</param>
    /// <param name="originalTitle">Optional. The original title of the audio.</param>
    /// <param name="description">Optional. A brief description or summary of the audio.</param>
    /// <param name="disambiguation">Optional. The disambiguation comment of the audio.</param>
    /// <param name="releaseInfo">Optional. The release information of the audio.</param>
    /// <param name="language">Optional. The language of the audio.</param>
    /// <param name="originalLanguage">Optional. The original language of the audio.</param>
    /// <param name="tags">Optional. The tags that describe or categorize the audio.</param>
    /// <param name="genres">Optional. The genres associated with the audio.</param>
    /// <param name="script">Optional. The script used by the language of the audio.</param>
    /// <param name="key">Optional. The musical key of the audio.</param>
    /// <param name="bpm">Optional. The tempo of the audio, in beats per minute.</param>
    /// <param name="isVideo">Optional. Whether the recording of the audio is a video recording.</param>
    /// <param name="work">Optional. The work the audio is a recording of.</param>
    /// <param name="isrcs">Optional. The ISRC codes of the audio.</param>
    /// <param name="moods">Optional. The moods of the audio.</param>
    /// <param name="durationInSeconds">Optional. The duration of the audio, in seconds.</param>
    /// <param name="sampleRate">Optional. The sample rate of the audio, in Hz.</param>
    /// <param name="channels">Optional. The number of audio channels.</param>
    /// <param name="bitDepth">Optional. The bit depth of the audio.</param>
    /// <param name="audioCodec">Optional. The audio codec used.</param>
    /// <param name="bitrate">Optional. The bitrate of the audio, in kbps.</param>
    /// <param name="musicBrainzRecordingId">Optional. The MusicBrainz identifier of the recording.</param>
    /// <param name="musicBrainzTrackId">Optional. The MusicBrainz identifier of the track.</param>
    /// <param name="contributors">Optional. The media contributors that performed on the audio.</param>
    /// <param name="ratings">Optional. The ratings of the audio.</param>
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
    /// <param name="includeKey">Whether the musical key should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeBpm">Whether the tempo should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeWork">Whether the work should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeIsrcs">Whether the ISRC codes should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeMoods">Whether the moods should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeDurationInSeconds">Whether the duration should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeSampleRate">Whether the sample rate should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeChannels">Whether the channels should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeBitDepth">Whether the bit depth should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeAudioCodec">Whether the audio codec should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeBitrate">Whether the bitrate should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeMusicBrainzRecordingId">Whether the MusicBrainz recording Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeMusicBrainzTrackId">Whether the MusicBrainz track Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeContributors">Whether the contributors should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeRatings">Whether the ratings should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="AudioMetadataDto"/>.</returns>
    public AudioMetadataDto Create(
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
        MusicKey? key = null,
        int? bpm = null,
        bool? isVideo = null,
        MusicWorkDto? work = null,
        List<IsrcDto>? isrcs = null,
        List<MoodDto>? moods = null,
        int? durationInSeconds = null,
        int? sampleRate = null,
        int? channels = null,
        int? bitDepth = null,
        string? audioCodec = null,
        int? bitrate = null,
        Guid? musicBrainzRecordingId = null,
        Guid? musicBrainzTrackId = null,
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
        bool includeKey = true,
        bool includeBpm = true,
        bool includeWork = true,
        bool includeIsrcs = true,
        bool includeMoods = true,
        bool includeDurationInSeconds = true,
        bool includeSampleRate = true,
        bool includeChannels = true,
        bool includeBitDepth = true,
        bool includeAudioCodec = true,
        bool includeBitrate = true,
        bool includeMusicBrainzRecordingId = true,
        bool includeMusicBrainzTrackId = true,
        bool includeContributors = true,
        bool includeRatings = true)
    {
        return new AudioMetadataDto(
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
            includeKey ? (key ?? _faker.PickRandom<MusicKey>()) : null,
            includeBpm ? (bpm ?? _faker.Random.Int(40, 220)) : null,
            isVideo ?? _faker.Random.Bool(),
            includeWork ? (work ?? _musicWorkDtoFixture.Create()) : null,
            includeIsrcs ? (isrcs ?? _isrcDtoFixture.CreateMany(_faker.Random.Int(1, 3))) : null,
            includeMoods ? (moods ?? _moodDtoFixture.CreateMany(_faker.Random.Int(1, 3))) : null,
            includeDurationInSeconds ? (durationInSeconds ?? _faker.Random.Int(60, 7200)) : null,
            includeSampleRate ? (sampleRate ?? _faker.Random.Int(8000, 192000)) : null,
            includeChannels ? (channels ?? _faker.Random.Int(1, 8)) : null,
            includeBitDepth ? (bitDepth ?? _faker.Random.Int(8, 32)) : null,
            includeAudioCodec ? (audioCodec ?? _faker.Random.ArrayElement(["FLAC", "MP3", "AAC", "ALAC"])) : null,
            includeBitrate ? (bitrate ?? _faker.Random.Int(96, 1411)) : null,
            includeMusicBrainzRecordingId ? (musicBrainzRecordingId ?? _faker.Random.Guid()) : null,
            includeMusicBrainzTrackId ? (musicBrainzTrackId ?? _faker.Random.Guid()) : null,
            includeContributors ? (contributors ?? _mediaContributorDtoFixture.CreateMany(_faker.Random.Int(1, 3))) : null,
            includeRatings ? (ratings ?? _audioRatingDtoFixture.CreateMany(_faker.Random.Int(1, 3))) : null);
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
