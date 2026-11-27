#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.CQRS;
using Nemera.Application.Common.DataAccess.UoW;
using Nemera.Application.Common.Infrastructure.Authentication;
using Nemera.Application.Common.Infrastructure.Validation;
using Nemera.Contracts.Responses.Playlists;
using Nemera.Domain.Common.Primitives;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Nemera.Application.Common.Errors.Errors;
#endregion

namespace Nemera.Application.Core.Playlists.Commands.RemovePlaylistEntry;

/// <summary>
/// Handler for the command to remove an entry from a playlist of the current user.
/// </summary>
public class RemovePlaylistEntryCommandHandler : ICommandHandler<RemovePlaylistEntryCommand, Result<PlaylistEntryRemovedResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IValidator<RemovePlaylistEntryCommand> _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="RemovePlaylistEntryCommandHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    /// <param name="currentUserService">Injected service to retrieve the current user information.</param>
    /// <param name="validator">Injected validator for application validation rules.</param>
    public RemovePlaylistEntryCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IValidator<RemovePlaylistEntryCommand> validator)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _validator = validator;
    }

    /// <summary>
    /// Handles the command to remove an entry from a playlist of the current user.
    /// </summary>
    /// <param name="command">The command to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either the removed entry with zero repositioned entries, or an error message.
    /// </returns>
    public async Task<Result<PlaylistEntryRemovedResponse>> HandleAsync(RemovePlaylistEntryCommand command, CancellationToken cancellationToken)
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

        // The row is deleted and the gap is closed in set-based statements, so a playlist of any size is not loaded into memory for one removal.
        DateTime updatedOnUtc = DateTime.UtcNow;
        Result<int> removeEntryResult = await _unitOfWork.PlaylistRepository.RemoveEntryAsync(playlistId, userId, playlistEntryId, updatedOnUtc, cancellationToken).ConfigureAwait(false);
        if (removeEntryResult.IsFailure)
            return removeEntryResult.Errors;

        return new PlaylistEntryRemovedResponse(
            playlistId,
            playlistEntryId,
            removeEntryResult.Value,
            updatedOnUtc);
    }
}
