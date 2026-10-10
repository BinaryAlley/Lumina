#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;
using System.Collections.Generic;
using System.Linq;
#endregion

namespace Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Common;

/// <summary>
/// Extension methods for converting <see cref="MusicArtistAlias"/>.
/// </summary>
public static class MusicArtistAliasMapping
{
    /// <summary>
    /// Converts <paramref name="domainEntity"/> to <see cref="ArtistAliasEntity"/>.
    /// </summary>
    /// <param name="domainEntity">The domain entity to be converted.</param>
    /// <returns>The converted repository entity.</returns>
    public static ArtistAliasEntity ToRepositoryEntity(this MusicArtistAlias domainEntity)
    {
        return new ArtistAliasEntity(
            domainEntity.Name,
            domainEntity.SortName.HasValue ? domainEntity.SortName.Value : null,
            domainEntity.Type.HasValue ? domainEntity.Type.Value : null,
            domainEntity.Locale.HasValue ? domainEntity.Locale.Value : null,
            domainEntity.IsPrimary,
            domainEntity.BeginDate.HasValue ? domainEntity.BeginDate.Value : null,
            domainEntity.EndDate.HasValue ? domainEntity.EndDate.Value : null,
            domainEntity.IsEnded);
    }

    /// <summary>
    /// Converts <paramref name="domainEntities"/> to a collection of <see cref="ArtistAliasEntity"/>.
    /// </summary>
    /// <param name="domainEntities">The domain entities to be converted.</param>
    /// <returns>The converted repository entities.</returns>
    public static List<ArtistAliasEntity> ToRepositoryEntities(this IEnumerable<MusicArtistAlias> domainEntities)
    {
        return [.. domainEntities.Select(domainEntity => domainEntity.ToRepositoryEntity())];
    }
}
