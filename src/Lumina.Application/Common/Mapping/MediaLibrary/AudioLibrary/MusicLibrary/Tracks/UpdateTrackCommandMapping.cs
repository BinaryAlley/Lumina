#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.MediaContributors;
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Common;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Commands.UpdateTrack;
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
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
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
        MusicTrackMetadataDto metadata = command.Metadata!;
        Result<AudioMetadata> metadataResult = metadata.ToDomainValueObject();
        if (metadataResult.IsFailure)
            return metadataResult.Errors;

        IEnumerable<Result<Mood>> domainMoodsResult = (command.Moods ?? []).ToDomainValueObjects();
        List<Error> errors = [.. domainMoodsResult.Where(moodResult => moodResult.IsFailure).SelectMany(moodResult => moodResult.Errors)];
        if (errors.Count > 0)
            return errors;

        IEnumerable<Result<Isrc>> domainIsrcsResult = (command.Isrcs ?? []).ToDomainValueObjects();
        errors = [.. domainIsrcsResult.Where(isrcResult => isrcResult.IsFailure).SelectMany(isrcResult => isrcResult.Errors)];
        if (errors.Count > 0)
            return errors;

        IEnumerable<Result<MusicMediaContributor>> domainContributorsResult = (command.Contributors ?? []).ToMusicDomainEntities();
        errors = [.. domainContributorsResult.Where(contributorResult => contributorResult.IsFailure).SelectMany(contributorResult => contributorResult.Errors)];
        if (errors.Count > 0)
            return errors;

        IEnumerable<Result<AudioRating>> domainRatingsResult = (command.Ratings ?? []).ToDomainValueObjects();
        errors = [.. domainRatingsResult.Where(ratingResult => ratingResult.IsFailure).SelectMany(ratingResult => ratingResult.Errors)];
        if (errors.Count > 0)
            return errors;

        Optional<MusicWork> work = Optional<MusicWork>.None();
        if (command.Work is not null)
        {
            Result<MusicWork> workResult = command.Work.ToDomainValueObject();
            if (workResult.IsFailure)
                return workResult.Errors;
            work = workResult.Value;
        }

        Optional<MusicBrainzId> musicBrainzRecordingId = Optional<MusicBrainzId>.None();
        if (command.MusicBrainzRecordingId is not null)
            musicBrainzRecordingId = MusicBrainzId.Create(command.MusicBrainzRecordingId.Value);
        Optional<MusicBrainzId> musicBrainzTrackId = Optional<MusicBrainzId>.None();
        if (command.MusicBrainzTrackId is not null)
            musicBrainzTrackId = MusicBrainzId.Create(command.MusicBrainzTrackId.Value);

        // The album and the track are entities inside the artist aggregate, so they are referenced by object, not by id; the aggregate members are located here and passed through.
        Album? album = artist.Albums.FirstOrDefault(album => album.Id.Value == Guid.Parse(command.AlbumId!));
        if (album is null)
            return DomainErrors.Music.AlbumNotFound;
        Track? track = album.Tracks.FirstOrDefault(track => track.Id.Value == Guid.Parse(command.TrackId!));
        if (track is null)
            return DomainErrors.Music.TrackNotFound;

        Result<Updated> updateResult = artist.UpdateTrackInAlbum(
            album,
            track,
            command.Path!,
            metadataResult.Value,
            command.TrackNumber!.Value,
            Optional<int>.FromNullable(command.DiscNumber),
            Optional<string>.FromNullable(command.Script),
            Optional<MusicKey>.FromNullable(command.Key),
            Optional<int>.FromNullable(command.Bpm),
            metadata.IsVideo,
            work,
            musicBrainzRecordingId,
            musicBrainzTrackId,
            [.. domainMoodsResult.Select(moodResult => moodResult.Value)],
            [.. domainIsrcsResult.Select(isrcResult => isrcResult.Value)],
            [.. domainContributorsResult.Select(contributorResult => contributorResult.Value)],
            [.. domainRatingsResult.Select(ratingResult => ratingResult.Value)]);
        if (updateResult.IsFailure)
            return updateResult.Errors;

        return artist;
    }
}
