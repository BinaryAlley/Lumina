#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.Common.ValueObjects;
using System.Collections.Generic;
using System.Linq;
#endregion

namespace Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Common;

/// <summary>
/// Extension methods for converting <see cref="AudioRating"/>.
/// </summary>
public static class AudioRatingMapping
{
    /// <summary>
    /// Converts <paramref name="domainEntity"/> to <see cref="AudioRatingEntity"/>.
    /// </summary>
    /// <param name="domainEntity">The domain entity to be converted.</param>
    /// <returns>The converted repository entity.</returns>
    public static AudioRatingEntity ToRepositoryEntity(this AudioRating domainEntity)
    {
        return new AudioRatingEntity(
            domainEntity.Value,
            domainEntity.MaxValue,
            domainEntity.Source.HasValue ? domainEntity.Source.Value : null,
            domainEntity.VoteCount.HasValue ? domainEntity.VoteCount.Value : null
        );
    }

    /// <summary>
    /// Converts <paramref name="domainEntities"/> to a collection of <see cref="AudioRatingEntity"/>.
    /// </summary>
    /// <param name="domainEntities">The domain entities to be converted.</param>
    /// <returns>The converted repository entities.</returns>
    public static IEnumerable<AudioRatingEntity> ToRepositoryEntities(this IEnumerable<AudioRating> domainEntities)
    {
        return domainEntities.Select(domainEntity => domainEntity.ToRepositoryEntity());
    }
}
