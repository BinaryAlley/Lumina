#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.Common.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using System.Collections.Generic;
using System.Linq;
#endregion

namespace Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Common;

/// <summary>
/// Extension methods for converting <see cref="AudioRatingDto"/>.
/// </summary>
public static class AudioRatingDtoMapping
{
    /// <summary>
    /// Converts <paramref name="dto"/> to <see cref="AudioRating"/>.
    /// </summary>
    /// <param name="dto">The DTO to be converted.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a successfully converted <see cref="AudioRating"/>, or an error message.
    /// </returns>
    public static Result<AudioRating> ToDomainValueObject(this AudioRatingDto dto)
    {
        return AudioRating.Create(
            dto.Value ?? default,
            dto.MaxValue ?? default,
            Optional<AudioRatingSource>.FromNullable(dto.Source),
            Optional<int>.FromNullable(dto.VoteCount)
        );
    }

    /// <summary>
    /// Converts <paramref name="dtos"/> to a collection of <see cref="AudioRating"/>.
    /// </summary>
    /// <param name="dtos">The DTOs to be converted.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a collection of converted <see cref="AudioRating"/>, or an error message.
    /// </returns>
    public static IEnumerable<Result<AudioRating>> ToDomainValueObjects(this IEnumerable<AudioRatingDto> dtos)
    {
        return dtos.Select(dto => dto.ToDomainValueObject());
    }
}
