#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Entities.WrittenContentLibrary.BookLibrary;
using Lumina.Domain.Core.Aggregates.WrittenContentLibrary.BookLibraryAggregate.ValueObjects;
using System.Collections.Generic;
using System.Linq;
#endregion

namespace Lumina.Application.Common.Mapping.WrittenContentLibrary.BookLibrary.Common;

/// <summary>
/// Extension methods for converting <see cref="Isbn"/>.
/// </summary>
public static class IsbnMapping
{
    /// <summary>
    /// Converts <paramref name="domainModel"/> to <see cref="IsbnEntity"/>.
    /// </summary>
    /// <param name="domainModel">The domain model to be converted.</param>
    /// <returns>The converted repository entity.</returns>
    public static IsbnEntity ToRepositoryEntity(this Isbn domainModel)
    {
        return new IsbnEntity(
            domainModel.Value,
            domainModel.Format
        );
    }

    /// <summary>
    /// Converts <paramref name="domainModels"/> to a collection of <see cref="IsbnEntity"/>.
    /// </summary>
    /// <param name="domainModels">The domain models to be converted.</param>
    /// <returns>The converted repository entities.</returns>
    public static IEnumerable<IsbnEntity> ToRepositoryEntities(this IEnumerable<Isbn> domainModels)
    {
        return domainModels.Select(domainModel => domainModel.ToRepositoryEntity());
    }
}
