#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.Common.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;
#endregion

namespace Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Common;

/// <summary>
/// Extension methods for converting <see cref="MusicAreaDto"/>.
/// </summary>
public static class MusicAreaDtoMapping
{
    /// <summary>
    /// Converts <paramref name="areaDto"/> to a domain <see cref="MusicArea"/>, wrapped in an <see cref="Optional{T}"/>.
    /// </summary>
    /// <param name="areaDto">The area data transfer object to be converted.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either the converted optional area, or an error message.
    /// </returns>
    public static Result<Optional<MusicArea>> ToDomainValueObject(this MusicAreaDto? areaDto)
    {
        if (areaDto is null || areaDto.MusicBrainzAreaId is null)
            return Optional<MusicArea>.None();

        Result<MusicArea> areaResult = MusicArea.Create(
            MusicBrainzId.Create(areaDto.MusicBrainzAreaId.Value),
            areaDto.Name!,
            Optional<string>.FromNullable(areaDto.SortName),
            Optional<string>.FromNullable(areaDto.Disambiguation),
            Optional<string>.FromNullable(areaDto.Type),
            Optional<string>.FromNullable(areaDto.Iso3166Code));
        if (areaResult.IsFailure)
            return areaResult.Errors;
        return Optional<MusicArea>.Some(areaResult.Value);
    }
}
