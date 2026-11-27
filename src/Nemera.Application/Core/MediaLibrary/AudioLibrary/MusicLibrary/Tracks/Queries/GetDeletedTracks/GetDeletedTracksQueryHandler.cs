#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.CQRS;
using Nemera.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Nemera.Application.Common.DataAccess.UoW;
using Nemera.Application.Common.Infrastructure.Authentication;
using Nemera.Application.Common.Infrastructure.Authorization;
using Nemera.Application.Common.Infrastructure.Authorization.Policies.LibraryOwnership;
using Nemera.Application.Common.Infrastructure.Validation;
using Nemera.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using Nemera.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using Nemera.Domain.Common.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Nemera.Application.Common.Errors.Errors;
#endregion

namespace Nemera.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Queries.GetDeletedTracks;

/// <summary>
/// Handler for the query to get the deleted tracks of a media library.
/// </summary>
public class GetDeletedTracksQueryHandler : IQueryHandler<GetDeletedTracksQuery, Result<IReadOnlyList<DeletedTrackResponse>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthorizationService _authorizationService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IValidator<GetDeletedTracksQuery> _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetDeletedTracksQueryHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    /// <param name="authorizationService">Injected service for authorization related functionality.</param>
    /// <param name="currentUserService">Injected service to retrieve the current user information.</param>
    /// <param name="validator">Injected validator for application validation rules.</param>
    public GetDeletedTracksQueryHandler(IUnitOfWork unitOfWork, IAuthorizationService authorizationService, ICurrentUserService currentUserService, IValidator<GetDeletedTracksQuery> validator)
    {
        _unitOfWork = unitOfWork;
        _authorizationService = authorizationService;
        _currentUserService = currentUserService;
        _validator = validator;
    }

    /// <summary>
    /// Handles the query to get the deleted tracks of a media library.
    /// </summary>
    /// <param name="query">The query to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the deleted tracks, or an error.</returns>
    public async Task<Result<IReadOnlyList<DeletedTrackResponse>>> HandleAsync(GetDeletedTracksQuery query, CancellationToken cancellationToken)
    {
        List<Error> validationResult = _validator.Validate(query);
        if (validationResult.Count > 0)
            return validationResult;

        // An authenticated request must always carry a user identity.
        Guid? currentUserId = _currentUserService.UserId;
        if (currentUserId is null)
            return ApplicationErrors.Authorization.NotAuthorized;
        Guid userId = currentUserId.Value;

        // The validator guarantees that the library id of the route is a non-empty Guid before this point.
        Guid libraryId = Guid.Parse(query.LibraryId!);

        // Admins can see all libraries; for everyone else, only the libraries they own.
        bool canAccessLibrary = await _authorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(
            userId, new LibraryOwnershipPolicyContext(libraryId), cancellationToken).ConfigureAwait(false);
        if (!canAccessLibrary)
            return ApplicationErrors.Authorization.NotAuthorized;

        Result<IReadOnlyList<TrackEntity>> getDeletedTracksResult = await _unitOfWork.TrackRepository.GetSoftDeletedByLibraryIdAsync(libraryId, cancellationToken).ConfigureAwait(false);
        if (getDeletedTracksResult.IsFailure)
            return getDeletedTracksResult.Errors;

        return Result.From<IReadOnlyList<DeletedTrackResponse>>(
            [.. getDeletedTracksResult.Value.Select(track => track.ToResponse(track.DeletedOnUtc ?? track.CreatedOnUtc))]);
    }
}
