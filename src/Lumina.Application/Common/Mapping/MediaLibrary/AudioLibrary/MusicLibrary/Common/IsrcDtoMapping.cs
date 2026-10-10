#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;
using System.Collections.Generic;
using System.Linq;
#endregion

namespace Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Common;

/// <summary>
/// Extension methods for converting <see cref="IsrcDto"/>.
/// </summary>
public static class IsrcDtoMapping
{
    /// <summary>
    /// Converts <paramref name="dto"/> to <see cref="Isrc"/>.
    /// </summary>
    /// <param name="dto">The DTO to be converted.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a successfully converted <see cref="Isrc"/>, or an error message.
    /// </returns>
    public static Result<Isrc> ToDomainValueObject(this IsrcDto dto)
    {
        return Isrc.Create(dto.Value!);
    }

    /// <summary>
    /// Converts <paramref name="dtos"/> to a collection of <see cref="Isrc"/>.
    /// </summary>
    /// <param name="dtos">The DTOs to be converted.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a collection of converted <see cref="Isrc"/>, or an error message.
    /// </returns>
    public static IEnumerable<Result<Isrc>> ToDomainValueObjects(this IEnumerable<IsrcDto> dtos)
    {
        return dtos.Select(dto => dto.ToDomainValueObject());
    }
}
