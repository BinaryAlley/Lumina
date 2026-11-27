#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.DataAccess.Entities.Favorites;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.DomainEvents;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Mapping.Favorites;
using Lumina.Domain.Common.Events;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.FavoriteBoundedContext.FavoriteAggregate;
using Lumina.Domain.SharedKernel.Common.Enums.Favorites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
#endregion

namespace Lumina.Application.Core.Favorites.Commands.RemoveFavorite;

/// <summary>
/// Handler for the command to remove a media item from the favorites of the current user.
/// </summary>
public class RemoveFavoriteCommandHandler : ICommandHandler<RemoveFavoriteCommand, Result<Deleted>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDomainEventsQueue _domainEventsQueue;
    private readonly IValidator<RemoveFavoriteCommand> _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="RemoveFavoriteCommandHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    /// <param name="currentUserService">Injected service to retrieve the current user information.</param>
    /// <param name="domainEventsQueue">Injected service for the queue of domain events.</param>
    /// <param name="validator">Injected validator for application validation rules.</param>
    public RemoveFavoriteCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IDomainEventsQueue domainEventsQueue,
        IValidator<RemoveFavoriteCommand> validator)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _domainEventsQueue = domainEventsQueue;
        _validator = validator;
    }

    /// <summary>
    /// Handles the command to remove a media item from the favorites of the current user.
    /// </summary>
    /// <param name="command">The command to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<Result<Deleted>> HandleAsync(RemoveFavoriteCommand command, CancellationToken cancellationToken)
    {
        List<Error> validationResult = _validator.Validate(command);
        if (validationResult.Count > 0)
            return validationResult;

        // An authenticated request must always carry a user identity.
        Guid? currentUserId = _currentUserService.UserId;
        if (currentUserId is null)
            return ApplicationErrors.Authorization.NotAuthorized;
        Guid userId = currentUserId.Value;

        // The validator guarantees that the route values are a defined media kind name and a non-empty Guid before this point.
        FavoriteMediaKind mediaKind = Enum.Parse<FavoriteMediaKind>(command.MediaKind!, ignoreCase: true);
        Guid mediaItemId = Guid.Parse(command.MediaItemId!);

        // The favorite is loaded by the current user, the kind and the item, so another user's favorite is never touched. It is loaded so that its
        // aggregate can raise the removal event before the row is deleted. The library of the item is deliberately not evaluated, so a user can
        // always remove their own favorite whose item was deleted or is no longer accessible.
        Result<FavoriteEntity?> getFavoriteResult = await _unitOfWork.FavoriteRepository
            .GetByMediaItemIdForUserAsync(userId, mediaKind, mediaItemId, shouldTrackEntities: true, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (getFavoriteResult.IsFailure)
            return getFavoriteResult.Errors;
        // Removing a favorite that is already absent succeeds, so a retry, or another tab that removed it first, does not turn the
        // caller's removal into an error. Nothing is deleted and no event is raised because this request did not remove anything.
        if (getFavoriteResult.Value is null)
            return Result.Deleted;

        Result<Favorite> toDomainEntityResult = getFavoriteResult.Value.ToDomainEntity();
        if (toDomainEntityResult.IsFailure)
            return toDomainEntityResult.Errors;
        Favorite favorite = toDomainEntityResult.Value;

        Result<Deleted> removeResult = favorite.Remove();
        if (removeResult.IsFailure)
            return removeResult.Errors;

        Result<Deleted> deleteFavoriteResult = await _unitOfWork.FavoriteRepository.DeleteByIdAsync(favorite.Id.Value, cancellationToken).ConfigureAwait(false);
        if (deleteFavoriteResult.IsFailure)
            return deleteFavoriteResult.Errors;

        Result<Success> saveChangesResult = await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        if (saveChangesResult.IsFailure)
        {
            // A concurrent request removed the same favorite between the read above and this write, so the row this request tried to delete is already gone. The
            // desired state already holds, so the request succeeds, without announcing a removal that this request did not perform.
            if (!saveChangesResult.Errors.Any(error => error == ApplicationErrors.Persistence.ConcurrencyConflict))
                return saveChangesResult.Errors;

            _unitOfWork.ClearTrackedEntities();
            Result<FavoriteEntity?> getRemainingFavoriteResult = await _unitOfWork.FavoriteRepository
                .GetByMediaItemIdForUserAsync(userId, mediaKind, mediaItemId, shouldTrackEntities: false, cancellationToken: cancellationToken).ConfigureAwait(false);
            if (getRemainingFavoriteResult.IsFailure)
                return getRemainingFavoriteResult.Errors;
            if (getRemainingFavoriteResult.Value is not null)
                return saveChangesResult.Errors;
            return Result.Deleted;
        }

        // Queue any domain events, to be published after the request has completed successfully.
        foreach (IDomainEvent domainEvent in favorite.GetDomainEvents())
            _domainEventsQueue.Enqueue(domainEvent);

        return Result.Deleted;
    }
}
