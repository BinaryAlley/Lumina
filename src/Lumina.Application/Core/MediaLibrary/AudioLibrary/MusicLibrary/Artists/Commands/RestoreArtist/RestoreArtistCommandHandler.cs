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

namespace Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Commands.RestoreArtist;

/// <summary>
/// Handler for the command to restore a deleted artist by its Id.
/// </summary>
public class RestoreArtistCommandHandler : ICommandHandler<RestoreArtistCommand, Result<Updated>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthorizationService _authorizationService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IValidator<RestoreArtistCommand> _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="RestoreArtistCommandHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    /// <param name="authorizationService">Injected service for authorization related functionality.</param>
    /// <param name="currentUserService">Injected service to retrieve the current user information.</param>
    /// <param name="validator">Injected validator for application validation rules.</param>
    public RestoreArtistCommandHandler(IUnitOfWork unitOfWork, IAuthorizationService authorizationService, ICurrentUserService currentUserService, IValidator<RestoreArtistCommand> validator)
    {
        _unitOfWork = unitOfWork;
        _authorizationService = authorizationService;
        _currentUserService = currentUserService;
        _validator = validator;
    }

    /// <summary>
    /// Handles the command to restore a deleted artist by its Id.
    /// </summary>
    /// <param name="command">The command to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<Result<Updated>> HandleAsync(RestoreArtistCommand command, CancellationToken cancellationToken)
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

        Result<ArtistEntity?> getArtistResult = await _unitOfWork.ArtistRepository.GetSoftDeletedByIdAsync(artistId, cancellationToken).ConfigureAwait(false);
        if (getArtistResult.IsFailure)
            return getArtistResult.Errors;
        if (getArtistResult.Value is null)
            return DomainErrors.Music.ArtistNotFound;
        ArtistEntity existingArtist = getArtistResult.Value;

        // Resource scoping: the artist must belong to the library named by the route, so that an artist can never be restored through another
        // library's route; the mismatch is reported as not found, without disclosing that the artist exists in another library.
        if (existingArtist.LibraryId != libraryId)
            return DomainErrors.Music.ArtistNotFound;

        // Admins can restore the artists of all libraries; for everyone else, only the artists of the libraries they own.
        bool canAccessLibrary = await _authorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(
            userId, new LibraryOwnershipPolicyContext(existingArtist.LibraryId), cancellationToken).ConfigureAwait(false);
        if (!canAccessLibrary)
            return ApplicationErrors.Authorization.NotAuthorized;

        // The name of a visible artist is unique within a library, so an artist can only be restored while no visible artist took over its name.
        // The lookup only considers visible artists, so it can never return the deleted artist itself.
        Result<ArtistEntity?> getArtistByNameResult = await _unitOfWork.ArtistRepository.GetByNameAsync(existingArtist.LibraryId, existingArtist.Name, cancellationToken).ConfigureAwait(false);
        if (getArtistByNameResult.IsFailure)
            return getArtistByNameResult.Errors;
        if (getArtistByNameResult.Value is not null)
            return DomainErrors.Music.ArtistAlreadyExists;

        Result<Updated> restoreArtistResult = await _unitOfWork.ArtistRepository.RestoreSoftDeletedByIdAsync(existingArtist.Id, cancellationToken).ConfigureAwait(false);
        if (restoreArtistResult.IsFailure)
            return restoreArtistResult.Errors;

        Result<Success> saveChangesResult = await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        if (saveChangesResult.IsFailure)
            return saveChangesResult.Errors;

        return Result.Updated;
    }
}
