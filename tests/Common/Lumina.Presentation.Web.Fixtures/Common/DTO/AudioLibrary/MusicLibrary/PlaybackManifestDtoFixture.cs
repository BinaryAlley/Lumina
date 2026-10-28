#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.DTO.AudioLibrary.MusicLibrary;
using Lumina.Presentation.Web.Common.Enums.AudioLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.DTO.AudioLibrary.MusicLibrary;

/// <summary>
/// Fixture class for the <see cref="PlaybackManifestDto"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class PlaybackManifestDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a new <see cref="PlaybackManifestDto"/> instance with randomized test data.
    /// </summary>
    /// <param name="trackId">Optional. The Id of the track the manifest describes.</param>
    /// <param name="durationInSeconds">Optional. The duration of the audio of the track, in seconds.</param>
    /// <param name="sourceContainer">Optional. The container of the stored audio file of the track.</param>
    /// <param name="sourceCodec">Optional. The codec of the stored audio of the track.</param>
    /// <param name="sampleRate">Optional. The sample rate of the stored audio, in Hz.</param>
    /// <param name="channels">Optional. The number of audio channels of the track.</param>
    /// <param name="bitDepth">Optional. The bit depth of the stored audio of the track.</param>
    /// <param name="replayGainTrackGain">Optional. The ReplayGain track gain of the track, in decibels.</param>
    /// <param name="replayGainTrackPeak">Optional. The ReplayGain track peak of the track.</param>
    /// <param name="replayGainAlbumGain">Optional. The ReplayGain album gain of the track, in decibels.</param>
    /// <param name="replayGainAlbumPeak">Optional. The ReplayGain album peak of the track.</param>
    /// <param name="isDirectPlay">Whether the stored audio is delivered as it is.</param>
    /// <param name="deliveryContainer">Optional. The container of the delivered audio.</param>
    /// <param name="deliveryCodec">Optional. The codec of the delivered audio.</param>
    /// <param name="deliveryShape">Optional. The shape the delivered audio is addressed by.</param>
    /// <param name="segmentDurationSeconds">Optional. The duration of each segment of the delivered audio, in seconds.</param>
    /// <param name="isSeekable">Whether the delivered audio can be seeked by the player.</param>
    /// <param name="streamPath">Optional. The relative path of the track stream.</param>
    /// <param name="deliveryId">Optional. The opaque handle of the normalized delivery.</param>
    /// <param name="segmentStreamPathTemplate">Optional. The relative path template of the segment route.</param>
    /// <param name="includeBitDepth">Whether the bit depth should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeReplayGain">Whether the ReplayGain values should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeDeliveryContainer">Whether the delivery container should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeDeliveryCodec">Whether the delivery codec should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeDeliveryShape">Whether the delivery shape should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeSegmentDurationSeconds">Whether the segment duration should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeDeliveryId">Whether the delivery handle should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeSegmentStreamPathTemplate">Whether the segment path template should be included, or forced to <see langword="null"/>.</param>
    /// <returns>A configured <see cref="PlaybackManifestDto"/> instance.</returns>
    public PlaybackManifestDto Create(
        Guid? trackId = null,
        int? durationInSeconds = null,
        string? sourceContainer = null,
        string? sourceCodec = null,
        int? sampleRate = null,
        int? channels = null,
        int? bitDepth = null,
        decimal? replayGainTrackGain = null,
        decimal? replayGainTrackPeak = null,
        decimal? replayGainAlbumGain = null,
        decimal? replayGainAlbumPeak = null,
        bool isDirectPlay = false,
        string? deliveryContainer = null,
        string? deliveryCodec = null,
        PlaybackDeliveryShape? deliveryShape = null,
        int? segmentDurationSeconds = null,
        bool isSeekable = true,
        string? streamPath = null,
        string? deliveryId = null,
        string? segmentStreamPathTemplate = null,
        bool includeBitDepth = true,
        bool includeReplayGain = true,
        bool includeDeliveryContainer = true,
        bool includeDeliveryCodec = true,
        bool includeDeliveryShape = true,
        bool includeSegmentDurationSeconds = true,
        bool includeDeliveryId = true,
        bool includeSegmentStreamPathTemplate = true)
    {
        Guid resolvedTrackId = trackId ?? _faker.Random.Guid();
        return new PlaybackManifestDto
        {
            TrackId = resolvedTrackId,
            DurationInSeconds = durationInSeconds ?? _faker.Random.Int(1, 3600),
            SourceContainer = sourceContainer ?? _faker.PickRandom("FLAC", "MP3", "OGG", "MP4", "AAC", "WAV"),
            SourceCodec = sourceCodec ?? _faker.PickRandom("FLAC", "MP3", "AAC", "ALAC", "OPUS", "PCM"),
            SampleRate = sampleRate ?? _faker.PickRandom(44100, 48000, 96000),
            Channels = channels ?? _faker.PickRandom(1, 2, 6),
            BitDepth = includeBitDepth ? (bitDepth ?? _faker.Random.Int(8, 32)) : null,
            ReplayGainTrackGain = includeReplayGain ? (replayGainTrackGain ?? _faker.Random.Decimal(-20m, 0m)) : null,
            ReplayGainTrackPeak = includeReplayGain ? (replayGainTrackPeak ?? _faker.Random.Decimal(0m, 1.5m)) : null,
            ReplayGainAlbumGain = includeReplayGain ? (replayGainAlbumGain ?? _faker.Random.Decimal(-20m, 0m)) : null,
            ReplayGainAlbumPeak = includeReplayGain ? (replayGainAlbumPeak ?? _faker.Random.Decimal(0m, 1.5m)) : null,
            IsDirectPlay = isDirectPlay,
            DeliveryContainer = includeDeliveryContainer ? (deliveryContainer ?? "MP3") : null,
            DeliveryCodec = includeDeliveryCodec ? (deliveryCodec ?? "MP3") : null,
            DeliveryShape = includeDeliveryShape ? (deliveryShape ?? PlaybackDeliveryShape.TimeAddressedSegments) : null,
            SegmentDurationSeconds = includeSegmentDurationSeconds ? (segmentDurationSeconds ?? 6) : null,
            IsSeekable = isSeekable,
            StreamPath = streamPath ?? $"/libraries/{Guid.NewGuid():N}/artists/{Guid.NewGuid():N}/albums/{Guid.NewGuid():N}/tracks/{resolvedTrackId:N}/stream",
            DeliveryId = includeDeliveryId ? (deliveryId ?? Guid.NewGuid().ToString("N")) : null,
            SegmentStreamPathTemplate = includeSegmentStreamPathTemplate
                ? (segmentStreamPathTemplate ?? $"/libraries/{Guid.NewGuid():N}/artists/{Guid.NewGuid():N}/albums/{Guid.NewGuid():N}/tracks/{resolvedTrackId:N}/stream/deliveries/{{deliveryId}}/segments/{{segmentIndex}}")
                : null
        };
    }

    /// <summary>
    /// Creates multiple <see cref="PlaybackManifestDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="PlaybackManifestDto"/> instances.</returns>
    public List<PlaybackManifestDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
