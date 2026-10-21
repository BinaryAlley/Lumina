#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.DataAccess.Entities.Playlists;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.DTO.Playlists;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.LibraryOwnership;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Contracts.Responses.Playlists;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.PlaylistBoundedContext.PlaylistAggregate;
using Lumina.Domain.SharedKernel.Common.Enums.Playlists;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
#endregion

namespace Lumina.Application.Core.Playlists.Commands.AddPlaylistScopeEntries;

/// <summary>
/// Handler for the command to add every track of a music library, an artist or an album to a playlist.
/// </summary>
public class AddPlaylistScopeEntriesCommandHandler : ICommandHandler<AddPlaylistScopeEntriesCommand, Result<PlaylistEntriesAddedResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuthorizationService _authorizationService;
    private readonly IValidator<AddPlaylistScopeEntriesCommand> _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="AddPlaylistScopeEntriesCommandHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    /// <param name="currentUserService">Injected service to retrieve the current user information.</param>
    /// <param name="authorizationService">Injected service for authorization related functionality.</param>
    /// <param name="validator">Injected validator for application validation rules.</param>
    public AddPlaylistScopeEntriesCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IAuthorizationService authorizationService,
        IValidator<AddPlaylistScopeEntriesCommand> validator)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _authorizationService = authorizationService;
        _validator = validator;
    }

    /// <summary>
    /// Handles the command to add every track of a music library, an artist or an album to a playlist.
    /// </summary>
    /// <param name="command">The command to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either the number of appended entries and the playlist activity time, or an error message.
    /// </returns>
    public async Task<Result<PlaylistEntriesAddedResponse>> HandleAsync(AddPlaylistScopeEntriesCommand command, CancellationToken cancellationToken)
    {
        List<Error> validationResult = _validator.Validate(command);
        if (validationResult.Count > 0)
            return validationResult;

        // An authenticated request must always carry a user identity.
        Guid? currentUserId = _currentUserService.UserId;
        if (currentUserId is null)
            return ApplicationErrors.Authorization.NotAuthorized;
        Guid userId = currentUserId.Value;

        // The validator guarantees that the identifiers are non-empty Guids before this point.
        Guid playlistId = Guid.Parse(command.PlaylistId!);
        Guid libraryId = command.LibraryId!.Value;

        // A missing playlist is reported before a whole library is resolved.
        Result<PlaylistEntity?> getPlaylistResult = await _unitOfWork.PlaylistRepository
            .GetByIdForUserAsync(playlistId, userId, shouldIncludeNavigationProperties: false, shouldTrackEntities: false, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (getPlaylistResult.IsFailure)
            return getPlaylistResult.Errors;
        if (getPlaylistResult.Value is null)
            return DomainErrors.Playlists.PlaylistNotFound;

        // The whole scope belongs to one library, so access is evaluated once for the entire scope.
        bool canAccessLibrary = await _authorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(
            userId, new LibraryOwnershipPolicyContext(libraryId), cancellationToken).ConfigureAwait(false);
        if (!canAccessLibrary)
            return ApplicationErrors.Authorization.NotAuthorized;

        if (!Playlist.CanContain(getPlaylistResult.Value.Type, PlaylistMediaItemType.MusicTrack))
            return DomainErrors.Playlists.PlaylistDoesNotAcceptMediaItems;

        // The repository resolves the scope and appends it in one set-based statement, so a whole library of any size is appended without
        // materializing its track Ids, sorting them in memory, or constructing one domain event per track.
        DateTime updatedOnUtc = DateTime.UtcNow;
        Result<PlaylistBulkAddResultDto> addEntriesResult = await _unitOfWork.PlaylistRepository
            .AddScopeEntriesAsync(playlistId, userId, libraryId, command.ArtistId, command.AlbumId, PlaylistMediaItemType.MusicTrack, updatedOnUtc, cancellationToken).ConfigureAwait(false);
        if (addEntriesResult.IsFailure)
            return addEntriesResult.Errors;

        return new PlaylistEntriesAddedResponse(
            playlistId,
            addEntriesResult.Value.AddedCount,
            addEntriesResult.Value.AddedCount == 0 ? null : updatedOnUtc);
    }
}
