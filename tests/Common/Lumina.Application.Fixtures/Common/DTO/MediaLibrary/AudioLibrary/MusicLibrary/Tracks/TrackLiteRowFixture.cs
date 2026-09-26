#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;

/// <summary>
/// Fixture class for the <see cref="TrackLiteRow"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class TrackLiteRowFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="TrackLiteRow"/>.
    /// </summary>
    /// <param name="id">Optional. The Id of the track.</param>
    /// <param name="title">Optional. The title of the track.</param>
    /// <param name="trackNumber">Optional. The number of the track on its disc.</param>
    /// <param name="discNumber">Optional. The number of the disc the track belongs to.</param>
    /// <param name="includeDiscNumber">Whether the disc number should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="TrackLiteRow"/>.</returns>
    public TrackLiteRow Create(
        Guid? id = null,
        string? title = null,
        int? trackNumber = null,
        int? discNumber = null,
        bool includeDiscNumber = true)
    {
        return new TrackLiteRow
        {
            Id = id ?? _faker.Random.Guid(),
            Title = title ?? _faker.Lorem.Sentence(),
            TrackNumber = trackNumber ?? _faker.Random.Int(1, 30),
            DiscNumber = includeDiscNumber ? (discNumber ?? _faker.Random.Int(1, 3)) : null
        };
    }

    /// <summary>
    /// Creates a list of <see cref="TrackLiteRow"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="TrackLiteRow"/> instances.</returns>
    public List<TrackLiteRow> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
