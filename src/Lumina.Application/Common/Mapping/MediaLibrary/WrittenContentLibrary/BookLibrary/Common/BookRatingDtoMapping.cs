#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Domain.Common.Enums.BookLibrary;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate.ValueObjects;
using System.Collections.Generic;
using System.Linq;
#endregion

namespace Lumina.Application.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Common;

/// <summary>
/// Extension methods for converting <see cref="BookRatingDto"/>.
/// </summary>
public static class BookRatingDtoMapping
{
    /// <summary>
    /// Converts <paramref name="dto"/> to <see cref="BookRating"/>.
    /// </summary>
    /// <param name="dto">The DTO to be converted.</param>
    /// <returns>The converted domain entity.</returns>
    public static ErrorOr<BookRating> ToDomainEntity(this BookRatingDto dto)
    {
        return BookRating.Create(
            dto.Value ?? default,
            dto.MaxValue ?? default,
            Optional<BookRatingSource>.FromNullable(dto.Source),
            Optional<int>.FromNullable(dto.VoteCount)
        );
    }

    /// <summary>
    /// Converts <paramref name="dtos"/> to a collection of <see cref="BookRating"/>.
    /// </summary>
    /// <param name="dtos">The DTOs to be converted.</param>
    /// <returns>The converted domain entities.</returns>
    public static IEnumerable<ErrorOr<BookRating>> ToDomainEntities(this IEnumerable<BookRatingDto> dtos)
    {
        return dtos.Select(domainEntity => domainEntity.ToDomainEntity());
    }
}
