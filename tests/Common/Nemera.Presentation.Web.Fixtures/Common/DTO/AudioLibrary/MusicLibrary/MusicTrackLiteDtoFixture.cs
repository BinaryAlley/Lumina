#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.DTO.AudioLibrary.MusicLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.DTO.AudioLibrary.MusicLibrary;

/// <summary>
/// Fixture class for the <see cref="MusicTrackLiteDto"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MusicTrackLiteDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a new <see cref="MusicTrackLiteDto"/> instance with randomized test data.
    /// </summary>
    /// <param name="id">Optional. The Id of the track.</param>
    /// <param name="title">Optional. The title of the track.</param>
    /// <param name="trackNumber">Optional. The number of the track on its disc.</param>
    /// <param name="discNumber">Optional. The number of the disc the track belongs to.</param>
    /// <param name="durationInSeconds">Optional. The duration of the track in seconds.</param>
    /// <param name="includeDiscNumber">Whether the disc number should be included, or forced to <see langword="null"/>.</param>
    /// <returns>A configured <see cref="MusicTrackLiteDto"/> instance.</returns>
    public MusicTrackLiteDto Create(
        Guid? id = null,
        string? title = null,
        int? trackNumber = null,
        int? discNumber = null,
        int? durationInSeconds = null,
        bool includeDiscNumber = true)
    {
        return new MusicTrackLiteDto
        {
            Id = id ?? _faker.Random.Guid(),
            Title = title ?? _faker.Lorem.Sentence(3),
            TrackNumber = trackNumber ?? _faker.Random.Number(1, 30),
            DiscNumber = includeDiscNumber ? (discNumber ?? _faker.Random.Number(1, 3)) : null,
            DurationInSeconds = durationInSeconds ?? _faker.Random.Number(30, 600)
        };
    }

    /// <summary>
    /// Creates multiple <see cref="MusicTrackLiteDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="MusicTrackLiteDto"/> instances.</returns>
    public List<MusicTrackLiteDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
