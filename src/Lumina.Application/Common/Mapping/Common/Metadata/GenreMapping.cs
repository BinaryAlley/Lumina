#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Entities.Common;
using Lumina.Domain.Common.ValueObjects.Metadata;
using System.Collections.Generic;
using System.Linq;
#endregion

namespace Lumina.Application.Common.Mapping.Common.Metadata;

/// <summary>
/// Extension methods for converting <see cref="Genre"/>.
/// </summary>
public static class GenreMapping
{
    /// <summary>
    /// Converts <paramref name="domainModel"/> to <see cref="GenreEntity"/>.
    /// </summary>
    /// <param name="domainModel">The domain model to be converted.</param>
    /// <returns>The converted repository entity.</returns>
    public static GenreEntity ToRepositoryEntity(this Genre domainModel)
    {
        return new GenreEntity(
            domainModel.Name
        );
    }

    /// <summary>
    /// Converts <paramref name="domainModels"/> to a collection of <see cref="GenreEntity"/>.
    /// </summary>
    /// <param name="domainModels">The domain models to be converted.</param>
    /// <returns>The converted repository entities.</returns>
    public static IEnumerable<GenreEntity> ToRepositoryEntities(this IEnumerable<Genre> domainModels)
    {
        return domainModels.Select(domainModel => domainModel.ToRepositoryEntity());
    }
}
