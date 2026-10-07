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

namespace Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;

/// <summary>
/// Extension methods for converting <see cref="AudioMetadataDto"/>.
/// </summary>
public static class AudioMetadataDtoMapping
{
    /// <summary>
    /// Applies the metadata of <paramref name="dto"/> to <paramref name="track"/>, through the owning <paramref name="artist"/> aggregate root.
    /// </summary>
    /// <param name="dto">The audio metadata to apply.</param>
    /// <param name="artist">The artist aggregate that owns the album of the track.</param>
    /// <param name="album">The album the track belongs to.</param>
    /// <param name="track">The track entity to update.</param>
    /// <param name="contributors">The media contributors of the track, already resolved to their persisted identifiers.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either the successfully updated <see cref="Artist"/>, or an error message.
    /// </returns>
    public static Result<Updated> ApplyTo(this AudioMetadataDto dto, Artist artist, Album album, Track track, IReadOnlyCollection<MusicMediaContributor> contributors)
    {
        Result<AudioMetadata> metadataResult = dto.ToDomainValueObject(track);
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

        // The work, the moods and the ISRCs of the track are only replaced when the provider returned values, so locally extracted data is never cleared.
        Optional<MusicWork> work = track.Work;
        if (dto.Work is not null)
        {
            Result<MusicWork> workResult = dto.Work.ToDomainValueObject();
            if (workResult.IsFailure)
                return workResult.Errors;
            work = workResult.Value;
        }

        List<Mood> moods = [.. track.Moods];
        if (dto.Moods is { Count: > 0 })
        {
            moods = [];
            foreach (MoodDto mood in dto.Moods)
            {
                Result<Mood> moodResult = mood.ToDomainValueObject();
                if (moodResult.IsFailure)
                    return moodResult.Errors;
                moods.Add(moodResult.Value);
            }
        }

        List<Isrc> isrcs = [.. track.Isrcs];
        if (dto.Isrcs is { Count: > 0 })
        {
            isrcs = [];
            foreach (IsrcDto isrc in dto.Isrcs)
            {
                Result<Isrc> isrcResult = isrc.ToDomainValueObject();
                if (isrcResult.IsFailure)
                    return isrcResult.Errors;
                isrcs.Add(isrcResult.Value);
            }
        }

        Optional<string> script = Optional<string>.FromNullable(dto.Script);
        if (!script.HasValue)
            script = track.Script;
        Optional<MusicKey> key = Optional<MusicKey>.FromNullable(dto.Key);
        if (!key.HasValue)
            key = track.Key;
        Optional<int> bpm = Optional<int>.FromNullable(dto.Bpm);
        if (!bpm.HasValue)
            bpm = track.Bpm;
        Optional<MusicBrainzId> musicBrainzRecordingId = Optional<MusicBrainzId>.None();
        if (dto.MusicBrainzRecordingId is not null)
            musicBrainzRecordingId = MusicBrainzId.Create(dto.MusicBrainzRecordingId.Value);
        else
            musicBrainzRecordingId = track.MusicBrainzRecordingId;
        Optional<MusicBrainzId> musicBrainzTrackId = Optional<MusicBrainzId>.None();
        if (dto.MusicBrainzTrackId is not null)
            musicBrainzTrackId = MusicBrainzId.Create(dto.MusicBrainzTrackId.Value);
        else
            musicBrainzTrackId = track.MusicBrainzTrackId;

        return artist.UpdateTrackInAlbum(
            album,
            track,
            track.Path,
            metadataResult.Value,
            track.TrackNumber,
            track.DiscNumber,
            script,
            key,
            bpm,
            dto.IsVideo,
            work,
            musicBrainzRecordingId,
            musicBrainzTrackId,
            moods,
            isrcs,
            contributors,
            ratings);
    }

    /// <summary>
    /// Converts <paramref name="dto"/> to a domain <see cref="AudioMetadata"/>, keeping the locally extracted audio properties that the provider did not return.
    /// </summary>
    /// <param name="dto">The data transfer object to be converted.</param>
    /// <param name="track">The existing track whose locally extracted properties are preserved for the fields the provider left empty.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a successfully converted <see cref="AudioMetadata"/>, or an error message.
    /// </returns>
    private static Result<AudioMetadata> ToDomainValueObject(this AudioMetadataDto dto, Track track)
    {
        Result<ReleaseInfo> releaseInfoResult = dto.ReleaseInfo is null
            ? ReleaseInfo.Create(
                track.Metadata.ReleaseInfo.OriginalReleaseDate,
                track.Metadata.ReleaseInfo.OriginalReleaseYear,
                track.Metadata.ReleaseInfo.ReReleaseDate,
                track.Metadata.ReleaseInfo.ReReleaseYear,
                track.Metadata.ReleaseInfo.ReleaseCountry,
                track.Metadata.ReleaseInfo.ReleaseVersion)
            : ReleaseInfo.Create(
                Optional<DateOnly>.FromNullable(dto.ReleaseInfo.OriginalReleaseDate),
                Optional<int>.FromNullable(dto.ReleaseInfo.OriginalReleaseYear),
                Optional<DateOnly>.FromNullable(dto.ReleaseInfo.ReReleaseDate),
                Optional<int>.FromNullable(dto.ReleaseInfo.ReReleaseYear),
                Optional<ReleaseCountry>.FromNullable(dto.ReleaseInfo.ReleaseCountry),
                Optional<string>.FromNullable(dto.ReleaseInfo.ReleaseVersion));
        if (releaseInfoResult.IsFailure)
            return releaseInfoResult.Errors;

        List<Genre> genres = [.. track.Metadata.Genres];
        if (dto.Genres is { Count: > 0 })
        {
            genres = [];
            foreach (GenreDto genre in dto.Genres)
            {
                Result<Genre> genreResult = genre.ToDomainValueObject();
                if (genreResult.IsFailure)
                    return genreResult.Errors;
                genres.Add(genreResult.Value);
            }
        }

        List<Tag> tags = [.. track.Metadata.Tags];
        if (dto.Tags is { Count: > 0 })
        {
            tags = [];
            foreach (TagDto tag in dto.Tags)
            {
                Result<Tag> tagResult = tag.ToDomainValueObject();
                if (tagResult.IsFailure)
                    return tagResult.Errors;
                tags.Add(tagResult.Value);
            }
        }

        Optional<LanguageInfo> language = track.Metadata.Language;
        if (dto.Language is not null && dto.Language.LanguageCode is not null && dto.Language.LanguageName is not null)
            language = LanguageInfo.Create(dto.Language.LanguageCode, dto.Language.LanguageName, Optional<string>.FromNullable(dto.Language.NativeName));

        Optional<LanguageInfo> originalLanguage = track.Metadata.OriginalLanguage;
        if (dto.OriginalLanguage is not null && dto.OriginalLanguage.LanguageCode is not null && dto.OriginalLanguage.LanguageName is not null)
            originalLanguage = LanguageInfo.Create(dto.OriginalLanguage.LanguageCode, dto.OriginalLanguage.LanguageName, Optional<string>.FromNullable(dto.OriginalLanguage.NativeName));

        return AudioMetadata.Create(
            string.IsNullOrWhiteSpace(dto.Title) ? track.Metadata.Title : dto.Title,
            Optional<string>.FromNullable(dto.OriginalTitle).HasValue ? Optional<string>.FromNullable(dto.OriginalTitle) : track.Metadata.OriginalTitle,
            dto.DurationInSeconds is > 0 ? dto.DurationInSeconds.Value : track.Metadata.DurationInSeconds,
            dto.SampleRate is > 0 ? dto.SampleRate.Value : track.Metadata.SampleRate,
            dto.Channels is > 0 ? dto.Channels.Value : track.Metadata.Channels,
            releaseInfoResult.Value,
            Optional<string>.FromNullable(dto.Description).HasValue ? Optional<string>.FromNullable(dto.Description) : track.Metadata.Description,
            genres,
            tags,
            language,
            originalLanguage,
            Optional<int>.FromNullable(dto.BitDepth).HasValue ? Optional<int>.FromNullable(dto.BitDepth) : track.Metadata.BitDepth,
            Optional<string>.FromNullable(dto.AudioCodec).HasValue ? Optional<string>.FromNullable(dto.AudioCodec) : track.Metadata.AudioCodec,
            Optional<int>.FromNullable(dto.Bitrate).HasValue ? Optional<int>.FromNullable(dto.Bitrate) : track.Metadata.Bitrate,
            track.Metadata.AcoustId,
            track.Metadata.ReplayGainTrackGain,
            track.Metadata.ReplayGainTrackPeak,
            track.Metadata.ReplayGainAlbumGain,
            track.Metadata.ReplayGainAlbumPeak);
    }
}
