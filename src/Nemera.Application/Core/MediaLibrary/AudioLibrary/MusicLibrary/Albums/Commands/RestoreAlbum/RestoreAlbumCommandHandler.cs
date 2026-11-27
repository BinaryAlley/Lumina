#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.LibraryOwnership;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Domain.Common.Primitives;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
#endregion

namespace Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Albums.Commands.RestoreAlbum;

/// <summary>
/// Handler for the command to restore a deleted album by its Id.
/// </summary>
public class RestoreAlbumCommandHandler : ICommandHandler<RestoreAlbumCommand, Result<Updated>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthorizationService _authorizationService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IValidator<RestoreAlbumCommand> _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="RestoreAlbumCommandHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    /// <param name="authorizationService">Injected service for authorization related functionality.</param>
    /// <param name="currentUserService">Injected service to retrieve the current user information.</param>
    /// <param name="validator">Injected validator for application validation rules.</param>
    public RestoreAlbumCommandHandler(IUnitOfWork unitOfWork, IAuthorizationService authorizationService, ICurrentUserService currentUserService, IValidator<RestoreAlbumCommand> validator)
    {
        _unitOfWork = unitOfWork;
        _authorizationService = authorizationService;
        _currentUserService = currentUserService;
        _validator = validator;
    }

    /// <summary>
    /// Handles the command to restore a deleted album by its Id.
    /// </summary>
    /// <param name="command">The command to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<Result<Updated>> HandleAsync(RestoreAlbumCommand command, CancellationToken cancellationToken)
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
        Guid libraryId = Guid.Parse(command.LibraryId!);
        Guid artistId = Guid.Parse(command.ArtistId!);
        Guid albumId = Guid.Parse(command.AlbumId!);

        // A deleted album whose artist is also deleted is not restorable on its own, so the repository reports it as missing until the artist is restored.
        Result<AlbumEntity?> getAlbumResult = await _unitOfWork.AlbumRepository.GetSoftDeletedByIdAsync(albumId, cancellationToken).ConfigureAwait(false);
        if (getAlbumResult.IsFailure)
            return getAlbumResult.Errors;
        if (getAlbumResult.Value is null)
            return DomainErrors.Music.AlbumNotFound;
        AlbumEntity existingAlbum = getAlbumResult.Value;

        // Resource scoping: the album must belong to the artist and library named by the route, so that an album can never be restored through
        // another library's route; the mismatch is reported as not found, without disclosing that the album exists elsewhere.
        if (existingAlbum.LibraryId != libraryId || existingAlbum.ArtistId != artistId)
            return DomainErrors.Music.AlbumNotFound;

        // Admins can restore the albums of all libraries; for everyone else, only the albums of the libraries they own.
        bool canAccessLibrary = await _authorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(
            userId, new LibraryOwnershipPolicyContext(existingAlbum.LibraryId), cancellationToken).ConfigureAwait(false);
        if (!canAccessLibrary)
            return ApplicationErrors.Authorization.NotAuthorized;

        Result<Updated> restoreAlbumResult = await _unitOfWork.AlbumRepository.RestoreSoftDeletedByIdAsync(existingAlbum.Id, cancellationToken).ConfigureAwait(false);
        if (restoreAlbumResult.IsFailure)
            return restoreAlbumResult.Errors;

        Result<Success> saveChangesResult = await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        if (saveChangesResult.IsFailure)
            return saveChangesResult.Errors;

        return Result.Updated;
    }
}
