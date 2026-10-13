#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.LibraryOwnership;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artwork;
using Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Artists;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
#endregion

namespace Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Commands.UpdateArtistCover;

/// <summary>
/// Handler for the command to update the cover image of an existing artist.
/// </summary>
public class UpdateArtistCoverCommandHandler : ICommandHandler<UpdateArtistCoverCommand, Result<UpdateArtistCoverResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMusicArtworkService _musicArtworkService;
    private readonly IAuthorizationService _authorizationService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IValidator<UpdateArtistCoverCommand> _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateArtistCoverCommandHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    /// <param name="musicArtworkService">Injected service for storing the artwork of a music library item.</param>
    /// <param name="authorizationService">Injected service for authorization related functionality.</param>
    /// <param name="currentUserService">Injected service to retrieve the current user information.</param>
    /// <param name="validator">Injected validator for application validation rules.</param>
    public UpdateArtistCoverCommandHandler(IUnitOfWork unitOfWork, IMusicArtworkService musicArtworkService, IAuthorizationService authorizationService, ICurrentUserService currentUserService, IValidator<UpdateArtistCoverCommand> validator)
    {
        _unitOfWork = unitOfWork;
        _musicArtworkService = musicArtworkService;
        _authorizationService = authorizationService;
        _currentUserService = currentUserService;
        _validator = validator;
    }

    /// <summary>
    /// Handles the command to update the cover image of an existing artist.
    /// </summary>
    /// <param name="command">The command to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either the response carrying the relative path of the stored cover image, or an error message.
    /// </returns>
    public async Task<Result<UpdateArtistCoverResponse>> HandleAsync(UpdateArtistCoverCommand command, CancellationToken cancellationToken)
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

        Result<ArtistEntity?> getArtistResult = await _unitOfWork.ArtistRepository.GetByIdAsync(artistId, shouldTrackEntities: false, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (getArtistResult.IsFailure)
            return getArtistResult.Errors;
        if (getArtistResult.Value is null)
            return DomainErrors.Music.ArtistNotFound;
        ArtistEntity existingArtist = getArtistResult.Value;

        // Resource scoping: the artist must belong to the library named by the route, so that an artist can never be edited through another
        // library's route; the mismatch is reported as not found, without disclosing that the artist exists in another library.
        if (existingArtist.LibraryId != libraryId)
            return DomainErrors.Music.ArtistNotFound;

        // Admins can update the artists of all libraries; for everyone else, only the artists of the libraries they own.
        bool canAccessLibrary = await _authorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(
            userId, new LibraryOwnershipPolicyContext(existingArtist.LibraryId), cancellationToken).ConfigureAwait(false);
        if (!canAccessLibrary)
            return ApplicationErrors.Authorization.NotAuthorized;

        // The artwork is stored under the library and artist segments, so their names are resolved to keep the location consistent with the scanned artwork.
        Result<LibraryEntity?> getLibraryResult = await _unitOfWork.LibraryRepository.GetByIdAsync(existingArtist.LibraryId, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (getLibraryResult.IsFailure)
            return getLibraryResult.Errors;
        if (getLibraryResult.Value is null)
            return DomainErrors.Library.LibraryNotFound;
        string libraryName = getLibraryResult.Value.Title;

        // The existing cover of the artist is looked up, so that the previous file can be replaced as a whole instead of being left orphaned on disk.
        Result<IReadOnlyList<MusicArtworkEntity>> getArtworkResult = await _unitOfWork.MusicArtworkRepository.GetByOwnerAsync(MusicArtworkOwnerType.Artist, artistId, cancellationToken).ConfigureAwait(false);
        if (getArtworkResult.IsFailure)
            return getArtworkResult.Errors;
        string? existingCoverPath = getArtworkResult.Value
            .Where(artwork => artwork.ArtworkType == ArtworkType.Cover)
            .Select(artwork => artwork.FileName)
            .FirstOrDefault();

        Result<string> storeArtworkResult = await _musicArtworkService.SaveArtistArtworkAsync(
            existingArtist.LibraryId,
            libraryName,
            existingArtist.Name,
            command.Cover!,
            command.FileName!,
            cancellationToken).ConfigureAwait(false);
        if (storeArtworkResult.IsFailure)
            return storeArtworkResult.Errors;

        // The stored cover of the artist is replaced as a whole: when the new cover is stored under a different name than the previous one
        // (for example a png replacing a jpg), the previous file is removed, so that no orphaned artwork is left on disk without a reference.
        if (!string.IsNullOrWhiteSpace(existingCoverPath) && existingCoverPath != storeArtworkResult.Value)
        {
            Result<Deleted> deleteResult = _musicArtworkService.DeleteStoredArtwork(existingCoverPath);
            if (deleteResult.IsFailure)
                return deleteResult.Errors;
        }

        // Map the newly stored cover image onto the artwork of the artist, replacing the existing cover or adding a new one.
        Result<Updated> upsertResult = await _unitOfWork.MusicArtworkRepository.UpsertCoverAsync(
            MusicArtworkOwnerType.Artist, artistId, storeArtworkResult.Value, cancellationToken).ConfigureAwait(false);
        if (upsertResult.IsFailure)
            return upsertResult.Errors;

        Result<Success> saveChangesResult = await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        if (saveChangesResult.IsFailure)
            return saveChangesResult.Errors;

        return new UpdateArtistCoverResponse(storeArtworkResult.Value);
    }
}
