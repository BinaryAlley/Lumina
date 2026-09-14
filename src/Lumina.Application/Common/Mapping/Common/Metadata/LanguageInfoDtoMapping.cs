#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.Common;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Common.ValueObjects.Metadata;
#endregion

namespace Lumina.Application.Common.Mapping.Common.Metadata;

/// <summary>
/// Extension methods for converting <see cref="LanguageInfoDto"/>.
/// </summary>
public static class LanguageInfoDtoMapping
{
    /// <summary>
    /// Converts <paramref name="dto"/> to <see cref="LanguageInfoDto"/>.
    /// </summary>
    /// <param name="dto">The DTO to be converted.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a successfully converted <see cref="LanguageInfo"/>, or an error message.
    /// </returns>
    public static Result<LanguageInfo> ToDomainEntity(this LanguageInfoDto dto)
    {
        return LanguageInfo.Create(
            dto.LanguageCode!,
            dto.LanguageName!,
            Optional<string>.FromNullable(dto.NativeName)
        );
    }
}
