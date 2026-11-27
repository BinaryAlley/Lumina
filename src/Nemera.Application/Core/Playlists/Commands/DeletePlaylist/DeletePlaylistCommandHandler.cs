#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.DataAccess.Entities.Playlists;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.DomainEvents;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Mapping.Playlists;
using Lumina.Domain.Common.Events;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.PlaylistBoundedContext.PlaylistAggregate;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
#endregion

namespace Lumina.Application.Core.Playlists.Commands.DeletePlaylist;

/// <summary>
/// Handler for the command to delete a playlist of the current user.
/// </summary>
public class DeletePlaylistCommandHandler : ICommandHandler<DeletePlaylistCommand, Result<Deleted>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDomainEventsQueue _domainEventsQueue;
    private readonly IValidator<DeletePlaylistCommand> _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeletePlaylistCommandHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    /// <param name="currentUserService">Injected service to retrieve the current user information.</param>
    /// <param name="domainEventsQueue">Injected service for the queue of domain events.</param>
    /// <param name="validator">Injected validator for application validation rules.</param>
    public DeletePlaylistCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IDomainEventsQueue domainEventsQueue,
        IValidator<DeletePlaylistCommand> validator)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _domainEventsQueue = domainEventsQueue;
        _validator = validator;
    }

    /// <summary>
    /// Handles the command to delete a playlist of the current user.
    /// </summary>
    /// <param name="command">The command to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<Result<Deleted>> HandleAsync(DeletePlaylistCommand command, CancellationToken cancellationToken)
    {
        List<Error> validationResult = _validator.Validate(command);
        if (validationResult.Count > 0)
            return validationResult;

        // An authenticated request must always carry a user identity.
        Guid? currentUserId = _currentUserService.UserId;
        if (currentUserId is null)
            return ApplicationErrors.Authorization.NotAuthorized;
        Guid userId = currentUserId.Value;

        // The validator guarantees that the route identifier is a non-empty Guid before this point.
        Guid playlistId = Guid.Parse(command.PlaylistId!);

        // The playlist is loaded so that its aggregate can raise the deletion event before the row is removed.
        Result<PlaylistEntity?> getPlaylistResult = await _unitOfWork.PlaylistRepository
            .GetByIdForUserAsync(playlistId, userId, shouldIncludeNavigationProperties: false, shouldTrackEntities: true, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (getPlaylistResult.IsFailure)
            return getPlaylistResult.Errors;
        if (getPlaylistResult.Value is null)
            return DomainErrors.Playlists.PlaylistNotFound;

        Result<Playlist> toDomainEntityResult = getPlaylistResult.Value.ToDomainEntity();
        if (toDomainEntityResult.IsFailure)
            return toDomainEntityResult.Errors;
        Playlist playlist = toDomainEntityResult.Value;

        Result<Deleted> deleteResult = playlist.Delete();
        if (deleteResult.IsFailure)
            return deleteResult.Errors;

        Result<Deleted> deletePlaylistResult = await _unitOfWork.PlaylistRepository.DeleteByIdAsync(playlist.Id.Value, cancellationToken).ConfigureAwait(false);
        if (deletePlaylistResult.IsFailure)
            return deletePlaylistResult.Errors;

        Result<Success> saveChangesResult = await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        if (saveChangesResult.IsFailure)
            return saveChangesResult.Errors;

        foreach (IDomainEvent domainEvent in playlist.GetDomainEvents())
            _domainEventsQueue.Enqueue(domainEvent);

        return Result.Deleted;
    }
}
