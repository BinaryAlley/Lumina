#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.DataAccess.Entities.Favorites;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.DomainEvents;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.LibraryOwnership;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Mapping.Favorites;
using Lumina.Application.Common.Utilities;
using Lumina.Contracts.Responses.Favorites;
using Lumina.Domain.Common.Events;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.FavoriteBoundedContext.ExternalIdentifiers.MediaLibraryBoundedContext.MediaItemAggregate;
using Lumina.Domain.Core.BoundedContexts.FavoriteBoundedContext.ExternalIdentifiers.UserManagementBoundedContext.UserAggregate;
using Lumina.Domain.Core.BoundedContexts.FavoriteBoundedContext.FavoriteAggregate;
using Lumina.Domain.SharedKernel.Common.Enums.Favorites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
#endregion

namespace Lumina.Application.Core.Favorites.Commands.AddFavorite;

/// <summary>
/// Handler for the command to mark a media item as a favorite of the current user.
/// </summary>
public class AddFavoriteCommandHandler : ICommandHandler<AddFavoriteCommand, Result<FavoriteResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuthorizationService _authorizationService;
    private readonly IDomainEventsQueue _domainEventsQueue;
    private readonly IValidator<AddFavoriteCommand> _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="AddFavoriteCommandHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    /// <param name="currentUserService">Injected service to retrieve the current user information.</param>
    /// <param name="authorizationService">Injected service for authorization related functionality.</param>
    /// <param name="domainEventsQueue">Injected service for the queue of domain events.</param>
    /// <param name="validator">Injected validator for application validation rules.</param>
    public AddFavoriteCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IAuthorizationService authorizationService,
        IDomainEventsQueue domainEventsQueue,
        IValidator<AddFavoriteCommand> validator)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _authorizationService = authorizationService;
        _domainEventsQueue = domainEventsQueue;
        _validator = validator;
    }

    /// <summary>
    /// Handles the command to mark a media item as a favorite of the current user.
    /// </summary>
    /// <param name="command">The command to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either the favorite of the current user on the media item, or an error message.
    /// </returns>
    public async Task<Result<FavoriteResponse>> HandleAsync(AddFavoriteCommand command, CancellationToken cancellationToken)
    {
        List<Error> validationResult = _validator.Validate(command);
        if (validationResult.Count > 0)
            return validationResult;

        // An authenticated request must always carry a user identity.
        Guid? currentUserId = _currentUserService.UserId;
        if (currentUserId is null)
            return ApplicationErrors.Authorization.NotAuthorized;
        Guid userId = currentUserId.Value;

        // The validator guarantees that the media kind is a defined value and the media item identifier is a non-empty Guid before this point.
        FavoriteMediaKind mediaKind = command.MediaKind!.Value;
        Guid mediaItemId = command.MediaItemId!.Value;

        // A planned media kind has no catalogued library to look the item up in, so it is rejected before any lookup.
        if (!Favorite.IsSupported(mediaKind))
            return DomainErrors.Favorites.FavoriteMediaKindIsNotSupported;

        // The item must exist, and its library must be reachable by the current user, so another user's library cannot be probed through favorites.
        Result<Guid?> getLibraryIdResult = await _unitOfWork.FavoriteRepository.GetMediaItemLibraryIdAsync(mediaKind, mediaItemId, cancellationToken).ConfigureAwait(false);
        if (getLibraryIdResult.IsFailure)
            return getLibraryIdResult.Errors;
        if (getLibraryIdResult.Value is null)
            return mediaKind.ToMediaItemNotFoundError();

        // An item whose library is not reachable is reported exactly like a missing item, so the response cannot be used to probe whether an item exists in another user's library.
        bool canAccessLibrary = await _authorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(
            userId, new LibraryOwnershipPolicyContext(getLibraryIdResult.Value.Value), cancellationToken).ConfigureAwait(false);
        if (!canAccessLibrary)
            return mediaKind.ToMediaItemNotFoundError();

        // Adding an item that is already a favorite is idempotent: the existing favorite is returned and no event is raised.
        Result<FavoriteEntity?> getExistingFavoriteResult = await _unitOfWork.FavoriteRepository
            .GetByMediaItemIdForUserAsync(userId, mediaKind, mediaItemId, shouldTrackEntities: false, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (getExistingFavoriteResult.IsFailure)
            return getExistingFavoriteResult.Errors;
        if (getExistingFavoriteResult.Value is not null)
            return getExistingFavoriteResult.Value.ToResponse();

        Result<Favorite> createFavoriteResult = Favorite.Create(UserId.Create(userId), MediaItemId.Create(mediaItemId), mediaKind);
        if (createFavoriteResult.IsFailure)
            return createFavoriteResult.Errors;
        Favorite favorite = createFavoriteResult.Value;

        Result<Created> insertResult = await _unitOfWork.FavoriteRepository.InsertAsync(favorite.ToRepositoryEntity(), cancellationToken).ConfigureAwait(false);
        if (insertResult.IsFailure)
            return insertResult.Errors;

        Result<Success> saveChangesResult = await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        if (saveChangesResult.IsFailure)
        {
            // A concurrent request favorited the same item between the read above and this write, and the unique index rejected this one. The
            // desired state already holds, so the request succeeds with the stored favorite, without announcing a favorite that this request did not create.
            if (!saveChangesResult.Errors.Any(error => error == ApplicationErrors.Persistence.UniqueConstraintViolation))
                return saveChangesResult.Errors;

            _unitOfWork.ClearTrackedEntities();
            Result<FavoriteEntity?> getConcurrentFavoriteResult = await _unitOfWork.FavoriteRepository
                .GetByMediaItemIdForUserAsync(userId, mediaKind, mediaItemId, shouldTrackEntities: false, cancellationToken: cancellationToken).ConfigureAwait(false);
            if (getConcurrentFavoriteResult.IsFailure)
                return getConcurrentFavoriteResult.Errors;
            if (getConcurrentFavoriteResult.Value is null)
                return saveChangesResult.Errors;
            return getConcurrentFavoriteResult.Value.ToResponse();
        }

        // The persisted row is read back, so the response carries the timestamps the auditing interceptor actually stored.
        Result<FavoriteEntity?> getFavoriteResult = await _unitOfWork.FavoriteRepository
            .GetByMediaItemIdForUserAsync(userId, mediaKind, mediaItemId, shouldTrackEntities: false, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (getFavoriteResult.IsFailure)
            return getFavoriteResult.Errors;
        if (getFavoriteResult.Value is null)
            return DomainErrors.Favorites.FavoriteNotFound;

        // Queue any domain events, to be published after the request has completed successfully. The events are queued only after the persisted
        // row has been read back, so a failed read cannot announce a favorite that the request reports as failed.
        foreach (IDomainEvent domainEvent in favorite.GetDomainEvents())
            _domainEventsQueue.Enqueue(domainEvent);

        return getFavoriteResult.Value.ToResponse();
    }
}
