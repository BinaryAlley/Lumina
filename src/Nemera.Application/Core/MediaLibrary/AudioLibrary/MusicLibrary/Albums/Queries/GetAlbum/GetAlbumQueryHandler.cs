#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.CQRS;
using Nemera.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Nemera.Application.Common.DataAccess.UoW;
using Nemera.Application.Common.Infrastructure.Authentication;
using Nemera.Application.Common.Infrastructure.Authorization;
using Nemera.Application.Common.Infrastructure.Authorization.Policies.LibraryOwnership;
using Nemera.Application.Common.Infrastructure.Validation;
using Nemera.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary;
using Nemera.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Albums;
using Nemera.Contracts.DTO.Common;
using Nemera.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Albums;
using Nemera.Domain.Common.Errors;
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

namespace Nemera.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Albums.Queries.GetAlbum;

/// <summary>
/// Handler for the query to get an album by its Id.
/// </summary>
public class GetAlbumQueryHandler : IQueryHandler<GetAlbumQuery, Result<AlbumResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthorizationService _authorizationService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IValidator<GetAlbumQuery> _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetAlbumQueryHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    /// <param name="authorizationService">Injected service for authorization related functionality.</param>
    /// <param name="currentUserService">Injected service to retrieve the current user information.</param>
    /// <param name="validator">Injected validator for application validation rules.</param>
    public GetAlbumQueryHandler(IUnitOfWork unitOfWork, IAuthorizationService authorizationService, ICurrentUserService currentUserService, IValidator<GetAlbumQuery> validator)
    {
        _unitOfWork = unitOfWork;
        _authorizationService = authorizationService;
        _currentUserService = currentUserService;
        _validator = validator;
    }

    /// <summary>
    /// Handles the query to get an album by its Id.
    /// </summary>
    /// <param name="query">The query to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a successfully retrieved <see cref="AlbumResponse"/>, or an error message.
    /// </returns>
    public async Task<Result<AlbumResponse>> HandleAsync(GetAlbumQuery query, CancellationToken cancellationToken)
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

        Result<AlbumEntity?> getAlbumResult = await _unitOfWork.AlbumRepository.GetByIdAsync(albumId, shouldTrackEntities: false, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (getAlbumResult.IsFailure)
            return getAlbumResult.Errors;
        if (getAlbumResult.Value is null)
            return Errors.Music.AlbumNotFound;
        AlbumEntity existingAlbum = getAlbumResult.Value;

        // Resource scoping: the album must belong to the library and the artist named by the route, so that an album can never be read through
        // another library's or artist's route; the mismatch is reported as not found, without disclosing that the album exists elsewhere.
        if (existingAlbum.LibraryId != libraryId || existingAlbum.ArtistId != artistId)
            return Errors.Music.AlbumNotFound;

        // Admins can see the albums of all libraries; for everyone else, only the albums of the libraries they own.
        bool canAccessLibrary = await _authorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(
            userId, new LibraryOwnershipPolicyContext(existingAlbum.LibraryId), cancellationToken).ConfigureAwait(false);
        if (!canAccessLibrary)
            return ApplicationErrors.Authorization.NotAuthorized;

        // The artwork of an album is stored in the MusicArtwork table, not on the album row, so it is loaded to expose the cover
        // and every other stored piece of artwork.
        Result<IReadOnlyList<MusicArtworkEntity>> getArtworkResult = await _unitOfWork.MusicArtworkRepository.GetByOwnerAsync(MusicArtworkOwnerType.Album, albumId, cancellationToken).ConfigureAwait(false);
        if (getArtworkResult.IsFailure)
            return getArtworkResult.Errors;
        string? coverPath = getArtworkResult.Value
            .Where(artwork => artwork.ArtworkType == ArtworkType.Cover)
            .Select(artwork => artwork.FileName)
            .FirstOrDefault();

        // Only the artwork whose file has already been stored can be presented, so the ones that are still pending resolution are left out.
        List<MediaArtworkDto> artwork = [.. getArtworkResult.Value
            .Where(storedArtwork => !string.IsNullOrWhiteSpace(storedArtwork.FileName))
            .Select(storedArtwork => new MediaArtworkDto(storedArtwork.ArtworkType, storedArtwork.Ordinal, storedArtwork.FileName!))];

        return existingAlbum.ToResponse(coverPath, artwork);
    }
}
