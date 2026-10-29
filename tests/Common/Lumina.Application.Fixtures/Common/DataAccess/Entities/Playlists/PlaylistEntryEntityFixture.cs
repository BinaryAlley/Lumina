#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Common.DataAccess.Entities.Playlists;
using Lumina.Domain.SharedKernel.Common.Enums.Playlists;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.Fixtures.Common.DataAccess.Entities.Playlists;

/// <summary>
/// Fixture class for the <see cref="PlaylistEntryEntity"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class PlaylistEntryEntityFixture
{
    /// <summary>
    /// Creates a random valid <see cref="PlaylistEntryEntity"/>.
    /// </summary>
    /// <param name="id">Optional. The Id of the entry.</param>
    /// <param name="playlistId">Optional. The Id of the playlist the entry belongs to.</param>
    /// <param name="mediaItemId">Optional. The Id of the referenced media item.</param>
    /// <param name="mediaItemType">Optional. The kind of the referenced media item.</param>
    /// <param name="position">Optional. The sparse order key of the entry.</param>
    /// <param name="createdOnUtc">Optional. The time and date when the entry was created.</param>
    /// <param name="createdBy">Optional. The Id of the user that created the entry.</param>
    /// <param name="includeUpdatedOnUtc">Whether the update time should be populated.</param>
    /// <param name="updatedOnUtc">Optional. The time and date when the entry was updated.</param>
    /// <param name="updatedBy">Optional. The Id of the user that updated the entry.</param>
    /// <returns>The created playlist entry entity.</returns>
    public PlaylistEntryEntity Create(
        Guid? id = null,
        Guid? playlistId = null,
        Guid? mediaItemId = null,
        PlaylistMediaItemType? mediaItemType = null,
        long? position = null,
        DateTime? createdOnUtc = null,
        Guid? createdBy = null,
        bool includeUpdatedOnUtc = false,
        DateTime? updatedOnUtc = null,
        Guid? updatedBy = null)
    {
        return new Faker<PlaylistEntryEntity>()
            .CustomInstantiator(f => new PlaylistEntryEntity
            {
                Id = id ?? f.Random.Guid(),
                PlaylistId = playlistId ?? f.Random.Guid(),
                MediaItemId = mediaItemId ?? f.Random.Guid(),
                MediaItemType = mediaItemType ?? f.PickRandom<PlaylistMediaItemType>(),
                Position = position ?? f.Random.Long(0, 1_000_000_000L),
                CreatedOnUtc = createdOnUtc ?? f.Date.Past(),
                CreatedBy = createdBy ?? f.Random.Guid(),
                UpdatedOnUtc = includeUpdatedOnUtc ? (updatedOnUtc ?? f.Date.Recent()) : null,
                UpdatedBy = includeUpdatedOnUtc ? (updatedBy ?? f.Random.Guid()) : null
            })
            .Generate();
    }
}
