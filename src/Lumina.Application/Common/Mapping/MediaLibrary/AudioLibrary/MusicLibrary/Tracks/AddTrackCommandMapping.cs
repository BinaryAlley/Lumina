#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.MediaContributors;
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Common;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Commands.AddTrack;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Common.ValueObjects.Metadata;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.Common.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.Entities;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using System.Collections.Generic;
using System.Linq;
#endregion

namespace Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;

/// <summary>
/// Extension methods for converting <see cref="AddTrackCommand"/>.
/// </summary>
public static class AddTrackCommandMapping
{
    /// <summary>
    /// Converts <paramref name="command"/> to a domain <see cref="Track"/>.
    /// </summary>
    /// <param name="command">The command whose data is used to create the track.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a successfully created <see cref="Track"/>, or an error message.
    /// </returns>
    public static Result<Track> ToDomainEntity(this AddTrackCommand command)
    {
        Result<AudioMetadata> metadataResult = command.Metadata!.ToDomainEntity();
        if (metadataResult.IsFailure)
            return metadataResult.Errors;

        IEnumerable<Result<Isrc>> domainIsrcsResult = (command.Isrcs ?? []).ToDomainEntities();
        List<Error> errors = [.. domainIsrcsResult.Where(isrcResult => isrcResult.IsFailure).SelectMany(isrcResult => isrcResult.Errors)];
        if (errors.Count > 0)
            return errors;

        IEnumerable<Result<Mood>> domainMoodsResult = (command.Moods ?? []).ToDomainEntities();
        errors = [.. domainMoodsResult.Where(moodResult => moodResult.IsFailure).SelectMany(moodResult => moodResult.Errors)];
        if (errors.Count > 0)
            return errors;

        IEnumerable<Result<MusicMediaContributor>> domainContributorsResult = (command.Contributors ?? []).ToMusicDomainEntities();
        errors = [.. domainContributorsResult.Where(contributorResult => contributorResult.IsFailure).SelectMany(contributorResult => contributorResult.Errors)];
        if (errors.Count > 0)
            return errors;

        IEnumerable<Result<AudioRating>> domainRatingsResult = (command.Ratings ?? []).ToDomainEntities();
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

        return Track.Create(
            command.Path!,
            metadataResult.Value,
            command.TrackNumber ?? 1,
            Optional<int>.FromNullable(command.DiscNumber),
            [.. domainIsrcsResult.Select(isrcResult => isrcResult.Value)],
            Optional<string>.FromNullable(command.Script),
            Optional<MusicKey>.FromNullable(command.Key),
            Optional<int>.FromNullable(command.Bpm),
            [.. domainMoodsResult.Select(moodResult => moodResult.Value)],
            Optional<string>.FromNullable(command.Work),
            musicBrainzRecordingId,
            musicBrainzTrackId,
            musicBrainzWorkId,
            [.. domainContributorsResult.Select(contributorResult => contributorResult.Value)],
            [.. domainRatingsResult.Select(ratingResult => ratingResult.Value)]);
    }

    /// <summary>
    /// Converts <paramref name="commands"/> to a collection of domain <see cref="Track"/>.
    /// </summary>
    /// <param name="commands">The commands whose data is used to create the tracks.</param>
    /// <returns>A collection of <see cref="Result{TValue}"/> containing either the converted tracks, or error messages.</returns>
    public static IEnumerable<Result<Track>> ToDomainEntities(this IEnumerable<AddTrackCommand> commands)
    {
        return commands.Select(command => command.ToDomainEntity());
    }
}
