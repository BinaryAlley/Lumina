#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Domain.SharedKernel.Common.Enums.Playback;
using Lumina.Infrastructure.Common.Models.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Playback;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Infrastructure.Fixtures.Common.Models.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Playback;

/// <summary>
/// Fixture class for the <see cref="NormalizedTrackIndexDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
internal class NormalizedTrackIndexDtoFixture
{
    private readonly Faker _faker = new();
    private readonly NormalizedTrackSegmentIndexDtoFixture _normalizedTrackSegmentIndexDtoFixture = new();

    /// <summary>
    /// Creates a random valid <see cref="NormalizedTrackIndexDto"/>.
    /// </summary>
    /// <param name="version">Optional version of the index layout.</param>
    /// <param name="trackId">Optional Id of the track the cached audio belongs to.</param>
    /// <param name="mode">Optional way the audio is delivered.</param>
    /// <param name="container">Optional container of the delivered audio.</param>
    /// <param name="codec">Optional codec of the delivered audio.</param>
    /// <param name="segmentDurationSeconds">Optional duration of each segment, in seconds.</param>
    /// <param name="durationInSeconds">Optional duration of the produced and cached audio, in seconds.</param>
    /// <param name="segments">
    /// Optional segments that make up the delivered audio, in playback order. A caller can supply an explicit list, including an empty one; when it is not
    /// supplied a random bounded list is generated.
    /// </param>
    /// <returns>The created <see cref="NormalizedTrackIndexDto"/>.</returns>
    public NormalizedTrackIndexDto Create(
        int? version = null,
        Guid? trackId = null,
        PlaybackDeliveryMode? mode = null,
        string? container = null,
        string? codec = null,
        int? segmentDurationSeconds = null,
        int? durationInSeconds = null,
        IReadOnlyList<NormalizedTrackSegmentIndexDto>? segments = null)
    {
        return new NormalizedTrackIndexDto(
            version ?? _faker.Random.Int(1, 10),
            trackId ?? _faker.Random.Guid(),
            mode ?? _faker.PickRandom(PlaybackDeliveryMode.DirectPlay, PlaybackDeliveryMode.Remux, PlaybackDeliveryMode.Transcode),
            container ?? _faker.PickRandom("MP4", "OGG", "WAV", "FLAC"),
            codec ?? _faker.PickRandom("AAC", "VORBIS", "PCM", "FLAC"),
            segmentDurationSeconds ?? _faker.Random.Int(1, 60),
            durationInSeconds ?? _faker.Random.Int(1, 100_000),
            segments ?? [.. _normalizedTrackSegmentIndexDtoFixture.CreateMany(_faker.Random.Int(1, 5))]
        );
    }

    /// <summary>
    /// Creates a list of <see cref="NormalizedTrackIndexDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="NormalizedTrackIndexDto"/> instances.</returns>
    public List<NormalizedTrackIndexDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
