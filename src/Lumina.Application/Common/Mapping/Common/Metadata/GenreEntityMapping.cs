#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using Lumina.Application.Common.DataAccess.Entities.Common;
using Lumina.Contracts.DTO.Common;
using Lumina.Domain.Common.ValueObjects.Metadata;
using System.Collections.Generic;
using System.Linq;
#endregion

namespace Lumina.Application.Common.Mapping.Common.Metadata;

/// <summary>
/// Extension methods for converting <see cref="GenreEntity"/>.
/// </summary>
public static class GenreEntityMapping
{
    /// <summary>
    /// Converts <paramref name="repositoryEntity"/> to <see cref="GenreDto"/>.
    /// </summary>
    /// <param name="repositoryEntity">The repository entity to be converted.</param>
    /// <returns>The converted DTO.</returns>
    public static GenreDto ToResponse(this GenreEntity repositoryEntity)
    {
        return new GenreDto(
            repositoryEntity.Name ?? default
        );
    }

    /// <summary>
    /// Converts <paramref name="repositoryEntities"/> to a collection of <see cref="GenreDto"/>.
    /// </summary>
    /// <param name="repositoryEntities">The repository entities to be converted.</param>
    /// <returns>The converted DTOs.</returns>
    public static IEnumerable<GenreDto> ToResponses(this IEnumerable<GenreEntity> repositoryEntities)
    {
        return repositoryEntities.Select(responseEntity => responseEntity.ToResponse());
    }

    /// <summary>
    /// Converts <paramref name="repositoryEntity"/> to <see cref="Genre"/>.
    /// </summary>
    /// <param name="repositoryEntity">The repository entity to be converted.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a successfully converted <see cref="Genre"/>, or an error message.
    /// </returns>
    public static Result<Genre> ToDomainEntity(this GenreEntity repositoryEntity)
    {
        return Genre.Create(
            repositoryEntity.Name ?? default
        );
    }

    /// <summary>
    /// Converts <paramref name="repositoryEntities"/> to a collection of <see cref="Genre"/>.
    /// </summary>
    /// <param name="repositoryEntities">The repository entities to be converted.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a collection of converted <see cref="Genre"/>, or an error message.
    /// </returns>
    public static IEnumerable<Result<Genre>> ToDomainEntities(this IEnumerable<GenreEntity> repositoryEntities)
    {
        return repositoryEntities.Select(domainEntity => domainEntity.ToDomainEntity());
    }
}
