#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Contracts.Entities.WrittenContentLibrary.BookLibrary;
using Lumina.Contracts.Enums.BookLibrary;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.Aggregates.WrittenContentLibrary.BookLibraryAggregate.ValueObjects;
using System.Collections.Generic;
using System.Linq;
#endregion

namespace Lumina.Application.Common.Mapping.WrittenContentLibrary.BookLibrary.Common;

/// <summary>
/// Extension methods for converting <see cref="BookRatingEntity"/>.
/// </summary>
public static class BookRatingModelMapping
{
    /// <summary>
    /// Converts <paramref name="repositoryEntity"/> to <see cref="BookRating"/>.
    /// </summary>
    /// <param name="repositoryEntity">The repository model to be converted.</param>
    /// <returns>The converted domain model.</returns>
    public static ErrorOr<BookRating> ToDomainModel(this BookRatingEntity repositoryEntity)
    {
        return BookRating.Create(
            repositoryEntity.Value ?? default,
            repositoryEntity.MaxValue ?? default,
            Optional<BookRatingSource>.FromNullable(repositoryEntity.Source),
            Optional<int>.FromNullable(repositoryEntity.VoteCount)
        );
    }

    /// <summary>
    /// Converts <paramref name="repositoryEntities"/> to a collection of <see cref="BookRating"/>.
    /// </summary>
    /// <param name="repositoryEntities">The repository models to be converted.</param>
    /// <returns>The converted domain models.</returns>
    public static IEnumerable<ErrorOr<BookRating>> ToDomainModels(this IEnumerable<BookRatingEntity> repositoryEntities)
    {
        return repositoryEntities.Select(domainModel => domainModel.ToDomainModel());
    }
}
