#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Playback;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Playback;

/// <summary>
/// Fixture class for the <see cref="NormalizedTrackSegmentDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class NormalizedTrackSegmentDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="NormalizedTrackSegmentDto"/>.
    /// </summary>
    /// <param name="index">Optional. The zero based position of the segment in the delivered audio.</param>
    /// <param name="startSeconds">Optional. The time, in seconds from the start of the track, at which the segment starts.</param>
    /// <param name="durationSeconds">Optional. The duration of the segment, in seconds.</param>
    /// <param name="length">Optional. The length of the segment, in bytes.</param>
    /// <returns>The created <see cref="NormalizedTrackSegmentDto"/>.</returns>
    public NormalizedTrackSegmentDto Create(
        int? index = null,
        decimal? startSeconds = null,
        decimal? durationSeconds = null,
        long? length = null)
    {
        return new NormalizedTrackSegmentDto(
            index ?? _faker.Random.Int(0, 1000),
            startSeconds ?? _faker.Random.Decimal(0, 3600),
            durationSeconds ?? _faker.Random.Decimal(1, 30),
            length ?? _faker.Random.Long(1, 10_000_000));
    }

    /// <summary>
    /// Creates a list of <see cref="NormalizedTrackSegmentDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="NormalizedTrackSegmentDto"/> instances.</returns>
    public List<NormalizedTrackSegmentDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
