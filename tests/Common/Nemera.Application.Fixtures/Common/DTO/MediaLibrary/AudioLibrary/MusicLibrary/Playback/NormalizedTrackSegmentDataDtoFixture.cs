#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Playback;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Playback;

/// <summary>
/// Fixture class for the <see cref="NormalizedTrackSegmentDataDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class NormalizedTrackSegmentDataDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="NormalizedTrackSegmentDataDto"/>.
    /// </summary>
    /// <param name="content">Optional. The read stream over the segment. When it is not supplied, a fresh empty <see cref="MemoryStream"/> is created.</param>
    /// <param name="length">Optional. The length of the segment, in bytes.</param>
    /// <param name="startSeconds">Optional. The exact start of the segment on the track timeline, in seconds.</param>
    /// <param name="container">Optional. The container of the delivered audio.</param>
    /// <param name="segmentCount">Optional. The number of segments the production holds.</param>
    /// <param name="durationInSeconds">Optional. The duration of the produced normalized audio, in seconds.</param>
    /// <returns>The created <see cref="NormalizedTrackSegmentDataDto"/>.</returns>
    public NormalizedTrackSegmentDataDto Create(
        Stream? content = null,
        long? length = null,
        decimal? startSeconds = null,
        string? container = null,
        int? segmentCount = null,
        int? durationInSeconds = null)
    {
        return new NormalizedTrackSegmentDataDto(
            content ?? new MemoryStream(),
            length ?? _faker.Random.Long(1, 10_000_000),
            startSeconds ?? _faker.Random.Decimal(0, 3600),
            container ?? _faker.PickRandom("MP3", "AAC", "FLAC", "WAV", "OGG"),
            segmentCount ?? _faker.Random.Int(1, 1000),
            durationInSeconds ?? _faker.Random.Int(1, 3600));
    }

    /// <summary>
    /// Creates a list of <see cref="NormalizedTrackSegmentDataDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="NormalizedTrackSegmentDataDto"/> instances.</returns>
    public List<NormalizedTrackSegmentDataDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
