#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Contracts.Responses.Playlists;
using Lumina.Domain.Common.Primitives;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
#endregion

namespace Lumina.Application.Core.Playlists.Commands.MovePlaylistEntry;

/// <summary>
/// Handler for the command to move one entry of a playlist of the current user to a new position.
/// </summary>
public class MovePlaylistEntryCommandHandler : ICommandHandler<MovePlaylistEntryCommand, Result<PlaylistEntryMovedResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IValidator<MovePlaylistEntryCommand> _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="MovePlaylistEntryCommandHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    /// <param name="currentUserService">Injected service to retrieve the current user information.</param>
    /// <param name="validator">Injected validator for application validation rules.</param>
    public MovePlaylistEntryCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IValidator<MovePlaylistEntryCommand> validator)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _validator = validator;
    }

    /// <summary>
    /// Handles the command to move one entry of a playlist of the current user to a new position.
    /// </summary>
    /// <param name="command">The command to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either the moved entry and its position, or an error message.
    /// </returns>
    public async Task<Result<PlaylistEntryMovedResponse>> HandleAsync(MovePlaylistEntryCommand command, CancellationToken cancellationToken)
    {
        List<Error> validationResult = _validator.Validate(command);
        if (validationResult.Count > 0)
            return validationResult;

        // An authenticated request must always carry a user identity.
        Guid? currentUserId = _currentUserService.UserId;
        if (currentUserId is null)
            return ApplicationErrors.Authorization.NotAuthorized;
        Guid userId = currentUserId.Value;

        // The validator guarantees that the route identifiers are non-empty Guids before this point.
        Guid playlistId = Guid.Parse(command.PlaylistId!);
        Guid playlistEntryId = Guid.Parse(command.PlaylistEntryId!);
        int targetPosition = command.TargetPosition!.Value;

        // The entry is moved directly in the storage medium, so a playlist of any size is not loaded into memory for one move.
        DateTime updatedOnUtc = DateTime.UtcNow;
        Result<Updated> moveEntryResult = await _unitOfWork.PlaylistRepository
            .MoveEntryAsync(playlistId, userId, playlistEntryId, targetPosition, updatedOnUtc, cancellationToken).ConfigureAwait(false);
        if (moveEntryResult.IsFailure)
            return moveEntryResult.Errors;

        return new PlaylistEntryMovedResponse(
            playlistId,
            playlistEntryId,
            targetPosition,
            updatedOnUtc);
    }
}
