#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Common.DTO.Playlists;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Common.DTO.Playlists;

/// <summary>
/// Fixture class for the <see cref="PlaylistEntryDisplayDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class PlaylistEntryDisplayDtoFixture
{
    private const int MAXIMUM_DURATION_SECONDS = 36_000;

    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="PlaylistEntryDisplayDto"/>.
    /// </summary>
    /// <param name="trackId">Optional. The Id of the track.</param>
    /// <param name="trackTitle">Optional. The title of the track.</param>
    /// <param name="durationInSeconds">Optional. The duration of the track, in seconds. Defaults to a bounded random duration.</param>
    /// <param name="libraryId">Optional. The Id of the media library that contains the track.</param>
    /// <param name="albumId">Optional. The Id of the album that contains the track.</param>
    /// <param name="albumTitle">Optional. The title of the album that contains the track.</param>
    /// <param name="artistId">Optional. The Id of the artist that produced the track.</param>
    /// <param name="artistName">Optional. The name of the artist that produced the track.</param>
    /// <param name="coverPath">Optional. The path of the cover art of the album.</param>
    /// <param name="includeCoverPath">Whether the cover path should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="PlaylistEntryDisplayDto"/>.</returns>
    public PlaylistEntryDisplayDto Create(
        Guid? trackId = null,
        string? trackTitle = null,
        int? durationInSeconds = null,
        Guid? libraryId = null,
        Guid? albumId = null,
        string? albumTitle = null,
        Guid? artistId = null,
        string? artistName = null,
        string? coverPath = null,
        bool includeCoverPath = true)
    {
        return new PlaylistEntryDisplayDto(
            trackId ?? _faker.Random.Guid(),
            trackTitle ?? _faker.Lorem.Sentence(3),
            durationInSeconds ?? _faker.Random.Int(0, MAXIMUM_DURATION_SECONDS),
            libraryId ?? _faker.Random.Guid(),
            albumId ?? _faker.Random.Guid(),
            albumTitle ?? _faker.Lorem.Sentence(3),
            artistId ?? _faker.Random.Guid(),
            artistName ?? _faker.Name.FullName(),
            includeCoverPath ? (coverPath ?? $"/covers/{_faker.Random.Guid()}.jpg") : null);
    }

    /// <summary>
    /// Creates a list of <see cref="PlaylistEntryDisplayDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="PlaylistEntryDisplayDto"/> instances.</returns>
    public List<PlaylistEntryDisplayDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
