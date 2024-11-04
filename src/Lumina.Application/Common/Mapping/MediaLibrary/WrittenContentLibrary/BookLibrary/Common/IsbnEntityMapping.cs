#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate.ValueObjects;
using System.Collections.Generic;
using System.Linq;
#endregion

namespace Lumina.Application.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Common;

/// <summary>
/// Extension methods for converting <see cref="IsbnEntity"/>.
/// </summary>
public static class IsbnEntityMapping
{
    /// <summary>
    /// Converts <paramref name="repositoryEntity"/> to <see cref="IsbnDto"/>.
    /// </summary>
    /// <param name="repositoryEntity">The repository entity to be converted.</param>
    /// <returns>The converted DTO.</returns>
    public static IsbnDto ToResponse(this IsbnEntity repositoryEntity)
    {
        return new IsbnDto(
            repositoryEntity.Value ?? default,
            repositoryEntity.Format ?? null
        );
    }

    /// <summary>
    /// Converts <paramref name="repositoryEntities"/> to a collection of <see cref="IsbnDto"/>.
    /// </summary>
    /// <param name="repositoryEntities">The repository entities to be converted.</param>
    /// <returns>The converted DTOs.</returns>
    public static IEnumerable<IsbnDto> ToResponses(this IEnumerable<IsbnEntity> repositoryEntities)
    {
        return repositoryEntities.Select(responseEntity => responseEntity.ToResponse());
    }

    /// <summary>
    /// Converts <paramref name="repositoryEntity"/> to <see cref="Isbn"/>.
    /// </summary>
    /// <param name="repositoryEntity">The repository entity to be converted.</param>
    /// <returns>The converted domain entity.</returns>
    public static ErrorOr<Isbn> ToDomainEntity(this IsbnEntity repositoryEntity)
    {
        return Isbn.Create(
            repositoryEntity.Value ?? default,
            repositoryEntity.Format ?? default
        );
    }

    /// <summary>
    /// Converts <paramref name="repositoryEntities"/> to a collection of <see cref="Isbn"/>.
    /// </summary>
    /// <param name="repositoryEntities">The repository entities to be converted.</param>
    /// <returns>The converted domain entities.</returns>
    public static IEnumerable<ErrorOr<Isbn>> ToDomainEntities(this IEnumerable<IsbnEntity> repositoryEntities)
    {
        return repositoryEntities.Select(domainEntity => domainEntity.ToDomainEntity());
    }
}
