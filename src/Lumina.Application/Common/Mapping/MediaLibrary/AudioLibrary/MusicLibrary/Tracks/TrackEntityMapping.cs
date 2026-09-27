#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.Common.Metadata;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Contracts.DTO.Common;
using Lumina.Contracts.DTO.MediaContributors;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Common.ValueObjects.Metadata;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.Common.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.ExternalIdentifiers.LibraryManagementBoundedContext.LibraryAggregate;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.ExternalIdentifiers.MediaContributorBoundedContext.MediaContributorAggregate;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.Entities;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Common;
#endregion

namespace Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;

/// <summary>
/// Extension methods for converting <see cref="TrackEntity"/>.
/// </summary>
public static class TrackEntityMapping
{
    /// <summary>
    /// Converts <paramref name="repositoryEntity"/> to <see cref="Track"/>.
    /// </summary>
    /// <param name="repositoryEntity">The repository entity to be converted.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a successfully converted <see cref="Track"/>, or an error message.
    /// </returns>
    public static Result<Track> ToDomainEntity(this TrackEntity repositoryEntity)
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

        Result<AudioMetadata> metadataResult = AudioMetadata.Create(
            repositoryEntity.Title,
            Optional<string>.FromNullable(repositoryEntity.OriginalTitle),
            repositoryEntity.DurationInSeconds,
            repositoryEntity.SampleRate,
            repositoryEntity.Channels,
            releaseInfoResult.Value,
            Optional<string>.FromNullable(repositoryEntity.Description),
            domainGenres,
            domainTags,
            language,
            originalLanguage,
            Optional<int>.FromNullable(repositoryEntity.BitDepth),
            Optional<string>.FromNullable(repositoryEntity.AudioCodec),
            Optional<int>.FromNullable(repositoryEntity.Bitrate));
        if (metadataResult.IsFailure)
            return metadataResult.Errors;

        Optional<MusicBrainzId> musicBrainzRecordingId = Optional<MusicBrainzId>.None();
        if (repositoryEntity.MusicBrainzRecordingId is not null)
            musicBrainzRecordingId = MusicBrainzId.Create(repositoryEntity.MusicBrainzRecordingId.Value);
        Optional<MusicBrainzId> musicBrainzTrackId = Optional<MusicBrainzId>.None();
        if (repositoryEntity.MusicBrainzTrackId is not null)
            musicBrainzTrackId = MusicBrainzId.Create(repositoryEntity.MusicBrainzTrackId.Value);
        Optional<MusicBrainzId> musicBrainzWorkId = Optional<MusicBrainzId>.None();
        if (repositoryEntity.MusicBrainzWorkId is not null)
            musicBrainzWorkId = MusicBrainzId.Create(repositoryEntity.MusicBrainzWorkId.Value);

        List<Mood> domainMoods = [];
        foreach (Result<Mood> moodResult in repositoryEntity.Moods.ToDomainEntities())
        {
            if (moodResult.IsFailure)
                return moodResult.Errors;
            domainMoods.Add(moodResult.Value);
        }

        List<Isrc> domainIsrcs = [];
        foreach (Result<Isrc> isrcResult in repositoryEntity.Isrcs.ToDomainEntities())
        {
            if (isrcResult.IsFailure)
                return isrcResult.Errors;
            domainIsrcs.Add(isrcResult.Value);
        }

        List<AudioRating> domainRatings = [];
        foreach (Result<AudioRating> ratingResult in repositoryEntity.Ratings.ToDomainEntities())
        {
            if (ratingResult.IsFailure)
                return ratingResult.Errors;
            domainRatings.Add(ratingResult.Value);
        }

        List<MusicMediaContributor> domainContributors = [];
        foreach (TrackContributorEntity contributorEntity in repositoryEntity.Contributors)
        {
            Result<MusicMediaContributor> contributorResult = MusicMediaContributor.Create(MediaContributorId.Create(contributorEntity.MediaContributorId), contributorEntity.Role);
            if (contributorResult.IsFailure)
                return contributorResult.Errors;
            domainContributors.Add(contributorResult.Value);
        }

        return Track.Create(
            TrackId.Create(repositoryEntity.Id),
            repositoryEntity.Path,
            metadataResult.Value,
            repositoryEntity.TrackNumber,
            Optional<int>.FromNullable(repositoryEntity.DiscNumber),
            domainMoods,
            Optional<string>.FromNullable(repositoryEntity.Script),
            Optional<MusicKey>.FromNullable(repositoryEntity.Key),
            Optional<int>.FromNullable(repositoryEntity.Bpm),
            domainIsrcs,
            Optional<string>.FromNullable(repositoryEntity.Work),
            musicBrainzRecordingId,
            musicBrainzTrackId,
            musicBrainzWorkId,
            domainContributors,
            domainRatings,
            repositoryEntity.CreatedOnUtc,
            Optional<DateTime>.FromNullable(repositoryEntity.UpdatedOnUtc));
    }

    /// <summary>
    /// Converts <paramref name="repositoryEntity"/> to <see cref="TrackResponse"/>.
    /// </summary>
    /// <param name="repositoryEntity">The repository entity to be converted.</param>
    /// <returns>The converted response entity.</returns>
    public static TrackResponse ToResponse(this TrackEntity repositoryEntity)
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
        AudioMetadataDto metadata = new(
            repositoryEntity.Title,
            repositoryEntity.OriginalTitle,
            repositoryEntity.Description,
            releaseInfo,
            languageInfo,
            originalLanguageInfo,
            [.. repositoryEntity.Tags.ToResponses()],
            [.. repositoryEntity.Genres.ToResponses()],
            repositoryEntity.DurationInSeconds,
            repositoryEntity.SampleRate,
            repositoryEntity.Channels,
            repositoryEntity.BitDepth,
            repositoryEntity.AudioCodec,
            repositoryEntity.Bitrate
        );
        return new TrackResponse(
            repositoryEntity.Id,
            repositoryEntity.AlbumId,
            repositoryEntity.LibraryId,
            repositoryEntity.Path,
            metadata,
            repositoryEntity.TrackNumber,
            repositoryEntity.DiscNumber,
            repositoryEntity.Script,
            repositoryEntity.Key,
            repositoryEntity.Bpm,
            repositoryEntity.Work,
            repositoryEntity.MusicBrainzRecordingId,
            repositoryEntity.MusicBrainzTrackId,
            repositoryEntity.MusicBrainzWorkId,
            repositoryEntity.CreatedOnUtc,
            repositoryEntity.UpdatedOnUtc,
            [.. repositoryEntity.Moods.ToResponses()],
            [.. repositoryEntity.Isrcs.ToResponses()],
            [.. repositoryEntity.Contributors.Select(contributor => new MediaContributorReferenceDto(contributor.MediaContributorId, contributor.Role))],
            [.. repositoryEntity.Ratings.ToResponses()]);
    }

    /// <summary>
    /// Converts <paramref name="repositoryEntities"/> to a collection of <see cref="TrackResponse"/>.
    /// </summary>
    /// <param name="repositoryEntities">The repository entities to be converted.</param>
    /// <returns>The converted responses.</returns>
    public static IReadOnlyList<TrackResponse> ToResponses(this IEnumerable<TrackEntity> repositoryEntities)
    {
        return [.. repositoryEntities.Select(repositoryEntity => repositoryEntity.ToResponse())];
    }
}
