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
/// Fixture class for the <see cref="ShuffleTrackResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class ShuffleTrackResponseFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="ShuffleTrackResponse"/>.
    /// </summary>
    /// <param name="trackId">Optional. The Id of the track.</param>
    /// <param name="title">Optional. The title of the track.</param>
    /// <param name="durationInSeconds">Optional. The duration of the track in seconds.</param>
    /// <param name="albumId">Optional. The Id of the album the track belongs to.</param>
    /// <param name="albumTitle">Optional. The title of the album the track belongs to.</param>
    /// <param name="artistId">Optional. The Id of the artist the album belongs to.</param>
    /// <param name="coverPath">Optional. The path of the cover of the album.</param>
    /// <param name="includeCoverPath">Whether the cover path should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="ShuffleTrackResponse"/>.</returns>
    public ShuffleTrackResponse Create(
        Guid? trackId = null,
        string? title = null,
        int? durationInSeconds = null,
        Guid? albumId = null,
        string? albumTitle = null,
        Guid? artistId = null,
        string? coverPath = null,
        bool includeCoverPath = true)
    {
        return new ShuffleTrackResponse(
            trackId ?? _faker.Random.Guid(),
            title ?? _faker.Lorem.Sentence(),
            durationInSeconds ?? _faker.Random.Int(1, 7200),
            albumId ?? _faker.Random.Guid(),
            albumTitle ?? _faker.Lorem.Sentence(),
            artistId ?? _faker.Random.Guid(),
            includeCoverPath ? (coverPath ?? _faker.System.FileName("jpg")) : null);
    }

    /// <summary>
    /// Creates a list of <see cref="ShuffleTrackResponse"/> records with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="ShuffleTrackResponse"/> records.</returns>
    public List<ShuffleTrackResponse> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
