#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.DataAccess.Entities.Playlists;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
#endregion

namespace Lumina.Application.Core.Playlists.Commands.AddPlaylistEntries;

/// <summary>
/// Handler for the command to add several tracks to a playlist of the current user, in order.
/// </summary>
public class AddPlaylistEntriesCommandHandler : ICommandHandler<AddPlaylistEntriesCommand, Result<PlaylistEntriesAddedResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuthorizationService _authorizationService;
    private readonly IValidator<AddPlaylistEntriesCommand> _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="AddPlaylistEntriesCommandHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    /// <param name="currentUserService">Injected service to retrieve the current user information.</param>
    /// <param name="authorizationService">Injected service for authorization related functionality.</param>
    /// <param name="validator">Injected validator for application validation rules.</param>
    public AddPlaylistEntriesCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IAuthorizationService authorizationService,
        IValidator<AddPlaylistEntriesCommand> validator)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _authorizationService = authorizationService;
        _validator = validator;
    }

    /// <summary>
    /// Handles the command to add several tracks to a playlist of the current user, in order.
    /// </summary>
    /// <param name="command">The command to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either the number of appended entries and the playlist activity time, or an error message.
    /// </returns>
    public async Task<Result<PlaylistEntriesAddedResponse>> HandleAsync(AddPlaylistEntriesCommand command, CancellationToken cancellationToken)
    {
        List<Error> validationResult = _validator.Validate(command);
        if (validationResult.Count > 0)
            return validationResult;

        // An authenticated request must always carry a user identity.
        Guid? currentUserId = _currentUserService.UserId;
        if (currentUserId is null)
            return ApplicationErrors.Authorization.NotAuthorized;
        Guid userId = currentUserId.Value;

        // The validator guarantees that the identifier is a non-empty Guid before this point.
        Guid playlistId = Guid.Parse(command.PlaylistId!);

        // A missing playlist is reported before any track is read.
        Result<PlaylistEntity?> getPlaylistResult = await _unitOfWork.PlaylistRepository
            .GetByIdForUserAsync(playlistId, userId, shouldIncludeNavigationProperties: false, shouldTrackEntities: false, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (getPlaylistResult.IsFailure)
            return getPlaylistResult.Errors;
        if (getPlaylistResult.Value is null)
            return DomainErrors.Playlists.PlaylistNotFound;

        // The referenced tracks are resolved in a few bounded chunks, grouped by library, so adding a long queue does not turn into one query per entry.
        // Duplicate track Ids are collapsed for the lookup, because the same track may legitimately appear more than once in a playlist.
        List<Guid> distinctTrackIds = [.. command.TrackIds!.Distinct()];
        Result<TrackAccessSummaryDto> getAccessInfoResult = await _unitOfWork.TrackRepository.GetAccessInfoByIdsAsync(distinctTrackIds, cancellationToken).ConfigureAwait(false);
        if (getAccessInfoResult.IsFailure)
            return getAccessInfoResult.Errors;
        if (getAccessInfoResult.Value.TrackCount != distinctTrackIds.Count)
            return DomainErrors.Music.TrackNotFound;

        // Every library that owns one of the referenced tracks must be reachable by the current user. The libraries are checked once each,
        // so a queue made of many tracks of the same library does not evaluate the policy once per track.
        foreach (Guid libraryId in getAccessInfoResult.Value.LibraryIds)
        {
            bool canAccessLibrary = await _authorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(
                userId, new LibraryOwnershipPolicyContext(libraryId), cancellationToken).ConfigureAwait(false);
            // A track whose library is not reachable is reported exactly like a missing track, so the response cannot be used to probe whether a track exists in another user's library.
            if (!canAccessLibrary)
                return DomainErrors.Music.TrackNotFound;
        }

        if (!Playlist.CanContain(getPlaylistResult.Value.Type, PlaylistMediaItemType.MusicTrack))
            return DomainErrors.Playlists.PlaylistDoesNotAcceptMediaItems;

        DateTime updatedOnUtc = DateTime.UtcNow;
        Result<PlaylistBulkAddResultDto> addEntriesResult = await _unitOfWork.PlaylistRepository
            .AddEntriesAsync(playlistId, userId, command.TrackIds!, PlaylistMediaItemType.MusicTrack, entryIds: null, updatedOnUtc, cancellationToken).ConfigureAwait(false);
        if (addEntriesResult.IsFailure)
            return addEntriesResult.Errors;

        return new PlaylistEntriesAddedResponse(
            playlistId,
            addEntriesResult.Value.AddedCount,
            updatedOnUtc);
    }
}
