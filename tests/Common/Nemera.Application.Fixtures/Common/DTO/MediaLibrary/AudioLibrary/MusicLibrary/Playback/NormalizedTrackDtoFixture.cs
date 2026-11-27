#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Playback;
using Lumina.Domain.SharedKernel.Common.Enums.Playback;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Playback;

/// <summary>
/// Fixture class for the <see cref="NormalizedTrackDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class NormalizedTrackDtoFixture
{
    private readonly Faker _faker = new();
    private readonly NormalizedTrackSegmentDtoFixture _normalizedTrackSegmentDtoFixture = new();

    /// <summary>
    /// Creates a random valid <see cref="NormalizedTrackDto"/>.
    /// </summary>
    /// <param name="trackId">Optional. The Id of the track whose audio the cached entry holds.</param>
    /// <param name="cacheId">Optional. The opaque identifier of the cached entry that holds the normalized audio.</param>
    /// <param name="mode">Optional. The way the audio is delivered.</param>
    /// <param name="container">Optional. The container of the delivered audio.</param>
    /// <param name="codec">Optional. The codec of the delivered audio.</param>
    /// <param name="segmentDurationSeconds">Optional. The duration of each segment of the delivered audio, in seconds.</param>
    /// <param name="durationInSeconds">Optional. The duration of the produced and cached audio, in seconds.</param>
    /// <param name="segments">Optional. The segments that make up the delivered audio, in playback order.</param>
    /// <param name="includeSegments">Whether the segments should be included, or forced to an empty collection.</param>
    /// <returns>The created <see cref="NormalizedTrackDto"/>.</returns>
    public NormalizedTrackDto Create(
        Guid? trackId = null,
        string? cacheId = null,
        PlaybackDeliveryMode? mode = null,
        string? container = null,
        string? codec = null,
        int? segmentDurationSeconds = null,
        int? durationInSeconds = null,
        IReadOnlyList<NormalizedTrackSegmentDto>? segments = null,
        bool includeSegments = true)
    {
        int resolvedSegmentDurationSeconds = segmentDurationSeconds ?? _faker.Random.Int(1, 30);
        List<NormalizedTrackSegmentDto> generatedSegments = [];
        int generatedSegmentCount = _faker.Random.Int(1, 6);
        for (int index = 0; index < generatedSegmentCount; index++)
            generatedSegments.Add(_normalizedTrackSegmentDtoFixture.Create(
                index: index,
                startSeconds: index * resolvedSegmentDurationSeconds,
                durationSeconds: resolvedSegmentDurationSeconds));

        return new NormalizedTrackDto(
            trackId ?? _faker.Random.Guid(),
            cacheId ?? _faker.Random.Guid().ToString("N") + _faker.Random.Guid().ToString("N"),
            mode ?? _faker.PickRandom(PlaybackDeliveryMode.Remux, PlaybackDeliveryMode.Transcode),
            container ?? _faker.PickRandom("MP3", "AAC", "FLAC", "WAV", "OGG"),
            codec ?? _faker.PickRandom("MP3", "AAC", "FLAC", "PCM", "OPUS"),
            resolvedSegmentDurationSeconds,
            durationInSeconds ?? _faker.Random.Int(1, 3600),
            includeSegments ? (segments ?? generatedSegments) : []);
    }

    /// <summary>
    /// Creates a list of <see cref="NormalizedTrackDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="NormalizedTrackDto"/> instances.</returns>
    public List<NormalizedTrackDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
