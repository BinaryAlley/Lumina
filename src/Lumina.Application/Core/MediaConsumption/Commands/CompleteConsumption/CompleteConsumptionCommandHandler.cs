#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.DataAccess.Entities.MediaConsumption;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.DomainEvents;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Mapping.MediaConsumption;
using Lumina.Contracts.Responses.MediaConsumption;
using Lumina.Domain.Common.Events;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.Common.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.MediaConsumptionRecordAggregate;
using Lumina.Domain.SharedKernel.Common.Enums.MediaConsumption;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
#endregion

namespace Lumina.Application.Core.MediaConsumption.Commands.CompleteConsumption;

/// <summary>
/// Handler for the command to report that a consumption session of the current user completed.
/// </summary>
public class CompleteConsumptionCommandHandler : ICommandHandler<CompleteConsumptionCommand, Result<ConsumptionRecordResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDomainEventsQueue _domainEventsQueue;
    private readonly IValidator<CompleteConsumptionCommand> _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="CompleteConsumptionCommandHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    /// <param name="currentUserService">Injected service to retrieve the current user information.</param>
    /// <param name="domainEventsQueue">Injected service for the queue of domain events.</param>
    /// <param name="validator">Injected validator for application validation rules.</param>
    public CompleteConsumptionCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IDomainEventsQueue domainEventsQueue,
        IValidator<CompleteConsumptionCommand> validator)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _domainEventsQueue = domainEventsQueue;
        _validator = validator;
    }

    /// <summary>
    /// Handles the command to report that a consumption session of the current user completed.
    /// </summary>
    /// <param name="command">The command to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either the completed <see cref="ConsumptionRecordResponse"/>, or an error message.
    /// </returns>
    public async Task<Result<ConsumptionRecordResponse>> HandleAsync(CompleteConsumptionCommand command, CancellationToken cancellationToken)
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
        Result<MediaConsumptionRecordEntity?> getRecordResult = await _unitOfWork.MediaConsumptionRecordRepository
            .GetByIdForUserAsync(consumptionRecordId, userId, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (getRecordResult.IsFailure)
            return getRecordResult.Errors;
        if (getRecordResult.Value is null)
            return DomainErrors.MediaConsumption.ConsumptionRecordNotFound;

        Result<MediaConsumptionRecord> toDomainEntityResult = getRecordResult.Value.ToDomainEntity();
        if (toDomainEntityResult.IsFailure)
            return toDomainEntityResult.Errors;
        MediaConsumptionRecord record = toDomainEntityResult.Value;

        Optional<MediaPosition> position = ToMediaPosition(command.PositionKind, command.PositionTimeOffsetInSeconds, command.PositionLocation);

        Result<Updated> completeResult = record.Complete(position, command.ProgressFraction!.Value, TimeSpan.FromSeconds(command.ConsumedDurationInSeconds!.Value));
        if (completeResult.IsFailure)
            return completeResult.Errors;

        // Completion discards the resume point, because the media item no longer has a point to continue from. A missing resume point is not an
        // error, so an explicit completion after a completion that already deleted it stays idempotent.
        Result<Deleted> deleteResumePointResult = await _unitOfWork.MediaResumePointRepository
            .DeleteForUserAndMediaItemAsync(userId, record.MediaKind, record.MediaItemId.Value, cancellationToken).ConfigureAwait(false);
        if (deleteResumePointResult.IsFailure)
            return deleteResumePointResult.Errors;

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
}
