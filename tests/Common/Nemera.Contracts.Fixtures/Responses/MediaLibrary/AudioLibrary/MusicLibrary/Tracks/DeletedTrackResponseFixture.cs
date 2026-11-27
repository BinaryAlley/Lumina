#region ========================================================================= USING =====================================================================================
using Bogus;
using Nemera.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Nemera.Contracts.Fixtures.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;

/// <summary>
/// Fixture class for the <see cref="DeletedTrackResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class DeletedTrackResponseFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="DeletedTrackResponse"/>.
    /// </summary>
    /// <param name="id">Optional. The Id of the deleted track.</param>
    /// <param name="libraryId">Optional. The Id of the media library the deleted track belongs to.</param>
    /// <param name="artistId">Optional. The Id of the artist the deleted track belongs to.</param>
    /// <param name="artistName">Optional. The name of the artist the deleted track belongs to.</param>
    /// <param name="albumId">Optional. The Id of the album the deleted track belongs to.</param>
    /// <param name="albumTitle">Optional. The title of the album the deleted track belongs to.</param>
    /// <param name="title">Optional. The title of the deleted track.</param>
    /// <param name="path">Optional. The file system path of the deleted track.</param>
    /// <param name="deletedOnUtc">Optional. The date and time when the track was deleted.</param>
    /// <returns>The created <see cref="DeletedTrackResponse"/>.</returns>
    public DeletedTrackResponse Create(
        Guid? id = null,
        Guid? libraryId = null,
        Guid? artistId = null,
        string? artistName = null,
        Guid? albumId = null,
        string? albumTitle = null,
        string? title = null,
        string? path = null,
        DateTime? deletedOnUtc = null)
    {
        return new DeletedTrackResponse(
            id ?? _faker.Random.Guid(),
            libraryId ?? _faker.Random.Guid(),
            artistId ?? _faker.Random.Guid(),
            artistName ?? _faker.Name.FullName(),
            albumId ?? _faker.Random.Guid(),
            albumTitle ?? _faker.Music.Genre(),
            title ?? _faker.Music.Genre(),
            path ?? $"/music/tracks/{_faker.Random.Guid()}.flac",
            deletedOnUtc ?? _faker.Date.Recent().ToUniversalTime());
    }

    /// <summary>
    /// Creates a list of <see cref="DeletedTrackResponse"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="DeletedTrackResponse"/> instances.</returns>
    public List<DeletedTrackResponse> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
