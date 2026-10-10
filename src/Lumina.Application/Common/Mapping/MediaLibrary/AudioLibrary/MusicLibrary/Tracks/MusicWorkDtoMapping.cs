#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.Common;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Common.ValueObjects.Metadata;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;
using System.Collections.Generic;
using System.Linq;
#endregion

namespace Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;

/// <summary>
/// Extension methods for converting <see cref="MusicWorkDto"/>.
/// </summary>
public static class MusicWorkDtoMapping
{
    /// <summary>
    /// Converts <paramref name="dto"/> to a domain <see cref="MusicWork"/>.
    /// </summary>
    /// <param name="dto">The data transfer object to be converted.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a successfully converted <see cref="MusicWork"/>, or an error message.
    /// </returns>
    public static Result<MusicWork> ToDomainValueObject(this MusicWorkDto dto)
    {
        if (dto.MusicBrainzWorkId is null)
            return Domain.Common.Errors.Errors.Music.MusicBrainzIdInvalidFormat;
        if (string.IsNullOrWhiteSpace(dto.Title))
            return Domain.Common.Errors.Errors.Music.WorkTitleCannotBeEmpty;

        List<LanguageInfo> languages = [];
        foreach (LanguageInfoDto languageDto in dto.Languages ?? [])
        {
            if (string.IsNullOrWhiteSpace(languageDto.LanguageCode) || string.IsNullOrWhiteSpace(languageDto.LanguageName))
                continue;
            languages.Add(LanguageInfo.Create(languageDto.LanguageCode, languageDto.LanguageName, Optional<string>.FromNullable(languageDto.NativeName)));
        }

        List<string> iswcs = [.. (dto.Iswcs ?? []).Where(iswc => !string.IsNullOrWhiteSpace(iswc)).Select(iswc => iswc.Trim())];

        return MusicWork.Create(
            MusicBrainzId.Create(dto.MusicBrainzWorkId.Value),
            dto.Title,
            Optional<string>.FromNullable(dto.Type),
            languages,
            iswcs);
    }
}
