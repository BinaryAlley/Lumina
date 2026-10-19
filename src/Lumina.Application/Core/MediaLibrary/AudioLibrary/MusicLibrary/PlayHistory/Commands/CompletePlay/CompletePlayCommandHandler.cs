#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.DomainEvents;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.PlayHistory;
using Lumina.Domain.Common.Events;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.MusicPlayHistoryBoundedContext.MusicPlayRecordAggregate;
using Lumina.Domain.Core.BoundedContexts.MusicPlayHistoryBoundedContext.MusicPlayRecordAggregate.ValueObjects;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
#endregion

namespace Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.PlayHistory.Commands.CompletePlay;

/// <summary>
/// Handler for the command to report that a play of a track by the current user reached its end.
/// </summary>
public class CompletePlayCommandHandler : ICommandHandler<CompletePlayCommand, Result<Updated>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDomainEventsQueue _domainEventsQueue;
    private readonly IValidator<CompletePlayCommand> _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="CompletePlayCommandHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    /// <param name="currentUserService">Injected service to retrieve the current user information.</param>
    /// <param name="domainEventsQueue">Injected service for the queue of domain events.</param>
    /// <param name="validator">Injected validator for application validation rules.</param>
    public CompletePlayCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IDomainEventsQueue domainEventsQueue,
        IValidator<CompletePlayCommand> validator)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _domainEventsQueue = domainEventsQueue;
        _validator = validator;
    }

    /// <summary>
    /// Handles the command to report that a play of a track by the current user reached its end.
    /// </summary>
    /// <param name="command">The command to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful update, or an error message.</returns>
    public async Task<Result<Updated>> HandleAsync(CompletePlayCommand command, CancellationToken cancellationToken)
    {
        List<Error> validationResult = _validator.Validate(command);
        if (validationResult.Count > 0)
            return validationResult;

        // An authenticated request must always carry a user identity.
        Guid? currentUserId = _currentUserService.UserId;
        if (currentUserId is null)
            return ApplicationErrors.Authorization.NotAuthorized;
        Guid userId = currentUserId.Value;

        // The validator guarantees that the route identifier and the reported times are valid before this point.
        Guid playRecordId = Guid.Parse(command.PlayRecordId!);

        // The record is loaded scoped to the current user, so a record of another user is indistinguishable from a missing one.
        Result<MusicPlayRecordEntity?> getPlayRecordResult = await _unitOfWork.MusicPlayRecordRepository
            .GetByIdForUserAsync(playRecordId, userId, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (getPlayRecordResult.IsFailure)
            return getPlayRecordResult.Errors;
        if (getPlayRecordResult.Value is null)
            return DomainErrors.MusicPlayHistory.MusicPlayRecordNotFound;

        Result<MusicPlayRecord> toDomainEntityResult = getPlayRecordResult.Value.ToDomainEntity();
        if (toDomainEntityResult.IsFailure)
            return toDomainEntityResult.Errors;
        MusicPlayRecord playRecord = toDomainEntityResult.Value;

        Result<Updated> completeResult = playRecord.Complete(
            MusicPlayPosition.Create(TimeSpan.FromSeconds(command.PositionInSeconds!.Value)),
            TimeSpan.FromSeconds(command.ListenedDurationInSeconds!.Value));
        if (completeResult.IsFailure)
            return completeResult.Errors;

        Result<Updated> updateResult = await _unitOfWork.MusicPlayRecordRepository
            .UpdateForUserAsync(playRecord.ToRepositoryEntity(), userId, cancellationToken).ConfigureAwait(false);
        if (updateResult.IsFailure)
            return updateResult.Errors;

        Result<Success> saveChangesResult = await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        if (saveChangesResult.IsFailure)
            return saveChangesResult.Errors;

        // Queue any domain events, to be published after the request has completed successfully.
        foreach (IDomainEvent domainEvent in playRecord.GetDomainEvents())
            _domainEventsQueue.Enqueue(domainEvent);

        return Result.Updated;
    }
}
