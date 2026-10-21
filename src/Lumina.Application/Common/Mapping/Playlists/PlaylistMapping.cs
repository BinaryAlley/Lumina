#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Playlists;
using Lumina.Domain.Core.BoundedContexts.PlaylistBoundedContext.PlaylistAggregate;
using System;
using System.Linq;
#endregion

namespace Lumina.Application.Common.Mapping.Playlists;

/// <summary>
/// Extension methods for converting <see cref="Playlist"/>.
/// </summary>
public static class PlaylistMapping
{
    /// <summary>
    /// Converts <paramref name="domainEntity"/> to <see cref="PlaylistEntity"/>.
    /// </summary>
    /// <param name="domainEntity">The domain entity to be converted.</param>
    /// <returns>The converted repository entity.</returns>
    public static PlaylistEntity ToRepositoryEntity(this Playlist domainEntity)
    {
        return new PlaylistEntity
        {
            Id = domainEntity.Id.Value,
            UserId = domainEntity.UserId.Value,
            Name = domainEntity.Name,
            Description = domainEntity.Description.HasValue ? domainEntity.Description.Value : null,
            Type = domainEntity.Type,
            Entries = [.. domainEntity.Entries
                .OrderBy(entry => entry.Position)
                .Select(entry => entry.ToRepositoryEntity(domainEntity.Id.Value))],
            // This is the correct count for an insert. The update path deliberately keeps the stored count, because details edits never load the entries and must not overwrite it.
            EntryCount = domainEntity.Entries.Count,
            CreatedOnUtc = domainEntity.CreatedOnUtc,
            CreatedBy = Guid.Empty,
            UpdatedOnUtc = domainEntity.UpdatedOnUtc.HasValue ? domainEntity.UpdatedOnUtc.Value : null,
            UpdatedBy = null
        };
    }
}
