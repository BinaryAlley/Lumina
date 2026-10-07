#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.Common;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Common.ValueObjects.Metadata;
using Lumina.Domain.SharedKernel.Common.Enums.Common;
using System;
#endregion

namespace Lumina.Application.Common.Mapping.Common.Metadata;

/// <summary>
/// Extension methods for converting <see cref="ReleaseInfoDto"/>.
/// </summary>
public static class ReleaseInfoDtoMapping
{
    /// <summary>
    /// Converts <paramref name="dto"/> to <see cref="ReleaseInfo"/>.
    /// </summary>
    /// <param name="dto">The DTO to be converted.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a successfully converted <see cref="ReleaseInfo"/>, or an error message.
    /// </returns>
    public static Result<ReleaseInfo> ToDomainValueObject(this ReleaseInfoDto dto)
    {
        return ReleaseInfo.Create(
            Optional<DateOnly>.FromNullable(dto.OriginalReleaseDate),
            Optional<int>.FromNullable(dto.OriginalReleaseYear),
            Optional<DateOnly>.FromNullable(dto.ReReleaseDate),
            Optional<int>.FromNullable(dto.ReReleaseYear),
            Optional<ReleaseCountry>.FromNullable(dto.ReleaseCountry),
            Optional<string>.FromNullable(dto.ReleaseVersion)
        );
    }
}
