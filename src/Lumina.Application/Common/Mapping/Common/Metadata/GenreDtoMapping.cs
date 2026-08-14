#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using Lumina.Contracts.DTO.Common;
using Lumina.Domain.Common.ValueObjects.Metadata;
using System.Collections.Generic;
using System.Linq;
#endregion

namespace Lumina.Application.Common.Mapping.Common.Metadata;

/// <summary>
/// Extension methods for converting <see cref="GenreDto"/>.
/// </summary>
public static class GenreDtoMapping
{
    /// <summary>
    /// Converts <paramref name="dto"/> to <see cref="GenreDto"/>.
    /// </summary>
    /// <param name="dto">The DTO to be converted.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a successfully converted <see cref="Genre"/>, or an error message.
    /// </returns>
    public static Result<Genre> ToDomainEntity(this GenreDto dto)
    {
        return Genre.Create(
            dto.Name
        );
    }

    /// <summary>
    /// Converts <paramref name="dtos"/> to a collection of <see cref="Genre"/>.
    /// </summary>
    /// <param name="dtos">The DTOs to be converted.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a collection of converted <see cref="Genre"/>, or an error message.
    /// </returns>
    public static IEnumerable<Result<Genre>> ToDomainEntities(this IEnumerable<GenreDto> dtos)
    {
        return dtos.Select(repositoryEntity => repositoryEntity.ToDomainEntity());
    }
}
