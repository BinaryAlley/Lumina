#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.DataAccess.Entities.Playlists;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.DTO.Playlists;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.LibraryOwnership;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Mapping.Playlists;
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

namespace Lumina.Application.Core.Playlists.Commands.AddPlaylistEntry;

/// <summary>
/// Handler for the command to add a track to a playlist of the current user.
/// </summary>
public class AddPlaylistEntryCommandHandler : ICommandHandler<AddPlaylistEntryCommand, Result<PlaylistEntryAddedResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuthorizationService _authorizationService;
    private readonly IValidator<AddPlaylistEntryCommand> _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="AddPlaylistEntryCommandHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    /// <param name="currentUserService">Injected service to retrieve the current user information.</param>
    /// <param name="authorizationService">Injected service for authorization related functionality.</param>
    /// <param name="validator">Injected validator for application validation rules.</param>
    public AddPlaylistEntryCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IAuthorizationService authorizationService,
        IValidator<AddPlaylistEntryCommand> validator)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _authorizationService = authorizationService;
        _validator = validator;
    }

    /// <summary>
    /// Handles the command to add a track to a playlist of the current user.
    /// </summary>
    /// <param name="command">The command to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either the appended entry and the playlist activity time, or an error message.
    /// </returns>
    public async Task<Result<PlaylistEntryAddedResponse>> HandleAsync(AddPlaylistEntryCommand command, CancellationToken cancellationToken)
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
        Guid trackId = command.TrackId!.Value;

        // A missing playlist is reported before the track is read.
        Result<PlaylistEntity?> getPlaylistResult = await _unitOfWork.PlaylistRepository
            .GetByIdForUserAsync(playlistId, userId, shouldIncludeNavigationProperties: false, shouldTrackEntities: false, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (getPlaylistResult.IsFailure)
            return getPlaylistResult.Errors;
        if (getPlaylistResult.Value is null)
            return DomainErrors.Playlists.PlaylistNotFound;

        // The track must exist, and its library must be reachable by the current user, so another user's library cannot be used as a source.
        Result<TrackEntity?> getTrackResult = await _unitOfWork.TrackRepository
            .GetByIdAsync(trackId, shouldIncludeNavigationProperties: false, shouldTrackEntities: false, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (getTrackResult.IsFailure)
            return getTrackResult.Errors;
        if (getTrackResult.Value is null)
            return DomainErrors.Music.TrackNotFound;

        bool canAccessLibrary = await _authorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(
            userId, new LibraryOwnershipPolicyContext(getTrackResult.Value.LibraryId), cancellationToken).ConfigureAwait(false);
        // A track whose library is not reachable is reported exactly like a missing track, so the response cannot be used to probe whether a track exists in another user's library.
        if (!canAccessLibrary)
            return DomainErrors.Music.TrackNotFound;

        if (!Playlist.CanContain(getPlaylistResult.Value.Type, PlaylistMediaItemType.MusicTrack))
            return DomainErrors.Playlists.PlaylistDoesNotAcceptMediaItems;

        Guid entryId = Guid.NewGuid();
        DateTime updatedOnUtc = DateTime.UtcNow;
        Result<PlaylistBulkAddResultDto> addEntryResult = await _unitOfWork.PlaylistRepository
            .AddEntriesAsync(playlistId, userId, [trackId], PlaylistMediaItemType.MusicTrack, [entryId], updatedOnUtc, cancellationToken).ConfigureAwait(false);
        if (addEntryResult.IsFailure)
            return addEntryResult.Errors;

        // Only the appended entry is read back, with the display details of its track, so a single add does not return the whole playlist.
        // The appended entry is the last one, so the read-back derives its display position from the stored entry count.
        Result<PlaylistEntryDto?> getEntryResult = await _unitOfWork.PlaylistRepository.GetAppendedEntryDetailsForUserAsync(playlistId, userId, entryId, cancellationToken).ConfigureAwait(false);
        if (getEntryResult.IsFailure)
            return getEntryResult.Errors;
        if (getEntryResult.Value is null)
            return DomainErrors.Playlists.PlaylistEntryNotFound;

        return new PlaylistEntryAddedResponse(
            playlistId,
            getEntryResult.Value.ToResponse(),
            updatedOnUtc);
    }
}
