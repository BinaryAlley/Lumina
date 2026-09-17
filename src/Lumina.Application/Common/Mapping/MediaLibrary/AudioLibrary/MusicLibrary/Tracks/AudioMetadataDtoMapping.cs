#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.Common.Metadata;
using Lumina.Contracts.DTO.Common;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Common.ValueObjects.Metadata;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.Common.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.Common;
using System;
using System.Collections.Generic;
#endregion

namespace Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;

/// <summary>
/// Extension methods for converting <see cref="AudioMetadataDto"/>.
/// </summary>
public static class AudioMetadataDtoMapping
{
    /// <summary>
    /// Converts <paramref name="dto"/> to a domain <see cref="AudioMetadata"/>.
    /// </summary>
    /// <param name="dto">The data transfer object to be converted.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a successfully converted <see cref="AudioMetadata"/>, or an error message.
    /// </returns>
    public static Result<AudioMetadata> ToDomainEntity(this AudioMetadataDto dto)
    {
        ReleaseInfoDto? releaseInfoDto = dto.ReleaseInfo;
        Result<ReleaseInfo> releaseInfoResult = ReleaseInfo.Create(
            Optional<DateOnly>.FromNullable(releaseInfoDto?.OriginalReleaseDate),
            Optional<int>.FromNullable(releaseInfoDto?.OriginalReleaseYear),
            Optional<DateOnly>.FromNullable(releaseInfoDto?.ReReleaseDate),
            Optional<int>.FromNullable(releaseInfoDto?.ReReleaseYear),
            Optional<ReleaseCountry>.FromNullable(releaseInfoDto?.ReleaseCountry),
            Optional<string>.FromNullable(releaseInfoDto?.ReleaseVersion));
        if (releaseInfoResult.IsFailure)
            return releaseInfoResult.Errors;

        List<Genre> domainGenres = [];
        foreach (GenreDto genre in dto.Genres ?? [])
        {
            Result<Genre> genreResult = genre.ToDomainEntity();
            if (genreResult.IsFailure)
                return genreResult.Errors;
            domainGenres.Add(genreResult.Value);
        }

        List<Tag> domainTags = [];
        foreach (TagDto tag in dto.Tags ?? [])
        {
            Result<Tag> tagResult = tag.ToDomainEntity();
            if (tagResult.IsFailure)
                return tagResult.Errors;
            domainTags.Add(tagResult.Value);
        }

        Optional<LanguageInfo> language = Optional<LanguageInfo>.None();
        if (dto.Language is not null && dto.Language.LanguageCode is not null && dto.Language.LanguageName is not null)
            language = LanguageInfo.Create(dto.Language.LanguageCode, dto.Language.LanguageName, Optional<string>.FromNullable(dto.Language.NativeName));

        Optional<LanguageInfo> originalLanguage = Optional<LanguageInfo>.None();
        if (dto.OriginalLanguage is not null && dto.OriginalLanguage.LanguageCode is not null && dto.OriginalLanguage.LanguageName is not null)
            originalLanguage = LanguageInfo.Create(dto.OriginalLanguage.LanguageCode, dto.OriginalLanguage.LanguageName, Optional<string>.FromNullable(dto.OriginalLanguage.NativeName));

        return AudioMetadata.Create(
            dto.Title!,
            Optional<string>.FromNullable(dto.OriginalTitle),
            dto.DurationInSeconds ?? 0,
            dto.SampleRate ?? 0,
            dto.Channels ?? 0,
            releaseInfoResult.Value,
            Optional<string>.FromNullable(dto.Description),
            domainGenres,
            domainTags,
            language,
            originalLanguage,
            Optional<int>.FromNullable(dto.BitDepth),
            Optional<string>.FromNullable(dto.AudioCodec),
            Optional<int>.FromNullable(dto.Bitrate));
    }
}
