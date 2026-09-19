#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.MediaContributors;
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Common;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Commands.UpdateTrack;
using Lumina.Contracts.DTO.Common;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Common.ValueObjects.Metadata;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.Common.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.Entities;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
#endregion

namespace Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;

/// <summary>
/// Extension methods for converting <see cref="UpdateTrackCommand"/>.
/// </summary>
public static class UpdateTrackCommandMapping
{
    /// <summary>
    /// Applies the editable data of <paramref name="command"/> to the track of <paramref name="artist"/> identified by the command, through the aggregate root.
    /// </summary>
    /// <param name="command">The command whose data is applied to the track.</param>
    /// <param name="artist">The artist aggregate that owns the track.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either the successfully updated <see cref="Artist"/>, or an error message.
    /// </returns>
    public static Result<Artist> ToDomainEntity(this UpdateTrackCommand command, Artist artist)
    {
        Result<AudioMetadata> metadataResult = command.Metadata!.ToDomainEntity();
        if (metadataResult.IsFailure)
            return metadataResult.Errors;

        IEnumerable<Result<MusicMediaContributor>> domainContributorsResult = command.Contributors!.ToMusicDomainEntities();
        List<Error> errors = [.. domainContributorsResult.Where(contributorResult => contributorResult.IsFailure).SelectMany(contributorResult => contributorResult.Errors)];
        if (errors.Count > 0)
            return errors;

        IEnumerable<Result<AudioRating>> domainRatingsResult = command.Ratings!.ToDomainEntities();
        errors = [.. domainRatingsResult.Where(ratingResult => ratingResult.IsFailure).SelectMany(ratingResult => ratingResult.Errors)];
        if (errors.Count > 0)
            return errors;

        Optional<MusicBrainzId> musicBrainzRecordingId = Optional<MusicBrainzId>.None();
        if (command.MusicBrainzRecordingId is not null)
        {
            Result<MusicBrainzId> musicBrainzRecordingIdResult = MusicBrainzId.Create(command.MusicBrainzRecordingId.Value);
            if (musicBrainzRecordingIdResult.IsFailure)
                return musicBrainzRecordingIdResult.Errors;
            musicBrainzRecordingId = musicBrainzRecordingIdResult.Value;
        }
        Optional<MusicBrainzId> musicBrainzTrackId = Optional<MusicBrainzId>.None();
        if (command.MusicBrainzTrackId is not null)
        {
            Result<MusicBrainzId> musicBrainzTrackIdResult = MusicBrainzId.Create(command.MusicBrainzTrackId.Value);
            if (musicBrainzTrackIdResult.IsFailure)
                return musicBrainzTrackIdResult.Errors;
            musicBrainzTrackId = musicBrainzTrackIdResult.Value;
        }
        Optional<MusicBrainzId> musicBrainzWorkId = Optional<MusicBrainzId>.None();
        if (command.MusicBrainzWorkId is not null)
        {
            Result<MusicBrainzId> musicBrainzWorkIdResult = MusicBrainzId.Create(command.MusicBrainzWorkId.Value);
            if (musicBrainzWorkIdResult.IsFailure)
                return musicBrainzWorkIdResult.Errors;
            musicBrainzWorkId = musicBrainzWorkIdResult.Value;
        }

        List<Isrc> domainIsrcs = [];
        foreach (IsrcDto isrc in command.Isrcs ?? [])
        {
            Result<Isrc> isrcResult = isrc.ToDomainEntity();
            if (isrcResult.IsFailure)
                return isrcResult.Errors;
            domainIsrcs.Add(isrcResult.Value);
        }

        List<Mood> domainMoods = [];
        foreach (MoodDto mood in command.Moods ?? [])
        {
            Result<Mood> moodResult = mood.ToDomainEntity();
            if (moodResult.IsFailure)
                return moodResult.Errors;
            domainMoods.Add(moodResult.Value);
        }

        Result<Updated> updateResult = artist.UpdateTrackInAlbum(
            AlbumId.Create(Guid.Parse(command.AlbumId!)),
            TrackId.Create(Guid.Parse(command.TrackId!)),
            command.Path!,
            metadataResult.Value,
            command.TrackNumber ?? 1,
            Optional<int>.FromNullable(command.DiscNumber),
            Optional<string>.FromNullable(command.Script),
            Optional<MusicKey>.FromNullable(command.Key),
            Optional<int>.FromNullable(command.Bpm),
            Optional<string>.FromNullable(command.Work),
            musicBrainzRecordingId,
            musicBrainzTrackId,
            musicBrainzWorkId,
            [.. domainContributorsResult.Select(contributorResult => contributorResult.Value)],
            [.. domainRatingsResult.Select(ratingResult => ratingResult.Value)],
            domainMoods,
            domainIsrcs);
        if (updateResult.IsFailure)
            return updateResult.Errors;

        return artist;
    }
}
