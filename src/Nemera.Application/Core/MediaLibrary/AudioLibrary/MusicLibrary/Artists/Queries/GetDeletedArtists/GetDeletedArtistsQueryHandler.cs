#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.CQRS;
using Nemera.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Nemera.Application.Common.DataAccess.UoW;
using Nemera.Application.Common.Infrastructure.Authentication;
using Nemera.Application.Common.Infrastructure.Authorization;
using Nemera.Application.Common.Infrastructure.Authorization.Policies.LibraryOwnership;
using Nemera.Application.Common.Infrastructure.Validation;
using Nemera.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Artists;
using Nemera.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Artists;
using Nemera.Domain.Common.Primitives;
using Nemera.Domain.SharedKernel.Common.Enums.AudioLibrary;
using Nemera.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Nemera.Application.Common.Errors.Errors;
#endregion

namespace Nemera.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Queries.GetDeletedArtists;

/// <summary>
/// Handler for the query to get the deleted artists of a media library.
/// </summary>
public class GetDeletedArtistsQueryHandler : IQueryHandler<GetDeletedArtistsQuery, Result<IReadOnlyList<DeletedArtistResponse>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthorizationService _authorizationService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IValidator<GetDeletedArtistsQuery> _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetDeletedArtistsQueryHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    /// <param name="authorizationService">Injected service for authorization related functionality.</param>
    /// <param name="currentUserService">Injected service to retrieve the current user information.</param>
    /// <param name="validator">Injected validator for application validation rules.</param>
    public GetDeletedArtistsQueryHandler(IUnitOfWork unitOfWork, IAuthorizationService authorizationService, ICurrentUserService currentUserService, IValidator<GetDeletedArtistsQuery> validator)
    {
        _unitOfWork = unitOfWork;
        _authorizationService = authorizationService;
        _currentUserService = currentUserService;
        _validator = validator;
    }

    /// <summary>
    /// Handles the query to get the deleted artists of a media library.
    /// </summary>
    /// <param name="query">The query to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the deleted artists, or an error.</returns>
    public async Task<Result<IReadOnlyList<DeletedArtistResponse>>> HandleAsync(GetDeletedArtistsQuery query, CancellationToken cancellationToken)
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

        Result<IReadOnlyList<ArtistEntity>> getDeletedArtistsResult = await _unitOfWork.ArtistRepository.GetSoftDeletedByLibraryIdAsync(libraryId, cancellationToken).ConfigureAwait(false);
        if (getDeletedArtistsResult.IsFailure)
            return getDeletedArtistsResult.Errors;

        IReadOnlyList<ArtistEntity> deletedArtists = getDeletedArtistsResult.Value;
        Result<IReadOnlyList<MusicArtworkEntity>> getArtworkResult = await _unitOfWork.MusicArtworkRepository
            .GetByOwnersAsync(MusicArtworkOwnerType.Artist, [.. deletedArtists.Select(artist => artist.Id)], cancellationToken).ConfigureAwait(false);
        if (getArtworkResult.IsFailure)
            return getArtworkResult.Errors;

        // A cover row can exist before its file is resolved, so rows without a stored file name are skipped instead of hiding a resolved cover.
        Dictionary<Guid, string> coverPaths = getArtworkResult.Value
            .Where(artwork => artwork.ArtworkType == ArtworkType.Cover && !string.IsNullOrWhiteSpace(artwork.FileName))
            .GroupBy(artwork => artwork.OwnerId)
            .ToDictionary(group => group.Key, group => group.Select(artwork => artwork.FileName!).First());

        return Result.From<IReadOnlyList<DeletedArtistResponse>>(
            [.. deletedArtists.Select(artist => artist.ToResponse(artist.DeletedOnUtc ?? artist.CreatedOnUtc, coverPaths.GetValueOrDefault(artist.Id)))]);
    }
}
