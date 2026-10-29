#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.DTO.Playlists;
using Lumina.Presentation.Web.Common.Enums.Playlists;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.DTO.Playlists;

/// <summary>
/// Fixture class for the <see cref="PlaylistEntryDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class PlaylistEntryDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a new <see cref="PlaylistEntryDto"/> instance with randomized test data.
    /// </summary>
    /// <param name="id">Optional Id of the entry.</param>
    /// <param name="mediaItemId">Optional Id of the media item referenced by the entry.</param>
    /// <param name="mediaItemType">Optional kind of media item referenced by the entry.</param>
    /// <param name="position">Optional zero based position of the entry in the playlist.</param>
    /// <param name="libraryId">Optional Id of the media library that contains the media item.</param>
    /// <param name="collectionId">Optional Id of the collection that contains the media item.</param>
    /// <param name="creatorId">Optional Id of the creator of the media item.</param>
    /// <param name="mediaItemTitle">Optional title of the media item.</param>
    /// <param name="collectionTitle">Optional title of the collection that contains the media item.</param>
    /// <param name="creatorName">Optional name of the creator of the media item.</param>
    /// <param name="durationInSeconds">Optional duration of the media item in seconds.</param>
    /// <param name="coverPath">Optional path of the cover art of the collection.</param>
    /// <param name="includeCoverPath">Whether the cover path should be included, or forced to <see langword="null"/>.</param>
    /// <returns>A configured <see cref="PlaylistEntryDto"/> instance.</returns>
    public PlaylistEntryDto Create(
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
        return new PlaylistEntryDto(
            Id: id ?? _faker.Random.Guid(),
            MediaItemId: mediaItemId ?? _faker.Random.Guid(),
            MediaItemType: mediaItemType ?? _faker.PickRandom(PlaylistMediaItemType.MusicTrack, PlaylistMediaItemType.VideoFile),
            Position: position ?? _faker.Random.Int(0, 1000),
            LibraryId: libraryId ?? _faker.Random.Guid(),
            CollectionId: collectionId ?? _faker.Random.Guid(),
            CreatorId: creatorId ?? _faker.Random.Guid(),
            MediaItemTitle: mediaItemTitle ?? _faker.Lorem.Sentence(3),
            CollectionTitle: collectionTitle ?? _faker.Lorem.Sentence(2),
            CreatorName: creatorName ?? _faker.Name.FullName(),
            DurationInSeconds: durationInSeconds ?? _faker.Random.Int(0, 36000),
            CoverPath: includeCoverPath ? (coverPath ?? $"/artwork/{_faker.Random.Guid()}/cover.jpg") : null
        );
    }

    /// <summary>
    /// Creates multiple <see cref="PlaylistEntryDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="PlaylistEntryDto"/> instances.</returns>
    public List<PlaylistEntryDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
