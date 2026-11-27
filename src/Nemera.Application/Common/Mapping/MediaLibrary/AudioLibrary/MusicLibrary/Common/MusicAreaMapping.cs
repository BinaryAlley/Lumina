#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;
#endregion

namespace Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Common;

/// <summary>
/// Extension methods for converting <see cref="MusicArea"/>.
/// </summary>
public static class MusicAreaMapping
{
    /// <summary>
    /// Converts <paramref name="domainEntity"/> to <see cref="MusicAreaEntity"/>.
    /// </summary>
    /// <param name="domainEntity">The domain entity to be converted.</param>
    /// <returns>The converted repository entity.</returns>
    public static MusicAreaEntity ToRepositoryEntity(this MusicArea domainEntity)
    {
        return new MusicAreaEntity(
            domainEntity.MusicBrainzAreaId.Value,
            domainEntity.Name,
            domainEntity.SortName.HasValue ? domainEntity.SortName.Value : null,
            domainEntity.Disambiguation.HasValue ? domainEntity.Disambiguation.Value : null,
            domainEntity.Type.HasValue ? domainEntity.Type.Value : null,
            domainEntity.Iso3166Code.HasValue ? domainEntity.Iso3166Code.Value : null);
    }
}
