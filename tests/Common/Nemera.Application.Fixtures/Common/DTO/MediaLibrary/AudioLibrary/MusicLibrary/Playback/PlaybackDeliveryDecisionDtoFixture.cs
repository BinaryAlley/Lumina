#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Playback;
using Lumina.Domain.SharedKernel.Common.Enums.Playback;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Playback;

/// <summary>
/// Fixture class for the <see cref="PlaybackDeliveryDecisionDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class PlaybackDeliveryDecisionDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="PlaybackDeliveryDecisionDto"/>.
    /// </summary>
    /// <param name="mode">Optional. The way the audio is delivered.</param>
    /// <param name="container">Optional. The container of the delivered audio.</param>
    /// <param name="codec">Optional. The codec of the delivered audio.</param>
    /// <param name="shape">Optional. The shape the delivered audio is addressed and seeked by.</param>
    /// <param name="segmentDurationSeconds">Optional. The duration of each segment of the delivered audio, in seconds.</param>
    /// <returns>The created <see cref="PlaybackDeliveryDecisionDto"/>.</returns>
    public PlaybackDeliveryDecisionDto Create(
        PlaybackDeliveryMode? mode = null,
        string? container = null,
        string? codec = null,
        PlaybackDeliveryShape? shape = null,
        int? segmentDurationSeconds = null)
    {
        PlaybackDeliveryShape resolvedShape = shape ?? _faker.PickRandom<PlaybackDeliveryShape>();
        return new PlaybackDeliveryDecisionDto(
            mode ?? _faker.PickRandom(PlaybackDeliveryMode.Remux, PlaybackDeliveryMode.Transcode),
            container ?? _faker.PickRandom("MP3", "AAC", "FLAC", "WAV", "OGG"),
            codec ?? _faker.PickRandom("MP3", "AAC", "FLAC", "PCM", "OPUS"),
            resolvedShape,
            segmentDurationSeconds ?? (resolvedShape == PlaybackDeliveryShape.TimeAddressedSegments ? _faker.Random.Int(1, 30) : 0));
    }

    /// <summary>
    /// Creates a list of <see cref="PlaybackDeliveryDecisionDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="PlaybackDeliveryDecisionDto"/> instances.</returns>
    public List<PlaybackDeliveryDecisionDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
