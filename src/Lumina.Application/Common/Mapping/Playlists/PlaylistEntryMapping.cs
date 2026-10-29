#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Playlists;
using Lumina.Domain.Core.BoundedContexts.PlaylistBoundedContext.PlaylistAggregate.Entities;
using System;
#endregion

namespace Lumina.Application.Common.Mapping.Playlists;

/// <summary>
/// Extension methods for converting <see cref="PlaylistEntry"/>.
/// </summary>
public static class PlaylistEntryMapping
{
    /// <summary>
    /// Converts <paramref name="domainEntity"/> to <see cref="PlaylistEntryEntity"/>.
    /// </summary>
    /// <param name="domainEntity">The domain entity to be converted.</param>
    /// <param name="playlistId">The Id of the playlist that owns the entry.</param>
    /// <returns>The converted repository entity.</returns>
    public static PlaylistEntryEntity ToRepositoryEntity(this PlaylistEntry domainEntity, Guid playlistId)
    {
        return new PlaylistEntryEntity
        {
            Id = domainEntity.Id.Value,
            PlaylistId = playlistId,
            MediaItemId = domainEntity.MediaItemId.Value,
            MediaItemType = domainEntity.MediaItemType,
            Position = domainEntity.Position
        };
    }
}
