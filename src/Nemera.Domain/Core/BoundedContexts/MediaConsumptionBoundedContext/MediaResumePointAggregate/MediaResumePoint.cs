#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Models.Core;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.Common.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.ExternalIdentifiers.LibraryManagementBoundedContext.LibraryAggregate;
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.ExternalIdentifiers.MediaLibraryBoundedContext.MediaItemAggregate;
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.ExternalIdentifiers.UserManagementBoundedContext.UserAggregate;
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.MediaResumePointAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.MediaResumePointAggregate;

/// <summary>
/// Aggregate root for the point at which a user can resume consuming a media item.
/// </summary>
[DebuggerDisplay("Id: {Id.Value}; MediaKind: {MediaKind}; MediaItemId: {MediaItemId.Value}")]
public sealed class MediaResumePoint : AggregateRoot<MediaResumePointId>
{
    /// <summary>
    /// Gets the object representing the unique identifier of the user that owns the resume point.
    /// </summary>
    public UserId UserId { get; private set; }

    /// <summary>
    /// Gets the Id of the media library that contains the media item.
    /// </summary>
    public LibraryId LibraryId { get; private set; }

    /// <summary>
    /// Gets the Id of the media item to resume.
    /// </summary>
    public MediaItemId MediaItemId { get; private set; }

    /// <summary>
    /// Gets the kind of media that is resumed.
    /// </summary>
    public MediaKind MediaKind { get; private set; }

    /// <summary>
    /// Gets the position at which the media item can be resumed.
    /// </summary>
    public MediaPosition LastPosition { get; private set; }

    /// <summary>
    /// Gets the progress fraction at which the media item can be resumed.
    /// </summary>
    public double LastProgressFraction { get; private set; }

    /// <summary>
    /// Gets the date and time when the media item was last consumed.
    /// </summary>
    public DateTime LastConsumedOnUtc { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaResumePoint"/> class.
    /// </summary>
    /// <param name="id">The object representing the unique identifier of the resume point.</param>
    /// <param name="userId">The object representing the unique identifier of the user that owns the resume point.</param>
    /// <param name="libraryId">The Id of the media library that contains the media item.</param>
    /// <param name="mediaItemId">The Id of the media item to resume.</param>
    /// <param name="mediaKind">The kind of media that is resumed.</param>
    /// <param name="lastPosition">The position at which the media item can be resumed.</param>
    /// <param name="lastProgressFraction">The progress fraction at which the media item can be resumed.</param>
    /// <param name="lastConsumedOnUtc">The date and time when the media item was last consumed.</param>
    /// <param name="createdOnUtc">The date and time when the resume point was created.</param>
    /// <param name="updatedOnUtc">The date and time when the resume point was last updated, if applicable.</param>
    private MediaResumePoint(
        MediaResumePointId id,
        UserId userId,
        LibraryId libraryId,
        MediaItemId mediaItemId,
        MediaKind mediaKind,
        MediaPosition lastPosition,
        double lastProgressFraction,
        DateTime lastConsumedOnUtc,
        DateTime createdOnUtc,
        Optional<DateTime> updatedOnUtc) : base(id)
    {
        Id = id;
        UserId = userId;
        LibraryId = libraryId;
        MediaItemId = mediaItemId;
        MediaKind = mediaKind;
        LastPosition = lastPosition;
        LastProgressFraction = lastProgressFraction;
        LastConsumedOnUtc = lastConsumedOnUtc;
        CreatedOnUtc = createdOnUtc;
        UpdatedOnUtc = updatedOnUtc;
    }

    /// <summary>
    /// Creates a new resume point for a media item.
    /// </summary>
    /// <param name="userId">The object representing the unique identifier of the user that owns the resume point.</param>
    /// <param name="libraryId">The Id of the media library that contains the media item.</param>
    /// <param name="mediaItemId">The Id of the media item to resume.</param>
    /// <param name="mediaKind">The kind of media that is resumed.</param>
    /// <param name="lastPosition">The position at which the media item can be resumed.</param>
    /// <param name="lastProgressFraction">The progress fraction at which the media item can be resumed.</param>
    /// <param name="lastConsumedOnUtc">The date and time when the media item was last consumed.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a successfully created <see cref="MediaResumePoint"/>, or an error message.
    /// </returns>
    public static Result<MediaResumePoint> Create(
        UserId userId,
        LibraryId libraryId,
        MediaItemId mediaItemId,
        MediaKind mediaKind,
        MediaPosition lastPosition,
        double lastProgressFraction,
        DateTime lastConsumedOnUtc)
    {
        Optional<MediaConsumptionRule> ruleResult = MediaConsumptionRule.For(mediaKind);
        if (!ruleResult.HasValue)
            return Errors.MediaConsumption.UnsupportedMediaKind;
        if (!ruleResult.Value.CanResume)
            return Errors.MediaConsumption.MediaKindCannotResume;
        if (!DoesPositionMatchRule(ruleResult.Value, lastPosition))
            return Errors.MediaConsumption.InvalidMediaPosition;
        if (!IsProgressFractionValid(lastProgressFraction))
            return Errors.MediaConsumption.InvalidProgressFraction;

        // A resume point is created when the media item is first consumed, so the first consumption time is also its creation time.
        return new MediaResumePoint(
            MediaResumePointId.CreateUnique(),
            userId,
            libraryId,
            mediaItemId,
            mediaKind,
            lastPosition,
            lastProgressFraction,
            lastConsumedOnUtc,
            lastConsumedOnUtc,
            Optional<DateTime>.None());
    }

    /// <summary>
    /// Creates a new instance of the <see cref="MediaResumePoint"/> class, with pre-existing data.
    /// </summary>
    /// <param name="id">The object representing the unique identifier of the resume point.</param>
    /// <param name="userId">The object representing the unique identifier of the user that owns the resume point.</param>
    /// <param name="libraryId">The Id of the media library that contains the media item.</param>
    /// <param name="mediaItemId">The Id of the media item to resume.</param>
    /// <param name="mediaKind">The kind of media that is resumed.</param>
    /// <param name="lastPosition">The position at which the media item can be resumed.</param>
    /// <param name="lastProgressFraction">The progress fraction at which the media item can be resumed.</param>
    /// <param name="lastConsumedOnUtc">The date and time when the media item was last consumed.</param>
    /// <param name="createdOnUtc">The date and time when the resume point was created.</param>
    /// <param name="updatedOnUtc">The date and time when the resume point was last updated, if applicable.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a successfully reconstituted <see cref="MediaResumePoint"/>, or an error message.
    /// </returns>
    public static Result<MediaResumePoint> Reconstitute(
        MediaResumePointId id,
        UserId userId,
        LibraryId libraryId,
        MediaItemId mediaItemId,
        MediaKind mediaKind,
        MediaPosition lastPosition,
        double lastProgressFraction,
        DateTime lastConsumedOnUtc,
        DateTime createdOnUtc,
        Optional<DateTime> updatedOnUtc)
    {
        Optional<MediaConsumptionRule> ruleResult = MediaConsumptionRule.For(mediaKind);
        if (!ruleResult.HasValue)
            return Errors.MediaConsumption.UnsupportedMediaKind;
        if (!ruleResult.Value.CanResume)
            return Errors.MediaConsumption.MediaKindCannotResume;
        if (!DoesPositionMatchRule(ruleResult.Value, lastPosition))
            return Errors.MediaConsumption.InvalidMediaPosition;
        if (!IsProgressFractionValid(lastProgressFraction))
            return Errors.MediaConsumption.InvalidProgressFraction;

        return new MediaResumePoint(
            id,
            userId,
            libraryId,
            mediaItemId,
            mediaKind,
            lastPosition,
            lastProgressFraction,
            lastConsumedOnUtc,
            createdOnUtc,
            updatedOnUtc);
    }

    /// <summary>
    /// Moves the resume point to a new position and progress fraction.
    /// </summary>
    /// <param name="position">The position at which the media item can be resumed.</param>
    /// <param name="progressFraction">The progress fraction at which the media item can be resumed.</param>
    /// <param name="lastConsumedOnUtc">The date and time when the media item was last consumed.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful update, or an error.</returns>
    public Result<Updated> Advance(MediaPosition position, double progressFraction, DateTime lastConsumedOnUtc)
    {
        Optional<MediaConsumptionRule> ruleResult = MediaConsumptionRule.For(MediaKind);
        if (!ruleResult.HasValue)
            return Errors.MediaConsumption.UnsupportedMediaKind;
        if (!DoesPositionMatchRule(ruleResult.Value, position))
            return Errors.MediaConsumption.InvalidMediaPosition;
        if (!IsProgressFractionValid(progressFraction))
            return Errors.MediaConsumption.InvalidProgressFraction;

        LastPosition = position;
        LastProgressFraction = progressFraction;
        LastConsumedOnUtc = lastConsumedOnUtc;
        UpdatedOnUtc = Optional<DateTime>.Some(lastConsumedOnUtc);
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
    /// Determines whether the position uses the position representation of the consumption rule of the media kind.
    /// </summary>
    /// <param name="rule">The consumption rule of the media kind.</param>
    /// <param name="position">The position to check.</param>
    /// <returns><see langword="true"/> if the position is valid for the media kind, <see langword="false"/> otherwise.</returns>
    private static bool DoesPositionMatchRule(MediaConsumptionRule rule, MediaPosition position)
    {
        return rule.PositionKind.HasValue && position.Kind == rule.PositionKind.Value;
    }
}
