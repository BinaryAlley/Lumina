#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Infrastructure.Common.Models.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Playback;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Infrastructure.Fixtures.Common.Models.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Playback;

/// <summary>
/// Fixture class for the <see cref="NormalizedTrackSegmentIndexDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
internal class NormalizedTrackSegmentIndexDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="NormalizedTrackSegmentIndexDto"/>.
    /// </summary>
    /// <param name="index">Optional zero based position of the segment in the delivered audio.</param>
    /// <param name="startSeconds">Optional time, in seconds from the start of the track, at which the segment starts.</param>
    /// <param name="durationSeconds">Optional duration of the segment, in seconds.</param>
    /// <param name="fileName">Optional file name of the segment inside the cache entry.</param>
    /// <param name="length">Optional length of the segment, in bytes.</param>
    /// <returns>The created <see cref="NormalizedTrackSegmentIndexDto"/>.</returns>
    public NormalizedTrackSegmentIndexDto Create(
        int? index = null,
        decimal? startSeconds = null,
        decimal? durationSeconds = null,
        string? fileName = null,
        long? length = null)
    {
        return new NormalizedTrackSegmentIndexDto(
            index ?? _faker.Random.Int(0, 10_000),
            startSeconds ?? _faker.Random.Decimal(0m, 100_000m),
            durationSeconds ?? _faker.Random.Decimal(1m, 600m),
            fileName ?? $"segment{_faker.Random.Int(0, 10_000):D6}{_faker.PickRandom(".m4s", ".mp4", ".ts")}",
            length ?? _faker.Random.Long(1, 100_000_000)
        );
    }

    /// <summary>
    /// Creates a list of <see cref="NormalizedTrackSegmentIndexDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="NormalizedTrackSegmentIndexDto"/> instances.</returns>
    public List<NormalizedTrackSegmentIndexDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(segmentIndex => Create(index: segmentIndex))];
    }
}
