#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Models.Core;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.MusicPlayHistoryBoundedContext.ExternalIdentifiers.AudioLibraryBoundedContext.MusicLibraryAggregate;
using Lumina.Domain.Core.BoundedContexts.MusicPlayHistoryBoundedContext.ExternalIdentifiers.LibraryManagementBoundedContext.LibraryAggregate;
using Lumina.Domain.Core.BoundedContexts.MusicPlayHistoryBoundedContext.ExternalIdentifiers.UserManagementBoundedContext.UserAggregate;
using Lumina.Domain.Core.BoundedContexts.MusicPlayHistoryBoundedContext.MusicPlayRecordAggregate.Events;
using Lumina.Domain.Core.BoundedContexts.MusicPlayHistoryBoundedContext.MusicPlayRecordAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.Playback;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.MusicPlayHistoryBoundedContext.MusicPlayRecordAggregate;

/// <summary>
/// Aggregate root for a recorded music play.
/// </summary>
/// <remarks>
/// A record is created when a player starts a track and is updated by the progress, completion and stop reports of that player. It logs what was
/// played, how far it reached and how long the user listened, so statistics and recommendations can be built from it. The record is created when the
/// play starts, so <see cref="CreatedOnUtc"/> is the start time.
/// </remarks>
[DebuggerDisplay("Id: {Id.Value}; TrackId: {TrackId.Value}; Status: {Status}")]
public sealed class MusicPlayRecord : AggregateRoot<MusicPlayRecordId>
{
    /// <summary>
    /// The minimum number of seconds a user must listen to a track for the play to count, unless the play completed first.
    /// </summary>
    private const int MINIMUM_COUNTED_LISTENED_DURATION_SECONDS = 30;

    /// <summary>
    /// Gets the object representing the unique identifier of the user that started the play.
    /// </summary>
    public UserId UserId { get; private set; }

    /// <summary>
    /// Gets the Id of the track that was played.
    /// </summary>
    public TrackId TrackId { get; private set; }

    /// <summary>
    /// Gets the Id of the media library that contains the track.
    /// </summary>
    public LibraryId LibraryId { get; private set; }

    /// <summary>
    /// Gets the Id of the album that contains the track.
    /// </summary>
    public AlbumId AlbumId { get; private set; }

    /// <summary>
    /// Gets the Id of the artist that produced the track.
    /// </summary>
    public ArtistId ArtistId { get; private set; }

    /// <summary>
    /// Gets the state of the recorded play.
    /// </summary>
    public PlaybackRecordStatus Status { get; private set; }

    /// <summary>
    /// Gets the last position reported by the player.
    /// </summary>
    public MusicPlayPosition LastPosition { get; private set; }

    /// <summary>
    /// Gets the duration that the user has listened to.
    /// </summary>
    public TimeSpan ListenedDuration { get; private set; }

    /// <summary>
    /// Gets the date and time when the play completed or stopped, if it has ended.
    /// </summary>
    public Optional<DateTime> EndedOnUtc { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MusicPlayRecord"/> class.
    /// </summary>
    /// <param name="id">The object representing the unique identifier of the play record.</param>
    /// <param name="userId">The object representing the unique identifier of the user that started the play.</param>
    /// <param name="trackId">The Id of the track that was played.</param>
    /// <param name="libraryId">The Id of the media library that contains the track.</param>
    /// <param name="albumId">The Id of the album that contains the track.</param>
    /// <param name="artistId">The Id of the artist that produced the track.</param>
    /// <param name="status">The state of the recorded play.</param>
    /// <param name="lastPosition">The last position reported by the player.</param>
    /// <param name="listenedDuration">The duration that the user has listened to.</param>
    /// <param name="endedOnUtc">The date and time when the play completed or stopped, if it has ended.</param>
    /// <param name="createdOnUtc">The date and time when the play started.</param>
    /// <param name="updatedOnUtc">The date and time when the play record was last updated.</param>
    private MusicPlayRecord(
        MusicPlayRecordId id,
        UserId userId,
        TrackId trackId,
        LibraryId libraryId,
        AlbumId albumId,
        ArtistId artistId,
        PlaybackRecordStatus status,
        MusicPlayPosition lastPosition,
        TimeSpan listenedDuration,
        Optional<DateTime> endedOnUtc,
        DateTime createdOnUtc,
        Optional<DateTime> updatedOnUtc) : base(id)
    {
        Id = id;
        UserId = userId;
        TrackId = trackId;
        LibraryId = libraryId;
        AlbumId = albumId;
        ArtistId = artistId;
        Status = status;
        LastPosition = lastPosition;
        ListenedDuration = listenedDuration;
        EndedOnUtc = endedOnUtc;
        CreatedOnUtc = createdOnUtc;
        UpdatedOnUtc = updatedOnUtc;
    }

    /// <summary>
    /// Creates a new play record for a track that started playing.
    /// </summary>
    /// <param name="userId">The object representing the unique identifier of the user that started the play.</param>
    /// <param name="trackId">The Id of the track that was played.</param>
    /// <param name="libraryId">The Id of the media library that contains the track.</param>
    /// <param name="albumId">The Id of the album that contains the track.</param>
    /// <param name="artistId">The Id of the artist that produced the track.</param>
    /// <returns>The created <see cref="MusicPlayRecord"/> instance.</returns>
    public static MusicPlayRecord Create(
        UserId userId,
        TrackId trackId,
        LibraryId libraryId,
        AlbumId albumId,
        ArtistId artistId)
    {
        DateTime occurredOnUtc = DateTime.UtcNow;
        MusicPlayRecord playRecord = new(
            MusicPlayRecordId.CreateUnique(),
            userId,
            trackId,
            libraryId,
            albumId,
            artistId,
            PlaybackRecordStatus.InProgress,
            MusicPlayPosition.Zero,
            TimeSpan.Zero,
            Optional<DateTime>.None(),
            occurredOnUtc,
            Optional<DateTime>.None());

        playRecord._domainEvents.Add(new MusicPlayStartedDomainEvent(
            Guid.NewGuid(),
            playRecord.Id,
            userId,
            trackId,
            libraryId,
            albumId,
            artistId,
            occurredOnUtc));
        return playRecord;
    }

    /// <summary>
    /// Creates a new instance of the <see cref="MusicPlayRecord"/> class, with pre-existing data.
    /// </summary>
    /// <param name="id">The object representing the unique identifier of the play record.</param>
    /// <param name="userId">The object representing the unique identifier of the user that started the play.</param>
    /// <param name="trackId">The Id of the track that was played.</param>
    /// <param name="libraryId">The Id of the media library that contains the track.</param>
    /// <param name="albumId">The Id of the album that contains the track.</param>
    /// <param name="artistId">The Id of the artist that produced the track.</param>
    /// <param name="status">The state of the recorded play.</param>
    /// <param name="lastPosition">The last position reported by the player.</param>
    /// <param name="listenedDuration">The duration that the user has listened to.</param>
    /// <param name="endedOnUtc">The date and time when the play completed or stopped, if it has ended.</param>
    /// <param name="createdOnUtc">The date and time when the play started.</param>
    /// <param name="updatedOnUtc">The date and time when the play record was last updated.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a successfully created <see cref="MusicPlayRecord"/>, or an error message.
    /// </returns>
    public static Result<MusicPlayRecord> Create(
        MusicPlayRecordId id,
        UserId userId,
        TrackId trackId,
        LibraryId libraryId,
        AlbumId albumId,
        ArtistId artistId,
        PlaybackRecordStatus status,
        MusicPlayPosition lastPosition,
        TimeSpan listenedDuration,
        Optional<DateTime> endedOnUtc,
        DateTime createdOnUtc,
        Optional<DateTime> updatedOnUtc)
    {
        if (status == PlaybackRecordStatus.InProgress && endedOnUtc.HasValue)
            return Errors.MusicPlayHistory.InProgressPlayCannotHaveEndTime;
        if (status != PlaybackRecordStatus.InProgress && !endedOnUtc.HasValue)
            return Errors.MusicPlayHistory.EndedPlayMustHaveEndTime;

        return new MusicPlayRecord(
            id,
            userId,
            trackId,
            libraryId,
            albumId,
            artistId,
            status,
            lastPosition,
            listenedDuration,
            endedOnUtc,
            createdOnUtc,
            updatedOnUtc);
    }

    /// <summary>
    /// Determines whether a play with the provided <paramref name="status"/> and <paramref name="listenedDuration"/> counts as a play for the listening statistics.
    /// </summary>
    /// <param name="status">The state of the recorded play.</param>
    /// <param name="listenedDuration">The duration that the user has listened to.</param>
    /// <returns><see langword="true"/> when the play counts, <see langword="false"/> otherwise.</returns>
    public static bool DoesCountAsPlay(PlaybackRecordStatus status, TimeSpan listenedDuration)
    {
        return status == PlaybackRecordStatus.Completed || listenedDuration.TotalSeconds >= MINIMUM_COUNTED_LISTENED_DURATION_SECONDS;
    }

    /// <summary>
    /// Updates the last reported position and the listened duration of an in-progress play.
    /// </summary>
    /// <param name="position">The last position reported by the player.</param>
    /// <param name="listenedDuration">The duration that the user has listened to.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful update, or an error.</returns>
    public Result<Updated> ReportProgress(MusicPlayPosition position, TimeSpan listenedDuration)
    {
        if (Status != PlaybackRecordStatus.InProgress)
            return Errors.MusicPlayHistory.PlayIsNotInProgress;
        if (listenedDuration < ListenedDuration)
            return Errors.MusicPlayHistory.ListenedDurationCannotDecrease;
        if (LastPosition.Offset == position.Offset && ListenedDuration == listenedDuration)
            return Result.Updated;

        LastPosition = position;
        ListenedDuration = listenedDuration;

        DateTime occurredOnUtc = DateTime.UtcNow;
        UpdatedOnUtc = Optional<DateTime>.Some(occurredOnUtc);
        _domainEvents.Add(new MusicPlayProgressedDomainEvent(
            Guid.NewGuid(),
            Id,
            UserId,
            TrackId,
            position,
            listenedDuration,
            occurredOnUtc));
        return Result.Updated;
    }

    /// <summary>
    /// Marks an in-progress play as completed.
    /// </summary>
    /// <param name="position">The last position reported by the player.</param>
    /// <param name="listenedDuration">The duration that the user listened to.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful update, or an error.</returns>
    public Result<Updated> Complete(MusicPlayPosition position, TimeSpan listenedDuration)
    {
        if (Status != PlaybackRecordStatus.InProgress)
            return Errors.MusicPlayHistory.PlayIsNotInProgress;
        if (listenedDuration < ListenedDuration)
            return Errors.MusicPlayHistory.ListenedDurationCannotDecrease;

        LastPosition = position;
        ListenedDuration = listenedDuration;
        Status = PlaybackRecordStatus.Completed;

        DateTime occurredOnUtc = DateTime.UtcNow;
        EndedOnUtc = Optional<DateTime>.Some(occurredOnUtc);
        UpdatedOnUtc = Optional<DateTime>.Some(occurredOnUtc);
        _domainEvents.Add(new MusicPlayCompletedDomainEvent(
            Guid.NewGuid(),
            Id,
            UserId,
            TrackId,
            position,
            listenedDuration,
            occurredOnUtc));
        return Result.Updated;
    }

    /// <summary>
    /// Marks an in-progress play as stopped before the track completed.
    /// </summary>
    /// <param name="position">The last position reported by the player.</param>
    /// <param name="listenedDuration">The duration that the user listened to.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful update, or an error.</returns>
    public Result<Updated> Stop(MusicPlayPosition position, TimeSpan listenedDuration)
    {
        if (Status != PlaybackRecordStatus.InProgress)
            return Errors.MusicPlayHistory.PlayIsNotInProgress;
        if (listenedDuration < ListenedDuration)
            return Errors.MusicPlayHistory.ListenedDurationCannotDecrease;

        LastPosition = position;
        ListenedDuration = listenedDuration;
        Status = PlaybackRecordStatus.Stopped;

        DateTime occurredOnUtc = DateTime.UtcNow;
        EndedOnUtc = Optional<DateTime>.Some(occurredOnUtc);
        UpdatedOnUtc = Optional<DateTime>.Some(occurredOnUtc);
        _domainEvents.Add(new MusicPlayStoppedDomainEvent(
            Guid.NewGuid(),
            Id,
            UserId,
            TrackId,
            position,
            listenedDuration,
            occurredOnUtc));
        return Result.Updated;
    }
}
