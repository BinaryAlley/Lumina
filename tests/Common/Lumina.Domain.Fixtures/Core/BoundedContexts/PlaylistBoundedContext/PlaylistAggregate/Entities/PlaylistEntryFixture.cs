#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Domain.Core.BoundedContexts.PlaylistBoundedContext.ExternalIdentifiers.MediaLibraryBoundedContext.MediaItemAggregate;
using Lumina.Domain.Core.BoundedContexts.PlaylistBoundedContext.PlaylistAggregate.Entities;
using Lumina.Domain.Core.BoundedContexts.PlaylistBoundedContext.PlaylistAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.PlaylistBoundedContext.ExternalIdentifiers.MediaLibraryBoundedContext.MediaItemAggregate;
using Lumina.Domain.Fixtures.Core.BoundedContexts.PlaylistBoundedContext.PlaylistAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.Playlists;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.Fixtures.Core.BoundedContexts.PlaylistBoundedContext.PlaylistAggregate.Entities;

/// <summary>
/// Fixture class for the <see cref="PlaylistEntry"/> entity.
/// </summary>
[ExcludeFromCodeCoverage]
public class PlaylistEntryFixture
{
    private const long MAXIMUM_RANDOM_POSITION = 1_000_000_000L;

    private readonly Faker _faker = new();
    private readonly PlaylistEntryIdFixture _playlistEntryIdFixture = new();
    private readonly MediaItemIdFixture _mediaItemIdFixture = new();

    /// <summary>
    /// Creates a random valid <see cref="PlaylistEntry"/>.
    /// </summary>
    /// <param name="id">Optional. The Id of the entry.</param>
    /// <param name="mediaItemId">Optional. The Id of the media item referenced by the entry.</param>
    /// <param name="mediaItemType">Optional. The kind of media item referenced by the entry. Defaults to a music track, which is the kind a music playlist accepts.</param>
    /// <param name="position">Optional. The sparse order key of the entry. Defaults to a bounded random key.</param>
    /// <returns>The created <see cref="PlaylistEntry"/>.</returns>
    public PlaylistEntry Create(
        PlaylistEntryId? id = null,
        MediaItemId? mediaItemId = null,
        PlaylistMediaItemType? mediaItemType = null,
        long? position = null)
    {
        return PlaylistEntry.Create(
            id ?? _playlistEntryIdFixture.Create(),
            mediaItemId ?? _mediaItemIdFixture.Create(),
            mediaItemType ?? PlaylistMediaItemType.MusicTrack,
            position ?? _faker.Random.Long(0, MAXIMUM_RANDOM_POSITION)).Value;
    }

    /// <summary>
    /// Creates multiple <see cref="PlaylistEntry"/> instances with randomized test data and distinct positions.
    /// </summary>
    /// <remarks>
    /// The positions are spaced apart and unique, so the created entries can be placed in the same playlist without violating the
    /// uniqueness invariant of the sparse order keys.
    /// </remarks>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="PlaylistEntry"/> instances.</returns>
    public List<PlaylistEntry> CreateMany(int count = 3)
    {
        long firstPosition = _faker.Random.Long(0, MAXIMUM_RANDOM_POSITION);
        return [.. Enumerable.Range(0, count).Select(index => Create(position: firstPosition + index))];
    }
}
