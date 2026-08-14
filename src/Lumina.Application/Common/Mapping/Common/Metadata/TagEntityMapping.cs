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
/// Extension methods for converting <see cref="TagEntity"/>.
/// </summary>
public static class TagEntityMapping
{
    /// <summary>
    /// Converts <paramref name="repositoryEntity"/> to <see cref="TagDto"/>.
    /// </summary>
    /// <param name="repositoryEntity">The repository entity to be converted.</param>
    /// <returns>The converted DTO.</returns>
    public static TagDto ToResponse(this TagEntity repositoryEntity)
    {
        return new TagDto(
            repositoryEntity.Name ?? default
        );
    }

    /// <summary>
    /// Converts <paramref name="repositoryEntities"/> to a collection of <see cref="TagDto"/>.
    /// </summary>
    /// <param name="repositoryEntities">The repository entities to be converted.</param>
    /// <returns>The converted DTOs.</returns>
    public static IEnumerable<TagDto> ToResponses(this IEnumerable<TagEntity> repositoryEntities)
    {
        return repositoryEntities.Select(responseEntity => responseEntity.ToResponse());
    }

    /// <summary>
    /// Converts <paramref name="repositoryEntity"/> to <see cref="Tag"/>.
    /// </summary>
    /// <param name="repositoryEntity">The repository entity to be converted.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a successfully converted <see cref="Tag"/>, or an error message.
    /// </returns>
    public static Result<Tag> ToDomainEntity(this TagEntity repositoryEntity)
    {
        return Tag.Create(
            repositoryEntity.Name ?? default
        );
    }

    /// <summary>
    /// Converts <paramref name="repositoryEntities"/> to a collection of <see cref="Tag"/>.
    /// </summary>
    /// <param name="repositoryEntities">The repository entities to be converted.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a collection of converted <see cref="Tag"/>, or an error message.
    /// </returns>
    public static IEnumerable<Result<Tag>> ToDomainEntities(this IEnumerable<TagEntity> repositoryEntities)
    {
        return repositoryEntities.Select(domainEntity => domainEntity.ToDomainEntity());
    }
}
