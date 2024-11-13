#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Domain.Common.Enums.BookLibrary;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate.ValueObjects;
using System.Collections.Generic;
using System.Linq;
#endregion

namespace Lumina.Application.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Common;

/// <summary>
/// Extension methods for converting <see cref="BookRatingEntity"/>.
/// </summary>
public static class BookRatingEntityMapping
{
    /// <summary>
    /// Converts <paramref name="repositoryEntity"/> to <see cref="BookRatingDto"/>.
    /// </summary>
    /// <param name="repositoryEntity">The repository entity to be converted.</param>
    /// <returns>The converted DTO.</returns>
    public static BookRatingDto ToResponse(this BookRatingEntity repositoryEntity)
    {
        return new BookRatingDto(
            repositoryEntity.Value ?? default,
            repositoryEntity.MaxValue ?? default,
            repositoryEntity.Source,
            repositoryEntity.VoteCount
        );
    }

    /// <summary>
    /// Converts <paramref name="repositoryEntities"/> to a collection of <see cref="BookRatingDto"/>.
    /// </summary>
    /// <param name="repositoryEntities">The repository entities to be converted.</param>
    /// <returns>The converted DTOs.</returns>
    public static IEnumerable<BookRatingDto> ToResponses(this IEnumerable<BookRatingEntity> repositoryEntities)
    {
        return repositoryEntities.Select(responseEntity => responseEntity.ToResponse());
    }

    /// <summary>
    /// Converts <paramref name="repositoryEntity"/> to <see cref="BookRating"/>.
    /// </summary>
    /// <param name="repositoryEntity">The repository entity to be converted.</param>
    /// <returns>The converted domain entity.</returns>
    public static ErrorOr<BookRating> ToDomainEntity(this BookRatingEntity repositoryEntity)
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
    /// <param name="repositoryEntities">The repository entities to be converted.</param>
    /// <returns>The converted domain entities.</returns>
    public static IEnumerable<ErrorOr<BookRating>> ToDomainEntities(this IEnumerable<BookRatingEntity> repositoryEntities)
    {
        return repositoryEntities.Select(domainEntity => domainEntity.ToDomainEntity());
    }
}
