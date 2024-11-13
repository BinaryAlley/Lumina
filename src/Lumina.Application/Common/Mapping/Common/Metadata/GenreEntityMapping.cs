#region ========================================================================= USING =====================================================================================
using ErrorOr;
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
    /// <returns>The converted domain entity.</returns>
    public static ErrorOr<Genre> ToDomainEntity(this GenreEntity repositoryEntity)
    {
        return Genre.Create(
            repositoryEntity.Name ?? default
        );
    }

    /// <summary>
    /// Converts <paramref name="repositoryEntities"/> to a collection of <see cref="Genre"/>.
    /// </summary>
    /// <param name="repositoryEntities">The repository entities to be converted.</param>
    /// <returns>The converted domain entities.</returns>
    public static IEnumerable<ErrorOr<Genre>> ToDomainEntities(this IEnumerable<GenreEntity> repositoryEntities)
    {
        return repositoryEntities.Select(domainEntity => domainEntity.ToDomainEntity());
    }
}
