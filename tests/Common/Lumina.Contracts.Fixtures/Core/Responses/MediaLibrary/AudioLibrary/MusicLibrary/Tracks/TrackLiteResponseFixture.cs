#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;

/// <summary>
/// Fixture class for the <see cref="TrackLiteResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class TrackLiteResponseFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="TrackLiteResponse"/>.
    /// </summary>
    /// <param name="id">Optional. The Id of the track.</param>
    /// <param name="title">Optional. The title of the track.</param>
    /// <param name="trackNumber">Optional. The number of the track on its disc.</param>
    /// <param name="discNumber">Optional. The number of the disc the track belongs to.</param>
    /// <param name="includeDiscNumber">Whether the disc number should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="TrackLiteResponse"/>.</returns>
    public TrackLiteResponse Create(
        Guid? id = null,
        string? title = null,
        int? trackNumber = null,
        int? discNumber = null,
        bool includeDiscNumber = true)
    {
        return new TrackLiteResponse(
            id ?? _faker.Random.Guid(),
            title ?? _faker.Lorem.Sentence(),
            trackNumber ?? _faker.Random.Int(1, 30),
            includeDiscNumber ? (discNumber ?? _faker.Random.Int(1, 3)) : null);
    }

    /// <summary>
    /// Creates a list of <see cref="TrackLiteResponse"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<TrackLiteResponse> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
