#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate.ValueObjects;
using System.Collections.Generic;
using System.Linq;
#endregion

namespace Lumina.Application.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Common;

/// <summary>
/// Extension methods for converting <see cref="Isbn"/>.
/// </summary>
public static class IsbnMapping
{
    /// <summary>
    /// Converts <paramref name="domainEntity"/> to <see cref="IsbnEntity"/>.
    /// </summary>
    /// <param name="domainEntity">The domain entity to be converted.</param>
    /// <returns>The converted repository entity.</returns>
    public static IsbnEntity ToRepositoryEntity(this Isbn domainEntity)
    {
        return new IsbnEntity(
            domainEntity.Value,
            domainEntity.Format
        );
    }

    /// <summary>
    /// Converts <paramref name="domainEntities"/> to a collection of <see cref="IsbnEntity"/>.
    /// </summary>
    /// <param name="domainEntities">The domain entities to be converted.</param>
    /// <returns>The converted repository entities.</returns>
    public static IEnumerable<IsbnEntity> ToRepositoryEntities(this IEnumerable<Isbn> domainEntities)
    {
        return domainEntities.Select(domainEntity => domainEntity.ToRepositoryEntity());
    }
}
