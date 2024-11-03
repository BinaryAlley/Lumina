#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Contracts.Entities.Common;
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
    /// Converts <paramref name="repositoryEntity"/> to <see cref="Tag"/>.
    /// </summary>
    /// <param name="repositoryEntity">The repository entity to be converted.</param>
    /// <returns>The converted domain entity.</returns>
    public static ErrorOr<Tag> ToDomainEntity(this TagEntity repositoryEntity)
    {
        return Tag.Create(
            repositoryEntity.Name
        );
    }

    /// <summary>
    /// Converts <paramref name="repositoryEntities"/> to a collection of <see cref="Tag"/>.
    /// </summary>
    /// <param name="repositoryEntities">The repository entities to be converted.</param>
    /// <returns>The converted domain entities.</returns>
    public static IEnumerable<ErrorOr<Tag>> ToDomainEntities(this IEnumerable<TagEntity> repositoryEntities)
    {
        return repositoryEntities.Select(domainEntity => domainEntity.ToDomainEntity());
    }
}