#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.DataAccess.Entities.Playlists;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.DomainEvents;
using Lumina.Application.Common.DTO.Playlists;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Mapping.Playlists;
using Lumina.Contracts.Responses.Playlists;
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

namespace Lumina.Application.Core.Playlists.Commands.UpdatePlaylist;

/// <summary>
/// Handler for the command to update the details of a playlist of the current user.
/// </summary>
public class UpdatePlaylistCommandHandler : ICommandHandler<UpdatePlaylistCommand, Result<PlaylistDetailsResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDomainEventsQueue _domainEventsQueue;
    private readonly IValidator<UpdatePlaylistCommand> _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdatePlaylistCommandHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    /// <param name="currentUserService">Injected service to retrieve the current user information.</param>
    /// <param name="domainEventsQueue">Injected service for the queue of domain events.</param>
    /// <param name="validator">Injected validator for application validation rules.</param>
    public UpdatePlaylistCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IDomainEventsQueue domainEventsQueue,
        IValidator<UpdatePlaylistCommand> validator)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _domainEventsQueue = domainEventsQueue;
        _validator = validator;
    }

    /// <summary>
    /// Handles the command to update the details of a playlist of the current user.
    /// </summary>
    /// <param name="command">The command to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either the updated <see cref="PlaylistResponse"/>, or an error message.
    /// </returns>
    public async Task<Result<PlaylistDetailsResponse>> HandleAsync(UpdatePlaylistCommand command, CancellationToken cancellationToken)
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

        // The playlist is scoped to the current user by the repository, so a playlist of another user is indistinguishable from a missing one.
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

        Result<Updated> updateDetailsResult = playlist.UpdateDetails(command.Name!, Optional<string>.FromNullable(command.Description));
        if (updateDetailsResult.IsFailure)
            return updateDetailsResult.Errors;

        Result<Updated> updateResult = await _unitOfWork.PlaylistRepository.UpdateAsync(playlist.ToRepositoryEntity(), cancellationToken).ConfigureAwait(false);
        if (updateResult.IsFailure)
            return updateResult.Errors;

        Result<Success> saveChangesResult = await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        if (saveChangesResult.IsFailure)
            return saveChangesResult.Errors;

        foreach (IDomainEvent domainEvent in playlist.GetDomainEvents())
            _domainEventsQueue.Enqueue(domainEvent);

        // A details update only changes the playlist scalars, so the response carries those scalars and the audit timestamp the interceptor actually stored.
        return new PlaylistDetailsResponse(
            playlist.Id.Value,
            playlist.Name,
            playlist.Description.HasValue ? playlist.Description.Value : null,
            playlist.Type,
            getPlaylistResult.Value.UpdatedOnUtc);
    }
}
