#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Models.Core;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.Common.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.ExternalIdentifiers.LibraryManagementBoundedContext.LibraryAggregate;
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.ExternalIdentifiers.MediaLibraryBoundedContext.MediaItemAggregate;
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.ExternalIdentifiers.UserManagementBoundedContext.UserAggregate;
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.MediaConsumptionRecordAggregate.Events;
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.MediaConsumptionRecordAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.MediaConsumption;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.MediaConsumptionRecordAggregate;

/// <summary>
/// Aggregate root for a media consumption session.
/// </summary>
[DebuggerDisplay("Id: {Id.Value}; MediaKind: {MediaKind}; Status: {Status}")]
public sealed class MediaConsumptionRecord : AggregateRoot<MediaConsumptionRecordId>
{
    /// <summary>
    /// Gets the object representing the unique identifier of the user that started the consumption session.
    /// </summary>
    public UserId UserId { get; private set; }

    /// <summary>
    /// Gets the Id of the media library that contains the consumed media item.
    /// </summary>
    public LibraryId LibraryId { get; private set; }

    /// <summary>
    /// Gets the Id of the consumed media item.
    /// </summary>
    public MediaItemId MediaItemId { get; private set; }

    /// <summary>
    /// Gets the kind of media that is consumed.
    /// </summary>
    public MediaKind MediaKind { get; private set; }

    /// <summary>
    /// Gets the state of the consumption session.
    /// </summary>
    public ConsumptionStatus Status { get; private set; }

    /// <summary>
    /// Gets the date and time when the consumption session started.
    /// </summary>
    public DateTime StartedOnUtc { get; private set; }

    /// <summary>
    /// Gets the date and time when the consumption session last received activity.
    /// </summary>
    public DateTime LastActivityOnUtc { get; private set; }

    /// <summary>
    /// Gets the date and time when the consumption session completed or stopped, if it has ended.
    /// </summary>
    public Optional<DateTime> EndedOnUtc { get; private set; }

    /// <summary>
    /// Gets the active duration that the user has consumed in the session. It never decreases within one record.
    /// </summary>
    public TimeSpan ConsumedDuration { get; private set; }

    /// <summary>
    /// Gets the last position reported for the media item, if the session has a known position. The position is the latest reported one, so it may
    /// be earlier in the media item than a previously reported position.
    /// </summary>
    public Optional<MediaPosition> LastPosition { get; private set; }

    /// <summary>
    /// Gets the latest reported progress fraction of the media item. It is the latest reported value, so it may be lower than a previously reported
    /// fraction.
    /// </summary>
    public double LastProgressFraction { get; private set; }

    /// <summary>
    /// Gets the number of consecutive pages reported read in the current session. It is zero for every kind of media except books.
    /// </summary>
    public int ConsecutivePagesRead { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaConsumptionRecord"/> class.
    /// </summary>
    /// <param name="id">The object representing the unique identifier of the consumption record.</param>
    /// <param name="userId">The object representing the unique identifier of the user that started the consumption session.</param>
    /// <param name="libraryId">The Id of the media library that contains the consumed media item.</param>
    /// <param name="mediaItemId">The Id of the consumed media item.</param>
    /// <param name="mediaKind">The kind of media that is consumed.</param>
    /// <param name="status">The state of the consumption session.</param>
    /// <param name="startedOnUtc">The date and time when the consumption session started.</param>
    /// <param name="lastActivityOnUtc">The date and time when the consumption session last received activity.</param>
    /// <param name="endedOnUtc">The date and time when the consumption session completed or stopped, if it has ended.</param>
    /// <param name="consumedDuration">The active duration that the user has consumed in the session.</param>
    /// <param name="lastPosition">The last position reported for the media item, if the session has a known position.</param>
    /// <param name="lastProgressFraction">The latest reported progress fraction of the media item.</param>
    /// <param name="consecutivePagesRead">The number of consecutive pages reported read in the current session.</param>
    private MediaConsumptionRecord(
        MediaConsumptionRecordId id,
        UserId userId,
        LibraryId libraryId,
        MediaItemId mediaItemId,
        MediaKind mediaKind,
        ConsumptionStatus status,
        DateTime startedOnUtc,
        DateTime lastActivityOnUtc,
        Optional<DateTime> endedOnUtc,
        TimeSpan consumedDuration,
        Optional<MediaPosition> lastPosition,
        double lastProgressFraction,
        int consecutivePagesRead) : base(id)
    {
        Id = id;
        UserId = userId;
        LibraryId = libraryId;
        MediaItemId = mediaItemId;
        MediaKind = mediaKind;
        Status = status;
        StartedOnUtc = startedOnUtc;
        LastActivityOnUtc = lastActivityOnUtc;
        EndedOnUtc = endedOnUtc;
        ConsumedDuration = consumedDuration;
        LastPosition = lastPosition;
        LastProgressFraction = lastProgressFraction;
        ConsecutivePagesRead = consecutivePagesRead;
        CreatedOnUtc = startedOnUtc;
        UpdatedOnUtc = lastActivityOnUtc > startedOnUtc ? Optional<DateTime>.Some(lastActivityOnUtc) : Optional<DateTime>.None();
    }

    /// <summary>
    /// Starts a new consumption session for a media item.
    /// </summary>
    /// <param name="userId">The object representing the unique identifier of the user that starts the consumption session.</param>
    /// <param name="libraryId">The Id of the media library that contains the consumed media item.</param>
    /// <param name="mediaItemId">The Id of the consumed media item.</param>
    /// <param name="mediaKind">The kind of media that is consumed.</param>
    /// <param name="lastPosition">The position to initialize the session with, such as the position of an existing resume point, if any.</param>
    /// <param name="lastProgressFraction">The progress fraction to initialize the session with, such as the fraction of an existing resume point.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a successfully started <see cref="MediaConsumptionRecord"/>, or an error message.
    /// </returns>
    public static Result<MediaConsumptionRecord> Start(UserId userId, LibraryId libraryId, MediaItemId mediaItemId, MediaKind mediaKind, Optional<MediaPosition> lastPosition, double lastProgressFraction)
    {
        Optional<MediaConsumptionRule> ruleResult = MediaConsumptionRule.For(mediaKind);
        if (!ruleResult.HasValue)
            return Errors.MediaConsumption.UnsupportedMediaKind;
        if (!IsProgressFractionValid(lastProgressFraction))
            return Errors.MediaConsumption.InvalidProgressFraction;
        if (!DoesPositionMatchRule(ruleResult.Value, lastPosition))
            return Errors.MediaConsumption.InvalidMediaPosition;

        DateTime occurredOnUtc = DateTime.UtcNow;
        MediaConsumptionRecord record = new(
            MediaConsumptionRecordId.CreateUnique(),
            userId,
            libraryId,
            mediaItemId,
            mediaKind,
            ConsumptionStatus.InProgress,
            occurredOnUtc,
            occurredOnUtc,
            Optional<DateTime>.None(),
            TimeSpan.Zero,
            lastPosition,
            lastProgressFraction,
            0);

        record._domainEvents.Add(new MediaConsumptionStartedDomainEvent(
            Guid.NewGuid(),
            record.Id,
            userId,
            libraryId,
            mediaItemId,
            mediaKind,
            occurredOnUtc));
        return record;
    }

    /// <summary>
    /// Starts and completes a consumption session in the same call, for one-shot discrete kinds of media such as a photo view.
    /// </summary>
    /// <param name="userId">The object representing the unique identifier of the user that consumed the media item.</param>
    /// <param name="libraryId">The Id of the media library that contains the consumed media item.</param>
    /// <param name="mediaItemId">The Id of the consumed media item.</param>
    /// <param name="mediaKind">The kind of media that is consumed.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a successfully created completed <see cref="MediaConsumptionRecord"/>, or an error message.
    /// </returns>
    public static Result<MediaConsumptionRecord> StartCompleted(UserId userId, LibraryId libraryId, MediaItemId mediaItemId, MediaKind mediaKind)
    {
        Optional<MediaConsumptionRule> ruleResult = MediaConsumptionRule.For(mediaKind);
        if (!ruleResult.HasValue)
            return Errors.MediaConsumption.UnsupportedMediaKind;
        if (!ruleResult.Value.IsOneShot)
            return Errors.MediaConsumption.MediaKindCannotStartCompleted;

        DateTime occurredOnUtc = DateTime.UtcNow;
        MediaConsumptionRecord record = new(
            MediaConsumptionRecordId.CreateUnique(),
            userId,
            libraryId,
            mediaItemId,
            mediaKind,
            ConsumptionStatus.Completed,
            occurredOnUtc,
            occurredOnUtc,
            Optional<DateTime>.Some(occurredOnUtc),
            TimeSpan.Zero,
            Optional<MediaPosition>.None(),
            1,
            0);

        record._domainEvents.Add(new MediaConsumptionStartedDomainEvent(
            Guid.NewGuid(),
            record.Id,
            userId,
            libraryId,
            mediaItemId,
            mediaKind,
            occurredOnUtc));
        record._domainEvents.Add(new MediaConsumptionCompletedDomainEvent(
            Guid.NewGuid(),
            record.Id,
            userId,
            libraryId,
            mediaItemId,
            mediaKind,
            record.LastPosition,
            record.LastProgressFraction,
            record.ConsumedDuration,
            record.ConsecutivePagesRead,
            occurredOnUtc));
        return record;
    }

    /// <summary>
    /// Creates a new instance of the <see cref="MediaConsumptionRecord"/> class, with pre-existing data.
    /// </summary>
    /// <param name="id">The object representing the unique identifier of the consumption record.</param>
    /// <param name="userId">The object representing the unique identifier of the user that started the consumption session.</param>
    /// <param name="libraryId">The Id of the media library that contains the consumed media item.</param>
    /// <param name="mediaItemId">The Id of the consumed media item.</param>
    /// <param name="mediaKind">The kind of media that is consumed.</param>
    /// <param name="status">The state of the consumption session.</param>
    /// <param name="startedOnUtc">The date and time when the consumption session started.</param>
    /// <param name="lastActivityOnUtc">The date and time when the consumption session last received activity.</param>
    /// <param name="endedOnUtc">The date and time when the consumption session completed or stopped, if it has ended.</param>
    /// <param name="consumedDuration">The active duration that the user has consumed in the session.</param>
    /// <param name="lastPosition">The last position reported for the media item, if the session has a known position.</param>
    /// <param name="lastProgressFraction">The latest reported progress fraction of the media item.</param>
    /// <param name="consecutivePagesRead">The number of consecutive pages reported read in the current session.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a successfully reconstituted <see cref="MediaConsumptionRecord"/>, or an error message.
    /// </returns>
    public static Result<MediaConsumptionRecord> Reconstitute(
        MediaConsumptionRecordId id,
        UserId userId,
        LibraryId libraryId,
        MediaItemId mediaItemId,
        MediaKind mediaKind,
        ConsumptionStatus status,
        DateTime startedOnUtc,
        DateTime lastActivityOnUtc,
        Optional<DateTime> endedOnUtc,
        TimeSpan consumedDuration,
        Optional<MediaPosition> lastPosition,
        double lastProgressFraction,
        int consecutivePagesRead)
    {
        Optional<MediaConsumptionRule> ruleResult = MediaConsumptionRule.For(mediaKind);
        if (!ruleResult.HasValue)
            return Errors.MediaConsumption.UnsupportedMediaKind;
        if (!IsProgressFractionValid(lastProgressFraction))
            return Errors.MediaConsumption.InvalidProgressFraction;
        if (consecutivePagesRead > 0 && !IsPageBased(ruleResult.Value))
            return Errors.MediaConsumption.ConsecutivePagesReadIsOnlySupportedForPageBasedMedia;
        if (!DoesPositionMatchRule(ruleResult.Value, lastPosition))
            return Errors.MediaConsumption.InvalidMediaPosition;
        if (status == ConsumptionStatus.InProgress && endedOnUtc.HasValue)
            return Errors.MediaConsumption.InProgressConsumptionCannotHaveEndTime;
        if (status != ConsumptionStatus.InProgress && !endedOnUtc.HasValue)
            return Errors.MediaConsumption.EndedConsumptionMustHaveEndTime;
        if (lastActivityOnUtc < startedOnUtc)
            return Errors.MediaConsumption.LastActivityCannotPrecedeStart;
        if (status != ConsumptionStatus.InProgress && endedOnUtc.Value < lastActivityOnUtc)
            return Errors.MediaConsumption.EndedConsumptionCannotPrecedeLastActivity;

        return new MediaConsumptionRecord(
            id,
            userId,
            libraryId,
            mediaItemId,
            mediaKind,
            status,
            startedOnUtc,
            lastActivityOnUtc,
            endedOnUtc,
            consumedDuration,
            lastPosition,
            lastProgressFraction,
            consecutivePagesRead);
    }

    /// <summary>
    /// Updates the measured progress of an in-progress consumption session.
    /// </summary>
    /// <param name="lastPosition">The last position reported by the client, if the report carries one. When it is absent, the last known position is kept.</param>
    /// <param name="lastProgressFraction">The last progress fraction reported by the client.</param>
    /// <param name="consumedDuration">The active duration consumed so far.</param>
    /// <param name="consecutivePagesRead">The number of consecutive pages read in the current session.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful update, or an error.</returns>
    public Result<Updated> ReportProgress(Optional<MediaPosition> lastPosition, double lastProgressFraction, TimeSpan consumedDuration, int consecutivePagesRead)
    {
        if (Status != ConsumptionStatus.InProgress)
            return Errors.MediaConsumption.ConsumptionIsNotInProgress;

        Optional<MediaConsumptionRule> ruleResult = MediaConsumptionRule.For(MediaKind);
        if (!ruleResult.HasValue)
            return Errors.MediaConsumption.UnsupportedMediaKind;
        if (!IsProgressFractionValid(lastProgressFraction))
            return Errors.MediaConsumption.InvalidProgressFraction;
        if (consumedDuration < ConsumedDuration)
            return Errors.MediaConsumption.ConsumedDurationCannotDecrease;
        if (consecutivePagesRead > 0 && !IsPageBased(ruleResult.Value))
            return Errors.MediaConsumption.ConsecutivePagesReadIsOnlySupportedForPageBasedMedia;
        if (!DoesPositionMatchRule(ruleResult.Value, lastPosition))
            return Errors.MediaConsumption.InvalidMediaPosition;

        if (lastPosition.HasValue)
            LastPosition = lastPosition;
        LastProgressFraction = lastProgressFraction;
        ConsumedDuration = consumedDuration;
        ConsecutivePagesRead = consecutivePagesRead;

        DateTime occurredOnUtc = GetNextActivityOnUtc();
        LastActivityOnUtc = occurredOnUtc;
        UpdatedOnUtc = Optional<DateTime>.Some(occurredOnUtc);
        _domainEvents.Add(new MediaConsumptionProgressedDomainEvent(
            Guid.NewGuid(),
            Id,
            UserId,
            LibraryId,
            MediaItemId,
            MediaKind,
            LastPosition,
            LastProgressFraction,
            ConsumedDuration,
            ConsecutivePagesRead,
            occurredOnUtc));
        return Result.Updated;
    }

    /// <summary>
    /// Marks an in-progress consumption session as completed.
    /// </summary>
    /// <param name="lastPosition">The final position reported by the client, if the call carries one. When it is absent, the last known position is kept.</param>
    /// <param name="lastProgressFraction">The final progress fraction reported by the client.</param>
    /// <param name="consumedDuration">The active duration consumed by the session.</param>
    /// <param name="consecutivePagesRead">The number of consecutive pages read in the session, when the client reported it. When it is absent, the count of the last progress report is kept.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful completion, or an error.</returns>
    public Result<Updated> Complete(Optional<MediaPosition> lastPosition, double lastProgressFraction, TimeSpan consumedDuration, Optional<int> consecutivePagesRead)
    {
        if (Status != ConsumptionStatus.InProgress)
            return Errors.MediaConsumption.ConsumptionIsAlreadyTerminal;

        Optional<MediaConsumptionRule> ruleResult = MediaConsumptionRule.For(MediaKind);
        if (!ruleResult.HasValue)
            return Errors.MediaConsumption.UnsupportedMediaKind;
        if (!IsProgressFractionValid(lastProgressFraction))
            return Errors.MediaConsumption.InvalidProgressFraction;
        if (consumedDuration < ConsumedDuration)
            return Errors.MediaConsumption.ConsumedDurationCannotDecrease;
        if (consecutivePagesRead.HasValue && consecutivePagesRead.Value > 0 && !IsPageBased(ruleResult.Value))
            return Errors.MediaConsumption.ConsecutivePagesReadIsOnlySupportedForPageBasedMedia;
        if (!DoesPositionMatchRule(ruleResult.Value, lastPosition))
            return Errors.MediaConsumption.InvalidMediaPosition;

        if (lastPosition.HasValue)
            LastPosition = lastPosition;
        LastProgressFraction = lastProgressFraction;
        ConsumedDuration = consumedDuration;
        if (consecutivePagesRead.HasValue)
            ConsecutivePagesRead = consecutivePagesRead.Value;
        Status = ConsumptionStatus.Completed;

        DateTime occurredOnUtc = GetNextActivityOnUtc();
        EndedOnUtc = Optional<DateTime>.Some(occurredOnUtc);
        LastActivityOnUtc = occurredOnUtc;
        UpdatedOnUtc = Optional<DateTime>.Some(occurredOnUtc);
        _domainEvents.Add(new MediaConsumptionCompletedDomainEvent(
            Guid.NewGuid(),
            Id,
            UserId,
            LibraryId,
            MediaItemId,
            MediaKind,
            LastPosition,
            LastProgressFraction,
            ConsumedDuration,
            ConsecutivePagesRead,
            occurredOnUtc));
        return Result.Updated;
    }

    /// <summary>
    /// Marks an in-progress consumption session as stopped before it completed.
    /// </summary>
    /// <param name="lastPosition">The final position reported by the client, if the call carries one. When it is absent, the last known position is kept.</param>
    /// <param name="lastProgressFraction">The final progress fraction reported by the client.</param>
    /// <param name="consumedDuration">The active duration consumed by the session.</param>
    /// <param name="consecutivePagesRead">The number of consecutive pages read in the session, when the client reported it. When it is absent, the count of the last progress report is kept.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful stop, or an error.</returns>
    public Result<Updated> Stop(Optional<MediaPosition> lastPosition, double lastProgressFraction, TimeSpan consumedDuration, Optional<int> consecutivePagesRead)
    {
        if (Status != ConsumptionStatus.InProgress)
            return Errors.MediaConsumption.ConsumptionIsAlreadyTerminal;

        Optional<MediaConsumptionRule> ruleResult = MediaConsumptionRule.For(MediaKind);
        if (!ruleResult.HasValue)
            return Errors.MediaConsumption.UnsupportedMediaKind;
        if (!IsProgressFractionValid(lastProgressFraction))
            return Errors.MediaConsumption.InvalidProgressFraction;
        if (consumedDuration < ConsumedDuration)
            return Errors.MediaConsumption.ConsumedDurationCannotDecrease;
        if (consecutivePagesRead.HasValue && consecutivePagesRead.Value > 0 && !IsPageBased(ruleResult.Value))
            return Errors.MediaConsumption.ConsecutivePagesReadIsOnlySupportedForPageBasedMedia;
        if (!DoesPositionMatchRule(ruleResult.Value, lastPosition))
            return Errors.MediaConsumption.InvalidMediaPosition;

        if (lastPosition.HasValue)
            LastPosition = lastPosition;
        LastProgressFraction = lastProgressFraction;
        ConsumedDuration = consumedDuration;
        if (consecutivePagesRead.HasValue)
            ConsecutivePagesRead = consecutivePagesRead.Value;
        Status = ConsumptionStatus.Stopped;

        DateTime occurredOnUtc = GetNextActivityOnUtc();
        EndedOnUtc = Optional<DateTime>.Some(occurredOnUtc);
        LastActivityOnUtc = occurredOnUtc;
        UpdatedOnUtc = Optional<DateTime>.Some(occurredOnUtc);
        _domainEvents.Add(new MediaConsumptionStoppedDomainEvent(
            Guid.NewGuid(),
            Id,
            UserId,
            LibraryId,
            MediaItemId,
            MediaKind,
            LastPosition,
            LastProgressFraction,
            ConsumedDuration,
            ConsecutivePagesRead,
            occurredOnUtc));
        return Result.Updated;
    }

    /// <summary>
    /// Determines whether the progress fraction is a finite value between zero and one.
    /// </summary>
    /// <param name="progressFraction">The progress fraction to check.</param>
    /// <returns><see langword="true"/> if the progress fraction is valid, <see langword="false"/> otherwise.</returns>
    private static bool IsProgressFractionValid(double progressFraction)
    {
        return double.IsFinite(progressFraction) && progressFraction >= 0 && progressFraction <= 1;
    }

    /// <summary>
    /// Determines whether the rule of the media kind counts its sessions by the number of consecutive pages read.
    /// </summary>
    /// <param name="rule">The consumption rule of the media kind.</param>
    /// <returns><see langword="true"/> if the rule is page based, <see langword="false"/> otherwise.</returns>
    private static bool IsPageBased(MediaConsumptionRule rule)
    {
        return rule is PageThresholdConsumptionRule;
    }

    /// <summary>
    /// Determines whether a reported position uses the position representation of the consumption rule of the media kind.
    /// </summary>
    /// <param name="rule">The consumption rule of the media kind.</param>
    /// <param name="lastPosition">The reported position, if the report carries one.</param>
    /// <returns><see langword="true"/> if the position is valid for the media kind, <see langword="false"/> otherwise.</returns>
    private static bool DoesPositionMatchRule(MediaConsumptionRule rule, Optional<MediaPosition> lastPosition)
    {
        if (!lastPosition.HasValue)
            return true;
        return rule.PositionKind.HasValue && lastPosition.Value.Kind == rule.PositionKind.Value;
    }

    /// <summary>
    /// Gets the current UTC date and time, advanced past the last recorded activity so that activity timestamps never go backwards. When the last
    /// recorded activity is already the maximum representable timestamp, that timestamp is returned instead of advancing it.
    /// </summary>
    /// <returns>The date and time of the new activity.</returns>
    private DateTime GetNextActivityOnUtc()
    {
        DateTime utcNow = DateTime.UtcNow;
        if (utcNow > LastActivityOnUtc)
            return utcNow;
        if (LastActivityOnUtc == DateTime.MaxValue)
            return DateTime.MaxValue;
        return LastActivityOnUtc.AddTicks(1);
    }
}
