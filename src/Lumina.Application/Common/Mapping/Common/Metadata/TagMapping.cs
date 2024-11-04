#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Common;
using Lumina.Domain.Common.ValueObjects.Metadata;
using System.Collections.Generic;
using System.Linq;
#endregion

namespace Lumina.Application.Common.Mapping.Common.Metadata;

/// <summary>
/// Extension methods for converting <see cref="Tag"/>.
/// </summary>
public static class TagMapping
{
    /// <summary>
    /// Converts <paramref name="domainEntity"/> to <see cref="TagEntity"/>.
    /// </summary>
    /// <param name="domainEntity">The domain entity to be converted.</param>
    /// <returns>The converted repository entity.</returns>
    public static TagEntity ToRepositoryEntity(this Tag domainEntity)
    {
        return new TagEntity(
            domainEntity.Name
        );
    }

    /// <summary>
    /// Converts <paramref name="domainEntities"/> to a collection of <see cref="TagEntity"/>.
    /// </summary>
    /// <param name="domainEntities">The domain entity to be converted.</param>
    /// <returns>The converted repository entities.</returns>
    public static IEnumerable<TagEntity> ToRepositoryEntities(this IEnumerable<Tag> domainEntities)
    {
        return domainEntities.Select(domainEntity => domainEntity.ToRepositoryEntity());
    }
}
