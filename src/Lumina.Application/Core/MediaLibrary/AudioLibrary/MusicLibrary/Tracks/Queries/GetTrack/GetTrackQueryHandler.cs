#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.LibraryOwnership;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
#endregion

namespace Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Queries.GetTrack;

/// <summary>
/// Handler for the query to get a track by its Id.
/// </summary>
public class GetTrackQueryHandler : IQueryHandler<GetTrackQuery, Result<TrackResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthorizationService _authorizationService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IValidator<GetTrackQuery> _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetTrackQueryHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    /// <param name="authorizationService">Injected service for authorization related functionality.</param>
    /// <param name="currentUserService">Injected service to retrieve the current user information.</param>
    /// <param name="validator">Injected validator for application validation rules.</param>
    public GetTrackQueryHandler(IUnitOfWork unitOfWork, IAuthorizationService authorizationService, ICurrentUserService currentUserService, IValidator<GetTrackQuery> validator)
    {
        _unitOfWork = unitOfWork;
        _authorizationService = authorizationService;
        _currentUserService = currentUserService;
        _validator = validator;
    }

    /// <summary>
    /// Handles the query to get a track by its Id.
    /// </summary>
    /// <param name="query">The query to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a successfully retrieved <see cref="TrackResponse"/>, or an error message.
    /// </returns>
    public async Task<Result<TrackResponse>> HandleAsync(GetTrackQuery query, CancellationToken cancellationToken)
    {
        List<Error> validationResult = _validator.Validate(query);
        if (validationResult.Count > 0)
            return validationResult;

        // An authenticated request must always carry a user identity.
        Guid? currentUserId = _currentUserService.UserId;
        if (currentUserId is null)
            return ApplicationErrors.Authorization.NotAuthorized;
        Guid userId = currentUserId.Value;

        // The validator guarantees that the route identifiers are non-empty Guids before this point.
        Guid libraryId = Guid.Parse(query.LibraryId!);
        Guid artistId = Guid.Parse(query.ArtistId!);
        Guid albumId = Guid.Parse(query.AlbumId!);
        Guid trackId = Guid.Parse(query.TrackId!);

        // The album is loaded first, so that the artist and the library named by the route are enforced before the track itself is read.
        Result<AlbumEntity?> getAlbumResult = await _unitOfWork.AlbumRepository.GetByIdAsync(albumId, shouldIncludeNavigationProperties: false, shouldTrackEntities: false, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (getAlbumResult.IsFailure)
            return getAlbumResult.Errors;
        if (getAlbumResult.Value is null)
            return Errors.Music.AlbumNotFound;
        AlbumEntity existingAlbum = getAlbumResult.Value;

        // Resource scoping: the album must belong to the artist and the library named by the route, so that a track can never be read through
        // another library's or artist's route; the mismatch is reported as not found, without disclosing that the album exists elsewhere.
        if (existingAlbum.LibraryId != libraryId || existingAlbum.ArtistId != artistId)
            return Errors.Music.AlbumNotFound;

        Result<TrackEntity?> getTrackResult = await _unitOfWork.TrackRepository.GetByIdAsync(trackId, shouldTrackEntities: false, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (getTrackResult.IsFailure)
            return getTrackResult.Errors;
        if (getTrackResult.Value is null)
            return Errors.Music.TrackNotFound;
        TrackEntity existingTrack = getTrackResult.Value;

        // Resource scoping: the track must belong to the album and the library named by the route, so that a track of another album can never
        // be read through this album's route; the mismatch is reported as not found, without disclosing that the track exists elsewhere.
        if (existingTrack.AlbumId != albumId || existingTrack.LibraryId != libraryId)
            return Errors.Music.TrackNotFound;

        // Admins can see the tracks of all libraries; for everyone else, only the tracks of the libraries they own.
        bool canAccessLibrary = await _authorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(
            userId, new LibraryOwnershipPolicyContext(existingTrack.LibraryId), cancellationToken).ConfigureAwait(false);
        if (!canAccessLibrary)
            return ApplicationErrors.Authorization.NotAuthorized;

        return existingTrack.ToResponse();
    }
}
