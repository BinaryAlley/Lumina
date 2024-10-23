#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Entities.Common;
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
    /// Converts <paramref name="domainModel"/> to <see cref="TagEntity"/>.
    /// </summary>
    /// <param name="domainModel">The domain model to be converted.</param>
    /// <returns>The converted repository entity.</returns>
    public static TagEntity ToRepositoryEntity(this Tag domainModel)
    {
        return new TagEntity(
            domainModel.Name
        );
    }

    /// <summary>
    /// Converts <paramref name="domainModels"/> to a collection of <see cref="TagEntity"/>.
    /// </summary>
    /// <param name="domainModels">The domain model to be converted.</param>
    /// <returns>The converted repository entities.</returns>
    public static IEnumerable<TagEntity> ToRepositoryEntities(this IEnumerable<Tag> domainModels)
    {
        return domainModels.Select(domainModel => domainModel.ToRepositoryEntity());
    }
}
