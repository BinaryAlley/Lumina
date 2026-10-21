#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.DTO.Playlists;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.LibraryOwnership;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Mapping.Playlists;
using Lumina.Contracts.Responses.Playlists;
using Lumina.Domain.Common.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
#endregion

namespace Lumina.Application.Core.Playlists.Queries.GetPlaylist;

/// <summary>
/// Handler for the query to get a playlist of the current user, together with its ordered entries.
/// </summary>
public class GetPlaylistQueryHandler : IQueryHandler<GetPlaylistQuery, Result<PlaylistResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuthorizationService _authorizationService;
    private readonly IValidator<GetPlaylistQuery> _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPlaylistQueryHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    /// <param name="currentUserService">Injected service to retrieve the current user information.</param>
    /// <param name="authorizationService">Injected service for authorization related functionality.</param>
    /// <param name="validator">Injected validator for application validation rules.</param>
    public GetPlaylistQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, IAuthorizationService authorizationService, IValidator<GetPlaylistQuery> validator)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _authorizationService = authorizationService;
        _validator = validator;
    }

    /// <summary>
    /// Handles the query to get a playlist of the current user, together with its ordered entries.
    /// </summary>
    /// <param name="query">The query to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either the requested <see cref="PlaylistResponse"/>, or an error message.
    /// </returns>
    public async Task<Result<PlaylistResponse>> HandleAsync(GetPlaylistQuery query, CancellationToken cancellationToken)
    {
        List<Error> validationResult = _validator.Validate(query);
        if (validationResult.Count > 0)
            return validationResult;

        // An authenticated request must always carry a user identity.
        Guid? currentUserId = _currentUserService.UserId;
        if (currentUserId is null)
            return ApplicationErrors.Authorization.NotAuthorized;
        Guid userId = currentUserId.Value;

        // The validator guarantees that the route identifier is a non-empty Guid before this point.
        Guid playlistId = Guid.Parse(query.PlaylistId!);

        // The playlist is scoped to the current user by the repository, so a playlist of another user is indistinguishable from a missing one.
        Result<PlaylistDetailsDto?> getPlaylistResult = await _unitOfWork.PlaylistRepository
            .GetDetailsForUserAsync(playlistId, userId, query.Offset, query.Limit, cancellationToken).ConfigureAwait(false);
        if (getPlaylistResult.IsFailure)
            return getPlaylistResult.Errors;
        if (getPlaylistResult.Value is null)
            return DomainErrors.Playlists.PlaylistNotFound;

        // Check the access permissions for each library that the tracks in the playlist might belong to.
        PlaylistDetailsDto playlist = getPlaylistResult.Value;
        List<Guid> libraryIds = [.. playlist.Entries
            .Select(entry => entry.Display?.LibraryId ?? Guid.Empty)
            .Where(libraryId => libraryId != Guid.Empty)
            .Distinct()];
        Dictionary<Guid, bool> libraryAccess = [];
        foreach (Guid libraryId in libraryIds)
        {
            bool canAccessLibrary = await _authorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(
                userId, new LibraryOwnershipPolicyContext(libraryId), cancellationToken).ConfigureAwait(false);
            libraryAccess[libraryId] = canAccessLibrary;
        }

        if (libraryAccess.Values.Any(canAccessLibrary => !canAccessLibrary))
        {
            List<PlaylistEntryDto> entries = [];
            foreach (PlaylistEntryDto entry in playlist.Entries)
            {
                Guid libraryId = entry.Display?.LibraryId ?? Guid.Empty;
                if (libraryId == Guid.Empty || libraryAccess[libraryId])
                    entries.Add(entry);
                else
                    // Keep the row so its owner can see and remove it, but do not expose display data from a library the user cannot access.
                    entries.Add(entry with { Display = null });
            }
            playlist = playlist with { Entries = entries };
        }

        return playlist.ToResponse();
    }
}
