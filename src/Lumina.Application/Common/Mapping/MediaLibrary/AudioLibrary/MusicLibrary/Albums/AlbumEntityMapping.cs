#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.Common.Metadata;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Contracts.DTO.Common;
using Lumina.Contracts.DTO.MediaContributors;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Albums;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Common.ValueObjects.Metadata;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.Common.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.ExternalIdentifiers.MediaContributorBoundedContext.MediaContributorAggregate;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.Entities;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Albums;
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Common;
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
#endregion

namespace Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Albums;

/// <summary>
/// Extension methods for converting <see cref="AlbumEntity"/>.
/// </summary>
public static class AlbumEntityMapping
{
    /// <summary>
    /// Converts <paramref name="repositoryEntity"/> to <see cref="Album"/>.
    /// </summary>
    /// <param name="repositoryEntity">The repository entity to be converted.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a successfully converted <see cref="Album"/>, or an error message.
    /// </returns>
    public static Result<Album> ToDomainEntity(this AlbumEntity repositoryEntity)
    {
        Result<ReleaseInfo> releaseInfoResult = ReleaseInfo.Create(
            Optional<DateOnly>.FromNullable(repositoryEntity.OriginalReleaseDate),
            Optional<int>.FromNullable(repositoryEntity.OriginalReleaseYear),
            Optional<DateOnly>.FromNullable(repositoryEntity.ReReleaseDate),
            Optional<int>.FromNullable(repositoryEntity.ReReleaseYear),
            Optional<ReleaseCountry>.FromNullable(repositoryEntity.ReleaseCountry),
            Optional<string>.FromNullable(repositoryEntity.ReleaseVersion));
        if (releaseInfoResult.IsFailure)
            return releaseInfoResult.Errors;

        List<Genre> domainGenres = [];
        foreach (Result<Genre> genreResult in repositoryEntity.Genres.ToDomainEntities())
        {
            if (genreResult.IsFailure)
                return genreResult.Errors;
            domainGenres.Add(genreResult.Value);
        }

        List<Tag> domainTags = [];
        foreach (Result<Tag> tagResult in repositoryEntity.Tags.ToDomainEntities())
        {
            if (tagResult.IsFailure)
                return tagResult.Errors;
            domainTags.Add(tagResult.Value);
        }

        Optional<LanguageInfo> language = Optional<LanguageInfo>.None();
        if (repositoryEntity.LanguageCode is not null && repositoryEntity.LanguageName is not null)
            language = LanguageInfo.Create(repositoryEntity.LanguageCode, repositoryEntity.LanguageName, Optional<string>.FromNullable(repositoryEntity.LanguageNativeName));

        Optional<LanguageInfo> originalLanguage = Optional<LanguageInfo>.None();
        if (repositoryEntity.OriginalLanguageCode is not null && repositoryEntity.OriginalLanguageName is not null)
            originalLanguage = LanguageInfo.Create(repositoryEntity.OriginalLanguageCode, repositoryEntity.OriginalLanguageName, Optional<string>.FromNullable(repositoryEntity.OriginalLanguageNativeName));

        Result<AlbumMetadata> metadataResult = AlbumMetadata.Create(
            repositoryEntity.Title,
            Optional<string>.FromNullable(repositoryEntity.OriginalTitle),
            Optional<string>.FromNullable(repositoryEntity.Description),
            releaseInfoResult.Value,
            domainGenres,
            domainTags,
            language,
            originalLanguage,
            Optional<MusicReleaseType>.FromNullable(repositoryEntity.ReleaseType),
            Optional<MusicReleaseStatus>.FromNullable(repositoryEntity.ReleaseStatus),
            Optional<int>.FromNullable(repositoryEntity.TotalDiscs),
            repositoryEntity.TotalTracks);
        if (metadataResult.IsFailure)
            return metadataResult.Errors;

        Optional<Barcode> barcode = Optional<Barcode>.None();
        if (repositoryEntity.Barcode is not null)
        {
            Result<Barcode> barcodeResult = Barcode.Create(repositoryEntity.Barcode);
            if (barcodeResult.IsFailure)
                return barcodeResult.Errors;
            barcode = barcodeResult.Value;
        }

        Optional<MusicBrainzId> musicBrainzReleaseId = Optional<MusicBrainzId>.None();
        if (repositoryEntity.MusicBrainzReleaseId is not null)
        {
            Result<MusicBrainzId> musicBrainzReleaseIdResult = MusicBrainzId.Create(repositoryEntity.MusicBrainzReleaseId.Value);
            if (musicBrainzReleaseIdResult.IsFailure)
                return musicBrainzReleaseIdResult.Errors;
            musicBrainzReleaseId = musicBrainzReleaseIdResult.Value;
        }
        Optional<MusicBrainzId> musicBrainzReleaseGroupId = Optional<MusicBrainzId>.None();
        if (repositoryEntity.MusicBrainzReleaseGroupId is not null)
        {
            Result<MusicBrainzId> musicBrainzReleaseGroupIdResult = MusicBrainzId.Create(repositoryEntity.MusicBrainzReleaseGroupId.Value);
            if (musicBrainzReleaseGroupIdResult.IsFailure)
                return musicBrainzReleaseGroupIdResult.Errors;
            musicBrainzReleaseGroupId = musicBrainzReleaseGroupIdResult.Value;
        }
        Optional<MusicBrainzId> musicBrainzReleaseArtistId = Optional<MusicBrainzId>.None();
        if (repositoryEntity.MusicBrainzReleaseArtistId is not null)
        {
            Result<MusicBrainzId> musicBrainzReleaseArtistIdResult = MusicBrainzId.Create(repositoryEntity.MusicBrainzReleaseArtistId.Value);
            if (musicBrainzReleaseArtistIdResult.IsFailure)
                return musicBrainzReleaseArtistIdResult.Errors;
            musicBrainzReleaseArtistId = musicBrainzReleaseArtistIdResult.Value;
        }

        List<AudioRating> domainRatings = [];
        foreach (Result<AudioRating> ratingResult in repositoryEntity.Ratings.ToDomainEntities())
        {
            if (ratingResult.IsFailure)
                return ratingResult.Errors;
            domainRatings.Add(ratingResult.Value);
        }

        List<MusicMediaContributor> domainContributors = [];
        foreach (AlbumContributorEntity contributorEntity in repositoryEntity.Contributors)
        {
            Result<MusicMediaContributor> contributorResult = MusicMediaContributor.Create(MediaContributorId.Create(contributorEntity.MediaContributorId), contributorEntity.Role);
            if (contributorResult.IsFailure)
                return contributorResult.Errors;
            domainContributors.Add(contributorResult.Value);
        }

        List<Track> domainTracks = [];
        foreach (TrackEntity trackEntity in repositoryEntity.Tracks)
        {
            Result<Track> trackResult = trackEntity.ToDomainEntity();
            if (trackResult.IsFailure)
                return trackResult.Errors;
            domainTracks.Add(trackResult.Value);
        }

        return Album.Create(
            AlbumId.Create(repositoryEntity.Id),
            metadataResult.Value,
            Optional<MusicMediaFormat>.FromNullable(repositoryEntity.MediaFormat),
            barcode,
            Optional<string>.FromNullable(repositoryEntity.CatalogNumber),
            musicBrainzReleaseId,
            musicBrainzReleaseGroupId,
            musicBrainzReleaseArtistId,
            domainContributors,
            domainRatings,
            domainTracks,
            repositoryEntity.CreatedOnUtc,
            Optional<DateTime>.FromNullable(repositoryEntity.UpdatedOnUtc));
    }

    /// <summary>
    /// Converts <paramref name="repositoryEntity"/> to <see cref="AlbumResponse"/>.
    /// </summary>
    /// <param name="repositoryEntity">The repository entity to be converted.</param>
    /// <returns>The converted response entity.</returns>
    public static AlbumResponse ToResponse(this AlbumEntity repositoryEntity)
    {
        ReleaseInfoDto releaseInfo = new(
            repositoryEntity.OriginalReleaseDate,
            repositoryEntity.OriginalReleaseYear,
            repositoryEntity.ReReleaseDate,
            repositoryEntity.ReReleaseYear,
            repositoryEntity.ReleaseCountry,
            repositoryEntity.ReleaseVersion
        );
        // language and original language make sense only if their subproperties have values
        LanguageInfoDto? languageInfo = !string.IsNullOrWhiteSpace(repositoryEntity.LanguageCode) ||
                                        !string.IsNullOrWhiteSpace(repositoryEntity.LanguageName) ||
                                        !string.IsNullOrWhiteSpace(repositoryEntity.LanguageNativeName)
            ? new LanguageInfoDto(
                repositoryEntity.LanguageCode,
                repositoryEntity.LanguageName,
                repositoryEntity.LanguageNativeName
            ) : null;
        LanguageInfoDto? originalLanguageInfo = !string.IsNullOrWhiteSpace(repositoryEntity.OriginalLanguageCode) ||
                                                !string.IsNullOrWhiteSpace(repositoryEntity.OriginalLanguageName) ||
                                                !string.IsNullOrWhiteSpace(repositoryEntity.OriginalLanguageNativeName)
            ? new LanguageInfoDto(
                repositoryEntity.OriginalLanguageCode,
                repositoryEntity.OriginalLanguageName,
                repositoryEntity.OriginalLanguageNativeName
            ) : null;
        AlbumMetadataDto metadata = new(
            repositoryEntity.Title,
            repositoryEntity.OriginalTitle,
            repositoryEntity.Description,
            releaseInfo,
            languageInfo,
            originalLanguageInfo,
            [.. repositoryEntity.Tags.ToResponses()],
            [.. repositoryEntity.Genres.ToResponses()],
            repositoryEntity.ReleaseType,
            repositoryEntity.ReleaseStatus,
            repositoryEntity.TotalDiscs,
            repositoryEntity.TotalTracks
        );
        return new AlbumResponse(
            repositoryEntity.Id,
            repositoryEntity.ArtistId,
            repositoryEntity.LibraryId,
            metadata,
            repositoryEntity.MediaFormat,
            repositoryEntity.Barcode,
            repositoryEntity.CatalogNumber,
            repositoryEntity.MusicBrainzReleaseId,
            repositoryEntity.MusicBrainzReleaseGroupId,
            repositoryEntity.MusicBrainzReleaseArtistId,
            repositoryEntity.CreatedOnUtc,
            repositoryEntity.UpdatedOnUtc,
            [.. repositoryEntity.Contributors.Select(contributor => new MediaContributorReferenceDto(contributor.MediaContributorId, contributor.Role))],
            [.. repositoryEntity.Ratings.ToResponses()],
            [.. repositoryEntity.Tracks.Select(track => track.ToResponse())]);
    }

    /// <summary>
    /// Converts <paramref name="repositoryEntities"/> to a collection of <see cref="AlbumResponse"/>.
    /// </summary>
    /// <param name="repositoryEntities">The repository entities to be converted.</param>
    /// <returns>The converted responses.</returns>
    public static IReadOnlyList<AlbumResponse> ToResponses(this IEnumerable<AlbumEntity> repositoryEntities)
    {
        return [.. repositoryEntities.Select(repositoryEntity => repositoryEntity.ToResponse())];
    }
}
