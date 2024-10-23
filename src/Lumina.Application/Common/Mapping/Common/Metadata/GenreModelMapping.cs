#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Contracts.Entities.Common;
using Lumina.Domain.Common.ValueObjects.Metadata;
using System.Collections.Generic;
using System.Linq;
#endregion

namespace Lumina.Application.Common.Mapping.Common.Metadata;

/// <summary>
/// Extension methods for converting <see cref="GenreEntity"/>.
/// </summary>
public static class GenreModelMapping
{
    /// <summary>
    /// Converts <paramref name="repositoryEntity"/> to <see cref="GenreEntity"/>.
    /// </summary>
    /// <param name="repositoryEntity">The repository model to be converted.</param>
    /// <returns>The converted domain model.</returns>
    public static ErrorOr<Genre> ToDomainModel(this GenreEntity repositoryEntity)
    {
        return Genre.Create(
            repositoryEntity.Name
        );
    }

    /// <summary>
    /// Converts <paramref name="repositoryEntities"/> to a collection of <see cref="Genre"/>.
    /// </summary>
    /// <param name="repositoryEntities">The repository models to be converted.</param>
    /// <returns>The converted domain models.</returns>
    public static IEnumerable<ErrorOr<Genre>> ToDomainModels(this IEnumerable<GenreEntity> repositoryEntities)
    {
        return repositoryEntities.Select(repositoryEntity => repositoryEntity.ToDomainModel());
    }
}
