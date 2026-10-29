#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;

/// <summary>
/// Fixture class for the <see cref="TrackPlaybackManifestDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class TrackPlaybackManifestDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="TrackPlaybackManifestDto"/>.
    /// </summary>
    /// <param name="path">Optional. The file system path of the stored audio file of the track.</param>
    /// <param name="container">Optional. The container of the audio file of the track.</param>
    /// <param name="audioCodec">Optional. The audio codec used by the audio file of the track.</param>
    /// <param name="durationInSeconds">Optional. The duration of the audio of the track, in seconds.</param>
    /// <param name="sampleRate">Optional. The sample rate of the audio of the track, in Hz.</param>
    /// <param name="channels">Optional. The number of audio channels of the track.</param>
    /// <param name="bitDepth">Optional. The bit depth of the audio of the track.</param>
    /// <param name="bitrate">Optional. The bitrate of the audio of the track, in kbps.</param>
    /// <param name="replayGainTrackGain">Optional. The ReplayGain track gain of the track, in decibels.</param>
    /// <param name="replayGainTrackPeak">Optional. The ReplayGain track peak of the track.</param>
    /// <param name="replayGainAlbumGain">Optional. The ReplayGain album gain of the track, in decibels.</param>
    /// <param name="replayGainAlbumPeak">Optional. The ReplayGain album peak of the track.</param>
    /// <param name="includeContainer">Whether the container should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeAudioCodec">Whether the audio codec should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeBitDepth">Whether the bit depth should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeBitrate">Whether the bitrate should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeReplayGain">Whether the ReplayGain values should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="TrackPlaybackManifestDto"/>.</returns>
    public TrackPlaybackManifestDto Create(
        string? path = null,
        string? container = null,
        string? audioCodec = null,
        int? durationInSeconds = null,
        int? sampleRate = null,
        int? channels = null,
        int? bitDepth = null,
        int? bitrate = null,
        decimal? replayGainTrackGain = null,
        decimal? replayGainTrackPeak = null,
        decimal? replayGainAlbumGain = null,
        decimal? replayGainAlbumPeak = null,
        bool includeContainer = true,
        bool includeAudioCodec = true,
        bool includeBitDepth = true,
        bool includeBitrate = true,
        bool includeReplayGain = true)
    {
        return new TrackPlaybackManifestDto(
            path ?? Path.Combine(_faker.System.DirectoryPath(), _faker.System.FileName("flac")),
            includeContainer ? (container ?? _faker.PickRandom("FLAC", "MP3", "OGG", "MP4", "AAC", "WAV")) : null,
            includeAudioCodec ? (audioCodec ?? _faker.PickRandom("FLAC", "MP3", "AAC", "ALAC", "PCM")) : null,
            durationInSeconds ?? _faker.Random.Int(1, 3600),
            sampleRate ?? _faker.PickRandom(44100, 48000, 96000),
            channels ?? _faker.PickRandom(1, 2, 6),
            includeBitDepth ? (bitDepth ?? _faker.Random.Int(8, 32)) : null,
            includeBitrate ? (bitrate ?? _faker.Random.Int(96, 1411)) : null,
            includeReplayGain ? (replayGainTrackGain ?? _faker.Random.Decimal(-20m, 0m)) : null,
            includeReplayGain ? (replayGainTrackPeak ?? _faker.Random.Decimal(0m, 1.5m)) : null,
            includeReplayGain ? (replayGainAlbumGain ?? _faker.Random.Decimal(-20m, 0m)) : null,
            includeReplayGain ? (replayGainAlbumPeak ?? _faker.Random.Decimal(0m, 1.5m)) : null);
    }

    /// <summary>
    /// Creates a list of <see cref="TrackPlaybackManifestDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="TrackPlaybackManifestDto"/> instances.</returns>
    public List<TrackPlaybackManifestDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
