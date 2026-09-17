#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.Common;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Common.ValueObjects.Metadata;
using System.Collections.Generic;
using System.Linq;
#endregion

namespace Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Common;

/// <summary>
/// Extension methods for converting <see cref="MoodDto"/>.
/// </summary>
public static class MoodDtoMapping
{
    /// <summary>
    /// Converts <paramref name="dto"/> to <see cref="Mood"/>.
    /// </summary>
    /// <param name="dto">The DTO to be converted.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a successfully converted <see cref="Mood"/>, or an error message.
    /// </returns>
    public static Result<Mood> ToDomainEntity(this MoodDto dto)
    {
        return Mood.Create(dto.Name);
    }

    /// <summary>
    /// Converts <paramref name="dtos"/> to a collection of <see cref="Mood"/>.
    /// </summary>
    /// <param name="dtos">The DTOs to be converted.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a collection of converted <see cref="Mood"/>, or an error message.
    /// </returns>
    public static IEnumerable<Result<Mood>> ToDomainEntities(this IEnumerable<MoodDto> dtos)
    {
        return dtos.Select(dto => dto.ToDomainEntity());
    }
}
