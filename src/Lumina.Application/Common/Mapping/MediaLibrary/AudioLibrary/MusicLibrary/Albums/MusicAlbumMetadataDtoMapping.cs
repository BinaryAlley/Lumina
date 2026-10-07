#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.Common.Metadata;
using Lumina.Contracts.DTO.Common;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Common.ValueObjects.Metadata;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.Common;
using System;
using System.Collections.Generic;
#endregion

namespace Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Albums;

/// <summary>
/// Extension methods for converting <see cref="MusicAlbumMetadataDto"/>.
/// </summary>
public static class MusicAlbumMetadataDtoMapping
{
    /// <summary>
    /// Converts <paramref name="dto"/> to a domain <see cref="AlbumMetadata"/>.
    /// </summary>
    /// <param name="dto">The data transfer object to be converted.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a successfully converted <see cref="AlbumMetadata"/>, or an error message.
    /// </returns>
    public static Result<AlbumMetadata> ToDomainValueObject(this MusicAlbumMetadataDto dto)
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
            Result<Genre> genreResult = genre.ToDomainValueObject();
            if (genreResult.IsFailure)
                return genreResult.Errors;
            domainGenres.Add(genreResult.Value);
        }

        List<Tag> domainTags = [];
        foreach (TagDto tag in dto.Tags ?? [])
        {
            Result<Tag> tagResult = tag.ToDomainValueObject();
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

        return AlbumMetadata.Create(
            dto.Title!,
            Optional<string>.FromNullable(dto.OriginalTitle),
            Optional<string>.FromNullable(dto.ReleaseTitle),
            Optional<string>.FromNullable(dto.Description),
            releaseInfoResult.Value,
            domainGenres,
            domainTags,
            language,
            originalLanguage,
            dto.ReleaseTypes is null ? [] : [.. dto.ReleaseTypes],
            Optional<MusicReleaseStatus>.FromNullable(dto.ReleaseStatus),
            Optional<int>.FromNullable(dto.TotalDiscs),
            dto.TotalTracks ?? 0);
    }
}
