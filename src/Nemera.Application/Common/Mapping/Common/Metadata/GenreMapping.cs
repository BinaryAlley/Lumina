#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.DataAccess.Entities.Common;
using Nemera.Domain.Common.ValueObjects.Metadata;
using System.Collections.Generic;
using System.Linq;
#endregion

namespace Nemera.Application.Common.Mapping.Common.Metadata;

/// <summary>
/// Extension methods for converting <see cref="Genre"/>.
/// </summary>
public static class GenreMapping
{
    /// <summary>
    /// Converts <paramref name="domainEntity"/> to <see cref="GenreEntity"/>.
    /// </summary>
    /// <param name="domainEntity">The domain entity to be converted.</param>
    /// <returns>The converted repository entity.</returns>
    public static GenreEntity ToRepositoryEntity(this Genre domainEntity)
    {
        return new GenreEntity(
            domainEntity.Name
        );
    }

    /// <summary>
    /// Converts <paramref name="domainEntities"/> to a collection of <see cref="GenreEntity"/>.
    /// </summary>
    /// <param name="domainEntities">The domain entities to be converted.</param>
    /// <returns>The converted repository entities.</returns>
    public static IEnumerable<GenreEntity> ToRepositoryEntities(this IEnumerable<Genre> domainEntities)
    {
        return domainEntities.Select(domainEntity => domainEntity.ToRepositoryEntity());
    }
}
