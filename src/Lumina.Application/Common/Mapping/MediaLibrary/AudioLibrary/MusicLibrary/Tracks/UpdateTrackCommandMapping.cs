#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Common;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Commands.UpdateTrack;
using Lumina.Contracts.DTO.Common;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Common.ValueObjects.Metadata;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.Common.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.Entities;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using System;
using System.Collections.Generic;
#endregion

namespace Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;

/// <summary>
/// Extension methods for converting <see cref="UpdateTrackCommand"/>.
/// </summary>
public static class UpdateTrackCommandMapping
{
    /// <summary>
    /// Converts <paramref name="command"/> to a domain <see cref="Track"/>, preserving the provided <paramref name="id"/>.
    /// </summary>
    /// <param name="command">The command whose data is used to create the track.</param>
    /// <param name="id">The unique identifier of the track.</param>
    /// <param name="libraryId">The Id of the media library the track belongs to.</param>
    /// <param name="ratings">The list of ratings of the track.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a successfully created <see cref="Track"/>, or an error message.
    /// </returns>
    public static Result<Track> ToDomainEntity(this UpdateTrackCommand command, TrackId id, Guid libraryId, List<AudioRating> ratings)
    {
        Result<AudioMetadata> metadataResult = command.Metadata!.ToDomainEntity();
        if (metadataResult.IsFailure)
            return metadataResult.Errors;

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

        return Track.Create(
            id,
            command.Path!,
            metadataResult.Value,
            command.TrackNumber ?? 1,
            Optional<int>.FromNullable(command.DiscNumber),
            domainIsrcs,
            Optional<string>.FromNullable(command.Script),
            Optional<MusicKey>.FromNullable(command.Key),
            Optional<int>.FromNullable(command.Bpm),
            domainMoods,
            Optional<string>.FromNullable(command.Work),
            musicBrainzRecordingId,
            musicBrainzTrackId,
            musicBrainzWorkId,
            [],
            ratings);
    }
}
