#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Playback;
using Lumina.Domain.SharedKernel.Common.Enums.Playback;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Playback;

/// <summary>
/// Fixture class for the <see cref="NormalizedTrackRequestDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class NormalizedTrackRequestDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="NormalizedTrackRequestDto"/>.
    /// </summary>
    /// <param name="trackId">Optional. The Id of the track whose audio is normalized.</param>
    /// <param name="userId">Optional. The Id of the user whose request starts the production.</param>
    /// <param name="sourcePath">Optional. The file system path of the stored audio file of the track.</param>
    /// <param name="allowedRoots">Optional. The content locations the source path is allowed to be inside.</param>
    /// <param name="sourceContainer">Optional. The container of the stored audio file, as captured by the probe.</param>
    /// <param name="mode">Optional. The way the audio is delivered.</param>
    /// <param name="container">Optional. The container of the delivered audio.</param>
    /// <param name="codec">Optional. The codec of the delivered audio.</param>
    /// <param name="segmentDurationSeconds">Optional. The duration of each segment of the delivered audio, in seconds.</param>
    /// <param name="durationInSeconds">Optional. The duration of the audio of the track, in seconds.</param>
    /// <param name="bitrate">Optional. The stored audio bitrate of the track, in kbps.</param>
    /// <param name="sampleRate">Optional. The sample rate of the stored audio of the track, in Hz.</param>
    /// <param name="channels">Optional. The number of audio channels of the track.</param>
    /// <param name="bitDepth">Optional. The bit depth of the stored audio of the track.</param>
    /// <param name="quality">Optional. The quality cap applied to the delivered audio when it has to be re-encoded.</param>
    /// <param name="sourceLength">Optional. The length the source file had when the delivery was declared, in bytes.</param>
    /// <param name="sourceLastModifiedUtc">Optional. The last modification time the source file had when the delivery was declared.</param>
    /// <param name="includeSourceContainer">Whether the source container should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeBitrate">Whether the bitrate should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeBitDepth">Whether the bit depth should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="NormalizedTrackRequestDto"/>.</returns>
    public NormalizedTrackRequestDto Create(
        Guid? trackId = null,
        Guid? userId = null,
        string? sourcePath = null,
        IReadOnlyList<string>? allowedRoots = null,
        string? sourceContainer = null,
        PlaybackDeliveryMode? mode = null,
        string? container = null,
        string? codec = null,
        int? segmentDurationSeconds = null,
        int? durationInSeconds = null,
        int? bitrate = null,
        int? sampleRate = null,
        int? channels = null,
        int? bitDepth = null,
        PlaybackStreamingQuality? quality = null,
        long? sourceLength = null,
        DateTime? sourceLastModifiedUtc = null,
        bool includeSourceContainer = true,
        bool includeBitrate = true,
        bool includeBitDepth = true)
    {
        IReadOnlyList<string> resolvedAllowedRoots = allowedRoots ?? [Path.Combine(Path.GetTempPath(), "lumina-tests", _faker.Random.AlphaNumeric(12))];
        string resolvedSourcePath = sourcePath ?? (resolvedAllowedRoots.Count > 0
            ? Path.Combine(resolvedAllowedRoots[0], "track-" + _faker.Random.AlphaNumeric(8) + ".flac")
            : Path.Combine(Path.GetTempPath(), "track-" + _faker.Random.AlphaNumeric(8) + ".flac"));

        return new NormalizedTrackRequestDto(
            trackId ?? _faker.Random.Guid(),
            userId ?? _faker.Random.Guid(),
            resolvedSourcePath,
            resolvedAllowedRoots,
            includeSourceContainer ? (sourceContainer ?? _faker.PickRandom("FLAC", "MP3", "OGG", "MP4", "WAV")) : null,
            mode ?? _faker.PickRandom(PlaybackDeliveryMode.Remux, PlaybackDeliveryMode.Transcode),
            container ?? _faker.PickRandom("MP3", "AAC", "FLAC", "WAV", "OGG"),
            codec ?? _faker.PickRandom("MP3", "AAC", "FLAC", "PCM", "OPUS"),
            segmentDurationSeconds ?? _faker.Random.Int(1, 30),
            durationInSeconds ?? _faker.Random.Int(1, 3600),
            includeBitrate ? (bitrate ?? _faker.Random.Int(96, 1411)) : null,
            sampleRate ?? _faker.PickRandom(44100, 48000, 96000),
            channels ?? _faker.PickRandom(1, 2, 6),
            includeBitDepth ? (bitDepth ?? _faker.Random.Int(16, 24)) : null,
            quality ?? _faker.PickRandom<PlaybackStreamingQuality>(),
            sourceLength ?? _faker.Random.Long(1, 100_000_000),
            sourceLastModifiedUtc ?? _faker.Date.Past().ToUniversalTime());
    }

    /// <summary>
    /// Creates a list of <see cref="NormalizedTrackRequestDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="NormalizedTrackRequestDto"/> instances.</returns>
    public List<NormalizedTrackRequestDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
