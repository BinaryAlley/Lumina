#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.DataAccess.Entities.MediaConsumption;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.DomainEvents;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Mapping.MediaConsumption;
using Lumina.Application.Common.Mapping.MediaLibrary.Management;
using Lumina.Contracts.Responses.MediaConsumption;
using Lumina.Domain.Common.Events;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.Common.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.MediaConsumptionRecordAggregate;
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.MediaResumePointAggregate;
using Lumina.Domain.SharedKernel.Common.Enums.MediaConsumption;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
#endregion

namespace Lumina.Application.Core.MediaConsumption.Commands.StopConsumption;

/// <summary>
/// Handler for the command to report that a consumption session of the current user stopped before it completed.
/// </summary>
public class StopConsumptionCommandHandler : ICommandHandler<StopConsumptionCommand, Result<ConsumptionRecordResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDomainEventsQueue _domainEventsQueue;
    private readonly IValidator<StopConsumptionCommand> _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="StopConsumptionCommandHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    /// <param name="currentUserService">Injected service to retrieve the current user information.</param>
    /// <param name="domainEventsQueue">Injected service for the queue of domain events.</param>
    /// <param name="validator">Injected validator for application validation rules.</param>
    public StopConsumptionCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IDomainEventsQueue domainEventsQueue,
        IValidator<StopConsumptionCommand> validator)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _domainEventsQueue = domainEventsQueue;
        _validator = validator;
    }

    /// <summary>
    /// Handles the command to report that a consumption session of the current user stopped before it completed.
    /// </summary>
    /// <param name="command">The command to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either the stopped or completed <see cref="ConsumptionRecordResponse"/>, or an error message.
    /// </returns>
    public async Task<Result<ConsumptionRecordResponse>> HandleAsync(StopConsumptionCommand command, CancellationToken cancellationToken)
    {
        List<Error> validationResult = _validator.Validate(command);
        if (validationResult.Count > 0)
            return validationResult;

        // An authenticated request must always carry a user identity.
        Guid? currentUserId = _currentUserService.UserId;
        if (currentUserId is null)
            return ApplicationErrors.Authorization.NotAuthorized;
        Guid userId = currentUserId.Value;

        // The validator guarantees that the route identifier and the reported measurements are valid before this point.
        Guid consumptionRecordId = Guid.Parse(command.ConsumptionRecordId!);

        // The record is loaded scoped to the current user, so a record of another user is indistinguishable from a missing one.
        Result<MediaConsumptionRecordEntity?> getRecordResult = await _unitOfWork.MediaConsumptionRecordRepository.GetByIdForUserAsync(consumptionRecordId, userId, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (getRecordResult.IsFailure)
            return getRecordResult.Errors;
        if (getRecordResult.Value is null)
            return DomainErrors.MediaConsumption.ConsumptionRecordNotFound;

        Result<MediaConsumptionRecord> toDomainEntityResult = getRecordResult.Value.ToDomainEntity();
        if (toDomainEntityResult.IsFailure)
            return toDomainEntityResult.Errors;
        MediaConsumptionRecord record = toDomainEntityResult.Value;

        // The settings of the library decide whether reaching a threshold turns the stop into a completion. If the library was deleted after
        // the session started, the defaults of the media kind are used instead, so the reported session can still be closed consistently.
        Result<LibraryEntity?> getLibraryResult = await _unitOfWork.LibraryRepository
            .GetByIdAsync(record.LibraryId.Value, shouldIncludeNavigationProperties: false, shouldTrackEntities: false, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (getLibraryResult.IsFailure)
            return getLibraryResult.Errors;
        LibraryConsumptionSettings consumptionSettings = getLibraryResult.Value is null
            ? LibraryConsumptionSettings.CreateDefault(record.MediaKind)
            : getLibraryResult.Value.ToDomainValueObject();

        Optional<MediaPosition> position = ToMediaPosition(command.PositionKind, command.PositionTimeOffsetInSeconds, command.PositionLocation);
        double progressFraction = command.ProgressFraction!.Value;
        TimeSpan consumedDuration = TimeSpan.FromSeconds(command.ConsumedDurationInSeconds!.Value);

        // A session stopped at or after the completion threshold of its library is recorded as completed, not stopped, and loses its resume
        // point. Tracking on without a threshold, and tracking off, never complete a session implicitly. The percentage is divided instead of
        // multiplying the fraction by one hundred, because the multiplication can round a fraction that is exactly at the threshold below it.
        bool isAtCompletionThreshold = consumptionSettings.ShouldTrackConsumptionProgress &&
                                       consumptionSettings.CompletionThresholdPercentage.HasValue &&
                                       progressFraction >= consumptionSettings.CompletionThresholdPercentage.Value / 100.0;

        Result<Updated> terminalResult = isAtCompletionThreshold
            ? record.Complete(position, progressFraction, consumedDuration)
            : record.Stop(position, progressFraction, consumedDuration);
        if (terminalResult.IsFailure)
            return terminalResult.Errors;

        Optional<MediaConsumptionRule> ruleResult = MediaConsumptionRule.For(record.MediaKind);
        if (!ruleResult.HasValue)
            return DomainErrors.MediaConsumption.UnsupportedMediaKind;

        if (isAtCompletionThreshold)
        {
            // Completion discards the resume point, because the media item no longer has a point to continue from.
            Result<Deleted> deleteResumePointResult = await _unitOfWork.MediaResumePointRepository.DeleteForUserAndMediaItemAsync(userId, record.MediaKind, record.MediaItemId.Value, cancellationToken).ConfigureAwait(false);
            if (deleteResumePointResult.IsFailure)
                return deleteResumePointResult.Errors;
        }
        else if (consumptionSettings.ShouldTrackConsumptionProgress && ruleResult.Value.CanResume && record.LastPosition.HasValue)
        {
            // A stopped session that did not reach the completion threshold keeps a resume point. Without a known position there is nothing to resume from, so no resume point is written.
            Result<Updated> upsertResumePointResult = await UpsertResumePointAsync(record, record.LastPosition.Value, record.LastProgressFraction, userId, cancellationToken).ConfigureAwait(false);
            if (upsertResumePointResult.IsFailure)
                return upsertResumePointResult.Errors;
        }

        Result<Updated> updateResult = await _unitOfWork.MediaConsumptionRecordRepository.UpdateForUserAsync(record.ToRepositoryEntity(), userId, cancellationToken).ConfigureAwait(false);
        if (updateResult.IsFailure)
            return updateResult.Errors;

        Result<Success> saveChangesResult = await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        if (saveChangesResult.IsFailure)
            return saveChangesResult.Errors;

        // Queue any domain events, to be published after the request has completed successfully.
        foreach (IDomainEvent domainEvent in record.GetDomainEvents())
            _domainEventsQueue.Enqueue(domainEvent);

        // The persisted row is read back, so the response carries the timestamps and version the persistence medium actually stored.
        Result<MediaConsumptionRecordEntity?> getPersistedRecordResult = await _unitOfWork.MediaConsumptionRecordRepository
            .GetByIdForUserAsync(record.Id.Value, userId, shouldTrackEntities: false, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (getPersistedRecordResult.IsFailure)
            return getPersistedRecordResult.Errors;
        if (getPersistedRecordResult.Value is null)
            return DomainErrors.MediaConsumption.ConsumptionRecordNotFound;

        return getPersistedRecordResult.Value.ToDto().ToResponse();
    }

    /// <summary>
    /// Converts reported position fields that have already been validated to a <see cref="MediaPosition"/>.
    /// </summary>
    /// <param name="positionKind">The name of the kind of the reported position, or <see langword="null"/> when no position was reported.</param>
    /// <param name="timeOffsetInSeconds">The reported position within the media item, in seconds, when the position is a time offset.</param>
    /// <param name="location">The reported opaque locator within the media item, when the position is not a time offset.</param>
    /// <returns>The converted position, or no position when the client did not report one.</returns>
    private static Optional<MediaPosition> ToMediaPosition(string? positionKind, double? timeOffsetInSeconds, string? location)
    {
        // A missing kind means the client did not report a position at all, which is different from a position at zero.
        if (string.IsNullOrWhiteSpace(positionKind))
            return Optional<MediaPosition>.None();

        // The validator already rejected unknown and undefined kinds, so the parse is safe here and no fallback value is invented.
        MediaPositionKind parsedPositionKind = Enum.Parse<MediaPositionKind>(positionKind, ignoreCase: true);
        // The kind names the single representation that the validator accepted, so only that representation is read.
        return parsedPositionKind == MediaPositionKind.TimeOffset
            ? Optional<MediaPosition>.Some(MediaPosition.CreateTimeOffset(TimeSpan.FromSeconds(timeOffsetInSeconds!.Value)))
            : Optional<MediaPosition>.Some(MediaPosition.CreateLocation(location!));
    }

    /// <summary>
    /// Creates or updates the resume point of <paramref name="record"/> for the current user.
    /// </summary>
    /// <param name="record">The consumption record whose session is being tracked.</param>
    /// <param name="position">The position at which the media item can be resumed.</param>
    /// <param name="progressFraction">The progress fraction at which the media item can be resumed.</param>
    /// <param name="userId">The Id of the user the resume point belongs to.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful save, or an error message.</returns>
    private async Task<Result<Updated>> UpsertResumePointAsync(
        MediaConsumptionRecord record, MediaPosition position, double progressFraction, Guid userId, CancellationToken cancellationToken)
    {
        // The resume point is read without tracking because it is only used to build the domain aggregate. The upsert call locates and updates
        // the stored row itself, so tracking this copy would put two tracked instances of the same row in the same unit of work.
        Result<MediaResumePointEntity?> getResumePointResult = await _unitOfWork.MediaResumePointRepository
            .GetForUserAndMediaItemAsync(userId, record.MediaKind, record.MediaItemId.Value, shouldTrackEntities: false, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (getResumePointResult.IsFailure)
            return getResumePointResult.Errors;

        MediaResumePoint resumePoint;
        if (getResumePointResult.Value is null)
        {
            // A resume point that does not exist yet is created from the session identifiers and the effective position. The position and
            // fraction are the ones the caller applied to the record, so the resume point and the record cannot disagree about the report.
            Result<MediaResumePoint> createResumePointResult = MediaResumePoint.Create(
                record.UserId, record.LibraryId, record.MediaItemId, record.MediaKind, position, progressFraction, record.LastActivityOnUtc);
            if (createResumePointResult.IsFailure)
                return createResumePointResult.Errors;
            resumePoint = createResumePointResult.Value;
        }
        else
        {
            // An existing resume point is advanced instead of replaced, so its identifier and creation time survive and its own rules validate
            // the new position and fraction before the update. The activity time is taken from the record, so both rows describe the same report.
            Result<MediaResumePoint> toDomainEntityResult = getResumePointResult.Value.ToDomainEntity();
            if (toDomainEntityResult.IsFailure)
                return toDomainEntityResult.Errors;
            Result<Updated> advanceResult = toDomainEntityResult.Value.Advance(position, progressFraction, record.LastActivityOnUtc);
            if (advanceResult.IsFailure)
                return advanceResult.Errors;
            resumePoint = toDomainEntityResult.Value;
        }

        // The upsert decides between the insert and the update from the unique business key, so a concurrent report that created the row
        // between the read above and this call still ends with one resume point instead of a duplicate.
        return await _unitOfWork.MediaResumePointRepository.UpsertForUserAsync(resumePoint.ToRepositoryEntity(), userId, cancellationToken).ConfigureAwait(false);
    }
}
