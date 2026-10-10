#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.Common.Metadata;
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Common;
using Lumina.Contracts.DTO.Common;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Common.ValueObjects.Metadata;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.Common.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.Entities;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.Common;
using System;
using System.Collections.Generic;
#endregion

namespace Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Albums;

/// <summary>
/// Extension methods for converting <see cref="AlbumMetadataDto"/>.
/// </summary>
public static class AlbumMetadataDtoMapping
{
    /// <summary>
    /// Applies the metadata of <paramref name="dto"/> to <paramref name="album"/>, through the owning <paramref name="artist"/> aggregate root.
    /// </summary>
    /// <param name="dto">The album metadata to apply.</param>
    /// <param name="artist">The artist aggregate that owns the album.</param>
    /// <param name="album">The album entity to update.</param>
    /// <param name="contributors">The media contributors of the album, already resolved to their persisted identifiers.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either the successfully updated <see cref="Artist"/>, or an error message.
    /// </returns>
    public static Result<Updated> ApplyTo(this AlbumMetadataDto dto, Artist artist, Album album, IReadOnlyCollection<MusicMediaContributor> contributors)
    {
        Result<AlbumMetadata> metadataResult = dto.ToDomainValueObject(album.Metadata.ReleaseInfo);
        if (metadataResult.IsFailure)
            return metadataResult.Errors;

        List<AudioRating> ratings = [];
        foreach (AudioRatingDto rating in dto.Ratings ?? [])
        {
            Result<AudioRating> ratingResult = rating.ToDomainValueObject();
            if (ratingResult.IsFailure)
                return ratingResult.Errors;
            ratings.Add(ratingResult.Value);
        }

        Optional<Barcode> barcode = Optional<Barcode>.None();
        if (!string.IsNullOrWhiteSpace(dto.Barcode))
        {
            Result<Barcode> barcodeResult = Barcode.Create(dto.Barcode);
            if (barcodeResult.IsFailure)
                return barcodeResult.Errors;
            barcode = barcodeResult.Value;
        }

        Optional<MusicBrainzId> musicBrainzReleaseId = Optional<MusicBrainzId>.None();
        if (dto.MusicBrainzReleaseId is not null)
            musicBrainzReleaseId = MusicBrainzId.Create(dto.MusicBrainzReleaseId.Value);
        Optional<MusicBrainzId> musicBrainzReleaseGroupId = Optional<MusicBrainzId>.None();
        if (dto.MusicBrainzReleaseGroupId is not null)
            musicBrainzReleaseGroupId = MusicBrainzId.Create(dto.MusicBrainzReleaseGroupId.Value);
        Optional<MusicBrainzId> musicBrainzReleaseArtistId = Optional<MusicBrainzId>.None();
        if (dto.MusicBrainzReleaseArtistId is not null)
            musicBrainzReleaseArtistId = MusicBrainzId.Create(dto.MusicBrainzReleaseArtistId.Value);

        return artist.UpdateAlbum(
            album,
            metadataResult.Value,
            Optional<string>.FromNullable(dto.Disambiguation),
            Optional<MusicMediaFormat>.FromNullable(dto.MediaFormat),
            Optional<MusicReleasePackaging>.FromNullable(dto.Packaging),
            Optional<string>.FromNullable(dto.Script),
            barcode,
            dto.CatalogNumbers ?? [],
            Optional<string>.FromNullable(dto.Label),
            Optional<string>.FromNullable(dto.ASIN),
            musicBrainzReleaseId,
            musicBrainzReleaseGroupId,
            musicBrainzReleaseArtistId,
            contributors,
            ratings);
    }

    /// <summary>
    /// Converts <paramref name="dto"/> to a domain <see cref="AlbumMetadata"/>.
    /// </summary>
    /// <param name="dto">The data transfer object to be converted.</param>
    /// <param name="existingReleaseInfo">The release information already stored for the album, used as a fallback for the fields the metadata does not provide.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a successfully converted <see cref="AlbumMetadata"/>, or an error message.
    /// </returns>
    private static Result<AlbumMetadata> ToDomainValueObject(this AlbumMetadataDto dto, ReleaseInfo existingReleaseInfo)
    {
        ReleaseInfoDto? releaseInfoDto = dto.ReleaseInfo;

        // The enrichment must not erase a release date that the tags already provided, so a date that the metadata does not carry falls back to the stored one.
        DateOnly? originalReleaseDate = releaseInfoDto?.OriginalReleaseDate;
        int? originalReleaseYear = releaseInfoDto?.OriginalReleaseYear;
        if (originalReleaseDate is null && originalReleaseYear is null)
        {
            originalReleaseDate = existingReleaseInfo.OriginalReleaseDate.HasValue ? existingReleaseInfo.OriginalReleaseDate.Value : null;
            originalReleaseYear = existingReleaseInfo.OriginalReleaseYear.HasValue ? existingReleaseInfo.OriginalReleaseYear.Value : null;
        }

        DateOnly? reReleaseDate = releaseInfoDto?.ReReleaseDate;
        int? reReleaseYear = releaseInfoDto?.ReReleaseYear;
        if (reReleaseDate is null && reReleaseYear is null)
        {
            reReleaseDate = existingReleaseInfo.ReReleaseDate.HasValue ? existingReleaseInfo.ReReleaseDate.Value : null;
            reReleaseYear = existingReleaseInfo.ReReleaseYear.HasValue ? existingReleaseInfo.ReReleaseYear.Value : null;
        }

        ReleaseCountry? releaseCountry = releaseInfoDto?.ReleaseCountry ?? (existingReleaseInfo.ReleaseCountry.HasValue ? existingReleaseInfo.ReleaseCountry.Value : null);
        string? releaseVersion = releaseInfoDto?.ReleaseVersion ?? (existingReleaseInfo.ReleaseVersion.HasValue ? existingReleaseInfo.ReleaseVersion.Value : null);

        Result<ReleaseInfo> releaseInfoResult = ReleaseInfo.Create(
            Optional<DateOnly>.FromNullable(originalReleaseDate),
            Optional<int>.FromNullable(originalReleaseYear),
            Optional<DateOnly>.FromNullable(reReleaseDate),
            Optional<int>.FromNullable(reReleaseYear),
            Optional<ReleaseCountry>.FromNullable(releaseCountry),
            Optional<string>.FromNullable(releaseVersion));
        if (releaseInfoResult.IsFailure)
            return releaseInfoResult.Errors;

        List<Genre> genres = [];
        foreach (GenreDto genre in dto.Genres ?? [])
        {
            Result<Genre> genreResult = genre.ToDomainValueObject();
            if (genreResult.IsFailure)
                return genreResult.Errors;
            genres.Add(genreResult.Value);
        }

        List<Tag> tags = [];
        foreach (TagDto tag in dto.Tags ?? [])
        {
            Result<Tag> tagResult = tag.ToDomainValueObject();
            if (tagResult.IsFailure)
                return tagResult.Errors;
            tags.Add(tagResult.Value);
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
            genres,
            tags,
            language,
            originalLanguage,
            dto.ReleaseTypes is null ? [] : [.. dto.ReleaseTypes],
            Optional<MusicReleaseStatus>.FromNullable(dto.ReleaseStatus),
            Optional<int>.FromNullable(dto.TotalDiscs),
            dto.TotalTracks ?? 0);
    }
}
