#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.DataAccess.Entities.Playlists;
using Nemera.Domain.Common.Primitives;
using Nemera.Domain.Core.BoundedContexts.PlaylistBoundedContext.ExternalIdentifiers.MediaLibraryBoundedContext.MediaItemAggregate;
using Nemera.Domain.Core.BoundedContexts.PlaylistBoundedContext.PlaylistAggregate.Entities;
using Nemera.Domain.Core.BoundedContexts.PlaylistBoundedContext.PlaylistAggregate.ValueObjects;
#endregion

namespace Nemera.Application.Common.Mapping.Playlists;

/// <summary>
/// Extension methods for converting <see cref="PlaylistEntryEntity"/>.
/// </summary>
public static class PlaylistEntryEntityMapping
{
    /// <summary>
    /// Converts <paramref name="repositoryEntity"/> to <see cref="PlaylistEntry"/>.
    /// </summary>
    /// <param name="repositoryEntity">The repository entity to be converted.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a successfully converted <see cref="PlaylistEntry"/>, or an error message.
    /// </returns>
    public static Result<PlaylistEntry> ToDomainEntity(this PlaylistEntryEntity repositoryEntity)
    {
        return PlaylistEntry.Create(
            PlaylistEntryId.Create(repositoryEntity.Id),
            MediaItemId.Create(repositoryEntity.MediaItemId),
            repositoryEntity.MediaItemType,
            repositoryEntity.Position);
    }
}
