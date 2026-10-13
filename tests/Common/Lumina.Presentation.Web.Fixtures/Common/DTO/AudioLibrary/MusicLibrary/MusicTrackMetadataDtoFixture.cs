#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.DTO.AudioLibrary.MusicLibrary;
using Lumina.Presentation.Web.Common.DTO.Common;
using Lumina.Presentation.Web.Fixtures.Common.DTO.Common;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.DTO.AudioLibrary.MusicLibrary;

/// <summary>
/// Fixture class for the <see cref="MusicTrackMetadataDto"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MusicTrackMetadataDtoFixture
{
    private readonly Faker _faker = new();
    private readonly ReleaseInfoDtoFixture _releaseInfoDtoFixture = new();
    private readonly LanguageInfoDtoFixture _languageInfoDtoFixture = new();
    private readonly TagDtoFixture _tagDtoFixture = new();
    private readonly GenreDtoFixture _genreDtoFixture = new();

    /// <summary>
    /// Creates a new <see cref="MusicTrackMetadataDto"/> instance with randomized test data.
    /// </summary>
    /// <param name="title">Optional. The title of the track.</param>
    /// <param name="originalTitle">Optional. The original title of the track.</param>
    /// <param name="description">Optional. A brief description of the track.</param>
    /// <param name="disambiguation">Optional. The disambiguation comment of the track.</param>
    /// <param name="releaseInfo">Optional. The release information of the track.</param>
    /// <param name="language">Optional. The language of the track.</param>
    /// <param name="originalLanguage">Optional. The original language of the track.</param>
    /// <param name="tags">Optional. The tags that describe or categorize the track.</param>
    /// <param name="genres">Optional. The genres associated with the track.</param>
    /// <param name="isVideo">Optional. Whether the recording of the track is a video recording.</param>
    /// <param name="durationInSeconds">Optional. The duration of the audio of the track in seconds.</param>
    /// <param name="sampleRate">Optional. The sample rate of the audio of the track in Hz.</param>
    /// <param name="channels">Optional. The number of audio channels of the track.</param>
    /// <param name="bitDepth">Optional. The bit depth of the audio of the track.</param>
    /// <param name="audioCodec">Optional. The audio codec used by the track.</param>
    /// <param name="bitrate">Optional. The bitrate of the audio of the track in kbps.</param>
    /// <param name="acoustId">Optional. The AcoustID fingerprint identifier of the audio file of the track.</param>
    /// <param name="replayGainTrackGain">Optional. The ReplayGain track gain of the track in decibels.</param>
    /// <param name="replayGainTrackPeak">Optional. The ReplayGain track peak of the track.</param>
    /// <param name="replayGainAlbumGain">Optional. The ReplayGain album gain of the track in decibels.</param>
    /// <param name="replayGainAlbumPeak">Optional. The ReplayGain album peak of the track.</param>
    /// <param name="includeTitle">Whether the title should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeOriginalTitle">Whether the original title should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeDescription">Whether the description should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeDisambiguation">Whether the disambiguation should be included, or forced to <see langword="null"/>.</param>
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
    /// <param name="includeAcoustId">Whether the AcoustID should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeReplayGainTrackGain">Whether the ReplayGain track gain should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeReplayGainTrackPeak">Whether the ReplayGain track peak should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeReplayGainAlbumGain">Whether the ReplayGain album gain should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeReplayGainAlbumPeak">Whether the ReplayGain album peak should be included, or forced to <see langword="null"/>.</param>
    /// <returns>A configured <see cref="MusicTrackMetadataDto"/> instance.</returns>
    public MusicTrackMetadataDto Create(
        string? title = null,
        string? originalTitle = null,
        string? description = null,
        string? disambiguation = null,
        ReleaseInfoDto? releaseInfo = null,
        LanguageInfoDto? language = null,
        LanguageInfoDto? originalLanguage = null,
        List<TagDto>? tags = null,
        List<GenreDto>? genres = null,
        bool isVideo = false,
        int? durationInSeconds = null,
        int? sampleRate = null,
        int? channels = null,
        int? bitDepth = null,
        string? audioCodec = null,
        int? bitrate = null,
        string? acoustId = null,
        decimal? replayGainTrackGain = null,
        decimal? replayGainTrackPeak = null,
        decimal? replayGainAlbumGain = null,
        decimal? replayGainAlbumPeak = null,
        bool includeTitle = true,
        bool includeOriginalTitle = true,
        bool includeDescription = true,
        bool includeDisambiguation = true,
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
        bool includeBitrate = true,
        bool includeAcoustId = true,
        bool includeReplayGainTrackGain = true,
        bool includeReplayGainTrackPeak = true,
        bool includeReplayGainAlbumGain = true,
        bool includeReplayGainAlbumPeak = true)
    {
        return new MusicTrackMetadataDto
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
            IsVideo = isVideo,
            DurationInSeconds = includeDurationInSeconds ? (durationInSeconds ?? _faker.Random.Number(30, 600)) : null,
            SampleRate = includeSampleRate ? (sampleRate ?? _faker.Random.Number(22050, 192000)) : null,
            Channels = includeChannels ? (channels ?? _faker.Random.Number(1, 8)) : null,
            BitDepth = includeBitDepth ? (bitDepth ?? _faker.Random.Number(8, 32)) : null,
            AudioCodec = includeAudioCodec ? (audioCodec ?? _faker.Lorem.Word()) : null,
            Bitrate = includeBitrate ? (bitrate ?? _faker.Random.Number(64, 1411)) : null,
            AcoustId = includeAcoustId ? (acoustId ?? _faker.Random.String2(32, "0123456789abcdef")) : null,
            ReplayGainTrackGain = includeReplayGainTrackGain ? (replayGainTrackGain ?? _faker.Random.Decimal(-20m, 0m)) : null,
            ReplayGainTrackPeak = includeReplayGainTrackPeak ? (replayGainTrackPeak ?? _faker.Random.Decimal(0m, 1m)) : null,
            ReplayGainAlbumGain = includeReplayGainAlbumGain ? (replayGainAlbumGain ?? _faker.Random.Decimal(-20m, 0m)) : null,
            ReplayGainAlbumPeak = includeReplayGainAlbumPeak ? (replayGainAlbumPeak ?? _faker.Random.Decimal(0m, 1m)) : null
        };
    }

    /// <summary>
    /// Creates multiple <see cref="MusicTrackMetadataDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="MusicTrackMetadataDto"/> instances.</returns>
    public List<MusicTrackMetadataDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
