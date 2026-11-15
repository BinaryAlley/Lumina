#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Albums;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Albums;

/// <summary>
/// Fixture class for the <see cref="DeletedAlbumResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class DeletedAlbumResponseFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="DeletedAlbumResponse"/>.
    /// </summary>
    /// <param name="id">Optional. The Id of the deleted album.</param>
    /// <param name="libraryId">Optional. The Id of the media library the deleted album belongs to.</param>
    /// <param name="artistId">Optional. The Id of the artist the deleted album belongs to.</param>
    /// <param name="artistName">Optional. The name of the artist the deleted album belongs to.</param>
    /// <param name="title">Optional. The title of the deleted album.</param>
    /// <param name="deletedOnUtc">Optional. The date and time when the album was deleted.</param>
    /// <param name="coverPath">Optional. The path of the cover image of the deleted album.</param>
    /// <param name="includeCoverPath">Whether the cover path should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="DeletedAlbumResponse"/>.</returns>
    public DeletedAlbumResponse Create(
        Guid? id = null,
        Guid? libraryId = null,
        Guid? artistId = null,
        string? artistName = null,
        string? title = null,
        DateTime? deletedOnUtc = null,
        string? coverPath = null,
        bool includeCoverPath = true)
    {
        return new DeletedAlbumResponse(
            id ?? _faker.Random.Guid(),
            libraryId ?? _faker.Random.Guid(),
            artistId ?? _faker.Random.Guid(),
            artistName ?? _faker.Name.FullName(),
            title ?? _faker.Music.Genre(),
            deletedOnUtc ?? _faker.Date.Recent().ToUniversalTime(),
            includeCoverPath ? (coverPath ?? $"/covers/{_faker.Random.Guid()}.jpg") : null);
    }

    /// <summary>
    /// Creates a list of <see cref="DeletedAlbumResponse"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="DeletedAlbumResponse"/> instances.</returns>
    public List<DeletedAlbumResponse> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
