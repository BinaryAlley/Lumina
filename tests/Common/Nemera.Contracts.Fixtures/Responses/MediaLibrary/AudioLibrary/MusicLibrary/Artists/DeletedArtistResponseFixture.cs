#region ========================================================================= USING =====================================================================================
using Bogus;
using Nemera.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Artists;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Nemera.Contracts.Fixtures.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Artists;

/// <summary>
/// Fixture class for the <see cref="DeletedArtistResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class DeletedArtistResponseFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="DeletedArtistResponse"/>.
    /// </summary>
    /// <param name="id">Optional. The Id of the deleted artist.</param>
    /// <param name="libraryId">Optional. The Id of the media library the deleted artist belongs to.</param>
    /// <param name="name">Optional. The name of the deleted artist.</param>
    /// <param name="deletedOnUtc">Optional. The date and time when the artist was deleted.</param>
    /// <param name="coverPath">Optional. The path of the cover image of the deleted artist.</param>
    /// <param name="includeCoverPath">Whether the cover path should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="DeletedArtistResponse"/>.</returns>
    public DeletedArtistResponse Create(
        Guid? id = null,
        Guid? libraryId = null,
        string? name = null,
        DateTime? deletedOnUtc = null,
        string? coverPath = null,
        bool includeCoverPath = true)
    {
        return new DeletedArtistResponse(
            id ?? _faker.Random.Guid(),
            libraryId ?? _faker.Random.Guid(),
            name ?? _faker.Name.FullName(),
            deletedOnUtc ?? _faker.Date.Recent().ToUniversalTime(),
            includeCoverPath ? (coverPath ?? $"/covers/{_faker.Random.Guid()}.jpg") : null);
    }

    /// <summary>
    /// Creates a list of <see cref="DeletedArtistResponse"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="DeletedArtistResponse"/> instances.</returns>
    public List<DeletedArtistResponse> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
