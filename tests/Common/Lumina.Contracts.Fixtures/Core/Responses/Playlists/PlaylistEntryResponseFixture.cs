#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.Responses.Playlists;
using Lumina.Domain.SharedKernel.Common.Enums.Playlists;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Responses.Playlists;

/// <summary>
/// Fixture class for the <see cref="PlaylistEntryResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class PlaylistEntryResponseFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="PlaylistEntryResponse"/>.
    /// </summary>
    /// <param name="id">Optional. The Id of the entry.</param>
    /// <param name="mediaItemId">Optional. The Id of the media item referenced by the entry.</param>
    /// <param name="mediaItemType">Optional. The kind of media item referenced by the entry.</param>
    /// <param name="position">Optional. The zero based position of the entry in the playlist.</param>
    /// <param name="libraryId">Optional. The Id of the media library that contains the media item.</param>
    /// <param name="collectionId">Optional. The Id of the collection that contains the media item.</param>
    /// <param name="creatorId">Optional. The Id of the creator of the media item.</param>
    /// <param name="mediaItemTitle">Optional. The title of the media item.</param>
    /// <param name="collectionTitle">Optional. The title of the collection that contains the media item.</param>
    /// <param name="creatorName">Optional. The name of the creator of the media item.</param>
    /// <param name="durationInSeconds">Optional. The duration of the media item in seconds.</param>
    /// <param name="coverPath">Optional. The path of the cover art of the collection.</param>
    /// <param name="includeCoverPath">Whether the cover path should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="PlaylistEntryResponse"/>.</returns>
    public PlaylistEntryResponse Create(
        Guid? id = null,
        Guid? mediaItemId = null,
        PlaylistMediaItemType? mediaItemType = null,
        int? position = null,
        Guid? libraryId = null,
        Guid? collectionId = null,
        Guid? creatorId = null,
        string? mediaItemTitle = null,
        string? collectionTitle = null,
        string? creatorName = null,
        int? durationInSeconds = null,
        string? coverPath = null,
        bool includeCoverPath = true)
    {
        return new PlaylistEntryResponse(
            id ?? _faker.Random.Guid(),
            mediaItemId ?? _faker.Random.Guid(),
            mediaItemType ?? _faker.PickRandom<PlaylistMediaItemType>(),
            position ?? _faker.Random.Int(0, 1_000),
            libraryId ?? _faker.Random.Guid(),
            collectionId ?? _faker.Random.Guid(),
            creatorId ?? _faker.Random.Guid(),
            mediaItemTitle ?? _faker.Lorem.Sentence(3),
            collectionTitle ?? _faker.Lorem.Sentence(2),
            creatorName ?? _faker.Name.FullName(),
            durationInSeconds ?? _faker.Random.Int(1, 3_600),
            includeCoverPath ? (coverPath ?? $"/covers/{_faker.Random.Guid()}.jpg") : null);
    }

    /// <summary>
    /// Creates a list of <see cref="PlaylistEntryResponse"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="PlaylistEntryResponse"/> instances.</returns>
    public List<PlaylistEntryResponse> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
