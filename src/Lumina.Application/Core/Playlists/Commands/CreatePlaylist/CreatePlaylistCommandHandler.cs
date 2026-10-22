#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.DomainEvents;
using Lumina.Application.Common.DTO.Pagination;
using Lumina.Application.Common.DTO.Playlists;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Mapping.Playlists;
using Lumina.Contracts.Responses.Playlists;
using Lumina.Domain.Common.Events;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.PlaylistBoundedContext.ExternalIdentifiers.UserManagementBoundedContext.UserAggregate;
using Lumina.Domain.Core.BoundedContexts.PlaylistBoundedContext.PlaylistAggregate;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
#endregion

namespace Lumina.Application.Core.Playlists.Commands.CreatePlaylist;

/// <summary>
/// Handler for the command to create a playlist of the current user.
/// </summary>
public class CreatePlaylistCommandHandler : ICommandHandler<CreatePlaylistCommand, Result<PlaylistResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDomainEventsQueue _domainEventsQueue;
    private readonly IValidator<CreatePlaylistCommand> _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreatePlaylistCommandHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    /// <param name="currentUserService">Injected service to retrieve the current user information.</param>
    /// <param name="domainEventsQueue">Injected service for the queue of domain events.</param>
    /// <param name="validator">Injected validator for application validation rules.</param>
    public CreatePlaylistCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IDomainEventsQueue domainEventsQueue,
        IValidator<CreatePlaylistCommand> validator)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _domainEventsQueue = domainEventsQueue;
        _validator = validator;
    }

    /// <summary>
    /// Handles the command to create a playlist of the current user.
    /// </summary>
    /// <param name="command">The command to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either the created <see cref="PlaylistResponse"/>, or an error message.
    /// </returns>
    public async Task<Result<PlaylistResponse>> HandleAsync(CreatePlaylistCommand command, CancellationToken cancellationToken)
    {
        List<Error> validationResult = _validator.Validate(command);
        if (validationResult.Count > 0)
            return validationResult;

        // An authenticated request must always carry a user identity.
        Guid? currentUserId = _currentUserService.UserId;
        if (currentUserId is null)
            return ApplicationErrors.Authorization.NotAuthorized;
        Guid userId = currentUserId.Value;

        Result<Playlist> createPlaylistResult = Playlist.Create(
            UserId.Create(userId),
            command.Name!,
            Optional<string>.FromNullable(command.Description),
            command.Type!.Value);
        if (createPlaylistResult.IsFailure)
            return createPlaylistResult.Errors;
        Playlist playlist = createPlaylistResult.Value;

        Result<Created> insertResult = await _unitOfWork.PlaylistRepository.InsertAsync(playlist.ToRepositoryEntity(), cancellationToken).ConfigureAwait(false);
        if (insertResult.IsFailure)
            return insertResult.Errors;

        Result<Success> saveChangesResult = await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        if (saveChangesResult.IsFailure)
            return saveChangesResult.Errors;

        // Queue any domain events, to be published after the request has completed successfully.
        foreach (IDomainEvent domainEvent in playlist.GetDomainEvents())
            _domainEventsQueue.Enqueue(domainEvent);

        // The persisted row is read back, so the response carries the timestamps the auditing interceptor actually stored.
        Result<PlaylistDetailsDto?> getDetailsResult = await _unitOfWork.PlaylistRepository
            .GetDetailsForUserAsync(playlist.Id.Value, userId, new PaginationDataDto(), cancellationToken).ConfigureAwait(false);
        if (getDetailsResult.IsFailure)
            return getDetailsResult.Errors;
        if (getDetailsResult.Value is null)
            return DomainErrors.Playlists.PlaylistNotFound;

        return getDetailsResult.Value.ToResponse();
    }
}
