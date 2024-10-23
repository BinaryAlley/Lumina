#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Contracts.Entities.WrittenContentLibrary.BookLibrary;
using Lumina.Contracts.Enums.BookLibrary;
using Lumina.Domain.Core.Aggregates.WrittenContentLibrary.BookLibraryAggregate.ValueObjects;
using System.Collections.Generic;
using System.Linq;
#endregion

namespace Lumina.Application.Common.Mapping.WrittenContentLibrary.BookLibrary.Common;

/// <summary>
/// Extension methods for converting <see cref="IsbnEntity"/>.
/// </summary>
public static class IsbnModelMapping
{
    /// <summary>
    /// Converts <paramref name="repositoryEntity"/> to <see cref="Isbn"/>.
    /// </summary>
    /// <param name="repositoryEntity">The repository model to be converted.</param>
    /// <returns>
    /// An <see cref="ErrorOr{TValue}"/> containing either a successfully converted <see cref="Isbn"/>, or an error message.
    /// </returns>
    public static ErrorOr<Isbn> ToDomainModel(this IsbnEntity repositoryEntity)
    {
        return Isbn.Create(
            repositoryEntity.Value,
            repositoryEntity.Format ?? IsbnFormat.Isbn13
        );
    }

    /// <summary>
    /// Converts <paramref name="repositoryEntities"/> to a collection of <see cref="Isbn"/>.
    /// </summary>
    /// <param name="repositoryEntities">The repository models to be converted.</param>
    /// <returns>The converted domain models.</returns>
    public static IEnumerable<ErrorOr<Isbn>> ToDomainModels(this IEnumerable<IsbnEntity> repositoryEntities)
    {
        return repositoryEntities.Select(domainModel => domainModel.ToDomainModel());
    }
}
