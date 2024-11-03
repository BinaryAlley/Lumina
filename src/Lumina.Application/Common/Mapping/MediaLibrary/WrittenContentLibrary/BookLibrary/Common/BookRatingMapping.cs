#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate.ValueObjects;
using System.Collections.Generic;
using System.Linq;
#endregion

namespace Lumina.Application.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Common;

/// <summary>
/// Extension methods for converting <see cref="BookRating"/>.
/// </summary>
public static class BookRatingMapping
{
    /// <summary>
    /// Converts <paramref name="domainEntity"/> to <see cref="BookRatingEntity"/>.
    /// </summary>
    /// <param name="domainEntity">The domain entity to be converted.</param>
    /// <returns>The converted repository entity.</returns>
    public static BookRatingEntity ToRepositoryEntity(this BookRating domainEntity)
    {
        return new BookRatingEntity(
            domainEntity.Value,
            domainEntity.MaxValue,
            domainEntity.Source.HasValue ? domainEntity.Source.Value : null,
            domainEntity.VoteCount.HasValue ? domainEntity.VoteCount.Value : null
        );
    }

    /// <summary>
    /// Converts <paramref name="domainEntities"/> to a collection of <see cref="BookRatingEntity"/>.
    /// </summary>
    /// <param name="domainEntities">The domain entities to be converted.</param>
    /// <returns>The converted repository entities.</returns>
    public static IEnumerable<BookRatingEntity> ToRepositoryEntities(this IEnumerable<BookRating> domainEntities)
    {
        return domainEntities.Select(domainEntity => domainEntity.ToRepositoryEntity());
    }
}
