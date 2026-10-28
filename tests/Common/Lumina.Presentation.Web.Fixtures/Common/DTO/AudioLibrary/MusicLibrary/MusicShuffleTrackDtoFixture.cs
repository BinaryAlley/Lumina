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
/// Fixture class for the <see cref="MusicShuffleTrackDto"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MusicShuffleTrackDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a new <see cref="MusicShuffleTrackDto"/> instance with randomized test data.
    /// </summary>
    /// <param name="trackId">Optional. The Id of the track.</param>
    /// <param name="title">Optional. The title of the track.</param>
    /// <param name="durationInSeconds">Optional. The duration of the track in seconds.</param>
    /// <param name="albumId">Optional. The Id of the album the track belongs to.</param>
    /// <param name="albumTitle">Optional. The title of the album the track belongs to.</param>
    /// <param name="artistId">Optional. The Id of the artist the album belongs to.</param>
    /// <param name="coverPath">Optional. The path of the cover of the album.</param>
    /// <param name="includeCoverPath">Whether the cover path should be included, or forced to <see langword="null"/>.</param>
    /// <returns>A configured <see cref="MusicShuffleTrackDto"/> instance.</returns>
    public MusicShuffleTrackDto Create(
        Guid? trackId = null,
        string? title = null,
        int? durationInSeconds = null,
        Guid? albumId = null,
        string? albumTitle = null,
        Guid? artistId = null,
        string? coverPath = null,
        bool includeCoverPath = true)
    {
        return new MusicShuffleTrackDto
        {
            TrackId = trackId ?? _faker.Random.Guid(),
            Title = title ?? _faker.Lorem.Sentence(),
            DurationInSeconds = durationInSeconds ?? _faker.Random.Int(1, 7200),
            AlbumId = albumId ?? _faker.Random.Guid(),
            AlbumTitle = albumTitle ?? _faker.Lorem.Sentence(),
            ArtistId = artistId ?? _faker.Random.Guid(),
            CoverPath = includeCoverPath ? (coverPath ?? _faker.System.FileName("jpg")) : null
        };
    }

    /// <summary>
    /// Creates multiple <see cref="MusicShuffleTrackDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="MusicShuffleTrackDto"/> instances.</returns>
    public List<MusicShuffleTrackDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
