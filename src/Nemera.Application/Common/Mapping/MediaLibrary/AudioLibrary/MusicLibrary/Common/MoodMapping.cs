#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Nemera.Domain.Common.ValueObjects.Metadata;
using System.Collections.Generic;
using System.Linq;
#endregion

namespace Nemera.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Common;

/// <summary>
/// Extension methods for converting <see cref="Mood"/>.
/// </summary>
public static class MoodMapping
{
    /// <summary>
    /// Converts <paramref name="domainEntity"/> to <see cref="TrackMoodEntity"/>.
    /// </summary>
    /// <param name="domainEntity">The domain entity to be converted.</param>
    /// <returns>The converted repository entity.</returns>
    public static TrackMoodEntity ToRepositoryEntity(this Mood domainEntity)
    {
        return new TrackMoodEntity(
            domainEntity.Name
        );
    }

    /// <summary>
    /// Converts <paramref name="domainEntities"/> to a collection of <see cref="TrackMoodEntity"/>.
    /// </summary>
    /// <param name="domainEntities">The domain entities to be converted.</param>
    /// <returns>The converted repository entities.</returns>
    public static IEnumerable<TrackMoodEntity> ToRepositoryEntities(this IEnumerable<Mood> domainEntities)
    {
        return domainEntities.Select(domainEntity => domainEntity.ToRepositoryEntity());
    }
}
