#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.DataAccess.Entities.Playlists;
using Nemera.Domain.Common.Primitives;
using Nemera.Domain.Core.BoundedContexts.PlaylistBoundedContext.ExternalIdentifiers.UserManagementBoundedContext.UserAggregate;
using Nemera.Domain.Core.BoundedContexts.PlaylistBoundedContext.PlaylistAggregate;
using Nemera.Domain.Core.BoundedContexts.PlaylistBoundedContext.PlaylistAggregate.Entities;
using Nemera.Domain.Core.BoundedContexts.PlaylistBoundedContext.PlaylistAggregate.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
#endregion

namespace Nemera.Application.Common.Mapping.Playlists;

/// <summary>
/// Extension methods for converting <see cref="PlaylistEntity"/>.
/// </summary>
public static class PlaylistEntityMapping
{
    /// <summary>
    /// Converts <paramref name="repositoryEntity"/> to <see cref="Playlist"/>.
    /// </summary>
    /// <param name="repositoryEntity">The repository entity to be converted.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a successfully converted <see cref="Playlist"/>, or an error message.
    /// </returns>
    public static Result<Playlist> ToDomainEntity(this PlaylistEntity repositoryEntity)
    {
        List<PlaylistEntry> entries = [];
        // The entries are ordered by their position, because the order of a navigation collection is not guaranteed by the storage medium.
        foreach (PlaylistEntryEntity entryEntity in repositoryEntity.Entries.OrderBy(entry => entry.Position))
        {
            Result<PlaylistEntry> convertEntryResult = entryEntity.ToDomainEntity();
            if (convertEntryResult.IsFailure)
                return convertEntryResult.Errors;
            entries.Add(convertEntryResult.Value);
        }

        return Playlist.Create(
            PlaylistId.Create(repositoryEntity.Id),
            UserId.Create(repositoryEntity.UserId),
            repositoryEntity.Name,
            Optional<string>.FromNullable(repositoryEntity.Description),
            repositoryEntity.Type,
            entries,
            repositoryEntity.CreatedOnUtc,
            Optional<DateTime>.FromNullable(repositoryEntity.UpdatedOnUtc));
    }
}
