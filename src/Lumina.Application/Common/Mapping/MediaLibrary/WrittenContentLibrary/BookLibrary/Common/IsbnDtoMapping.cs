#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Domain.Common.Enums.BookLibrary;
using Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate.ValueObjects;
using System.Collections.Generic;
using System.Linq;
#endregion

namespace Lumina.Application.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Common;

/// <summary>
/// Extension methods for converting <see cref="IsbnDto"/>.
/// </summary>
public static class IsbnDtoMapping
{
    /// <summary>
    /// Converts <paramref name="dto"/> to <see cref="Isbn"/>.
    /// </summary>
    /// <param name="dto">The DTO to be converted.</param>
    /// <returns>
    /// An <see cref="ErrorOr{TValue}"/> containing either a successfully converted <see cref="Isbn"/>, or an error message.
    /// </returns>
    public static ErrorOr<Isbn> ToDomainEntity(this IsbnDto dto)
    {
        return Isbn.Create(
            dto.Value,
            dto.Format ?? IsbnFormat.Isbn13
        );
    }

    /// <summary>
    /// Converts <paramref name="dtos"/> to a collection of <see cref="Isbn"/>.
    /// </summary>
    /// <param name="dtos">The DTOs to be converted.</param>
    /// <returns>The converted domain entities.</returns>
    public static IEnumerable<ErrorOr<Isbn>> ToDomainEntities(this IEnumerable<IsbnDto> dtos)
    {
        return dtos.Select(domainEntity => domainEntity.ToDomainEntity());
    }
}
