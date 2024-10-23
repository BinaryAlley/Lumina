#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Entities.WrittenContentLibrary.BookLibrary;
using Lumina.Domain.Core.Aggregates.WrittenContentLibrary.BookLibraryAggregate.ValueObjects;
using System.Collections.Generic;
using System.Linq;
#endregion

namespace Lumina.Application.Common.Mapping.WrittenContentLibrary.BookLibrary.Common;

/// <summary>
/// Extension methods for converting <see cref="BookRating"/>.
/// </summary>
public static class BookRatingMapping
{
    /// <summary>
    /// Converts <paramref name="domainModel"/> to <see cref="BookRatingEntity"/>.
    /// </summary>
    /// <param name="domainModel">The domain model to be converted.</param>
    /// <returns>The converted repository entity.</returns>
    public static BookRatingEntity ToRepositoryEntity(this BookRating domainModel)
    {
        return new BookRatingEntity(
            domainModel.Value,
            domainModel.MaxValue,
            domainModel.Source.HasValue ? domainModel.Source.Value : null,
            domainModel.VoteCount.HasValue ? domainModel.VoteCount.Value : null
        );
    }

    /// <summary>
    /// Converts <paramref name="domainModels"/> to a collection of <see cref="BookRatingEntity"/>.
    /// </summary>
    /// <param name="domainModels">The domain models to be converted.</param>
    /// <returns>The converted repository entities.</returns>
    public static IEnumerable<BookRatingEntity> ToRepositoryEntities(this IEnumerable<BookRating> domainModels)
    {
        return domainModels.Select(domainModel => domainModel.ToRepositoryEntity());
    }
}
