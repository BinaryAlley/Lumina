#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.CQRS;
using Nemera.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Nemera.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Nemera.Application.Common.DataAccess.UoW;
using Nemera.Application.Common.Infrastructure.Authentication;
using Nemera.Application.Common.Infrastructure.Authorization;
using Nemera.Application.Common.Infrastructure.Authorization.Policies.LibraryOwnership;
using Nemera.Application.Common.Infrastructure.Validation;
using Nemera.Domain.Common.Primitives;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Nemera.Application.Common.Errors.Errors;
using DomainErrors = Nemera.Domain.Common.Errors.Errors;
#endregion

namespace Nemera.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Commands.DeleteTrack;

/// <summary>
/// Handler for the command to delete a track by its Id.
/// </summary>
public class DeleteTrackCommandHandler : ICommandHandler<DeleteTrackCommand, Result<Deleted>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthorizationService _authorizationService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IValidator<DeleteTrackCommand> _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteTrackCommandHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    /// <param name="authorizationService">Injected service for authorization related functionality.</param>
    /// <param name="currentUserService">Injected service to retrieve the current user information.</param>
    /// <param name="validator">Injected validator for application validation rules.</param>
    public DeleteTrackCommandHandler(IUnitOfWork unitOfWork, IAuthorizationService authorizationService, ICurrentUserService currentUserService, IValidator<DeleteTrackCommand> validator)
    {
        _unitOfWork = unitOfWork;
        _authorizationService = authorizationService;
        _currentUserService = currentUserService;
        _validator = validator;
    }

    /// <summary>
    /// Handles the command to delete a track by its Id.
    /// </summary>
    /// <param name="command">The command to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<Result<Deleted>> HandleAsync(DeleteTrackCommand command, CancellationToken cancellationToken)
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
        Guid trackId = Guid.Parse(command.TrackId!);

        // Only the library of the artist is needed to scope the delete, so the artist row is read without its aggregate graph and without tracking.
        Result<ArtistEntity?> getArtistResult = await _unitOfWork.ArtistRepository
            .GetByIdAsync(artistId, shouldIncludeNavigationProperties: false, shouldTrackEntities: false, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (getArtistResult.IsFailure)
            return getArtistResult.Errors;
        if (getArtistResult.Value is null)
            return DomainErrors.Music.ArtistNotFound;
        ArtistEntity existingArtist = getArtistResult.Value;

        // Resource scoping: the track must belong to an artist of the library named by the route, so that a track can never be deleted through
        // another library's route; the mismatch is reported as not found, without disclosing that the artist exists in another library.
        if (existingArtist.LibraryId != libraryId)
            return DomainErrors.Music.ArtistNotFound;

        // Admins can delete the tracks of all libraries; for everyone else, only the tracks of the libraries they own.
        bool canAccessLibrary = await _authorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(
            userId, new LibraryOwnershipPolicyContext(existingArtist.LibraryId), cancellationToken).ConfigureAwait(false);
        if (!canAccessLibrary)
            return ApplicationErrors.Authorization.NotAuthorized;

        // A track can only belong to a library that exists, so a client can never delete a track of a library of the host that is not there.
        Result<LibraryEntity?> getLibraryResult = await _unitOfWork.LibraryRepository
            .GetByIdAsync(libraryId, shouldTrackEntities: false, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (getLibraryResult.IsFailure)
            return getLibraryResult.Errors;
        if (getLibraryResult.Value is null)
            return DomainErrors.Library.LibraryNotFound;

        Result<AlbumEntity?> getAlbumResult = await _unitOfWork.AlbumRepository
            .GetByIdAsync(albumId, shouldIncludeNavigationProperties: false, shouldTrackEntities: false, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (getAlbumResult.IsFailure)
            return getAlbumResult.Errors;
        if (getAlbumResult.Value is null || getAlbumResult.Value.ArtistId != artistId || getAlbumResult.Value.LibraryId != libraryId)
            return DomainErrors.Music.AlbumNotFound;

        Result<TrackEntity?> getTrackResult = await _unitOfWork.TrackRepository
            .GetByIdAsync(trackId, shouldIncludeNavigationProperties: false, shouldTrackEntities: false, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (getTrackResult.IsFailure)
            return getTrackResult.Errors;
        if (getTrackResult.Value is null || getTrackResult.Value.AlbumId != albumId || getTrackResult.Value.LibraryId != libraryId)
            return DomainErrors.Music.TrackNotFound;

        // The track is soft deleted, so its metadata, artwork and path stay available until the user restores it or the file disappears from disk.
        // Keeping the row is also what makes the deletion survive an automatic scan, because the scan treats its path as already catalogued.
        Result<Updated> deleteTrackResult = await _unitOfWork.TrackRepository.SoftDeleteByIdAsync(trackId, cancellationToken).ConfigureAwait(false);
        if (deleteTrackResult.IsFailure)
            return deleteTrackResult.Errors;

        Result<Success> saveChangesResult = await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        if (saveChangesResult.IsFailure)
            return saveChangesResult.Errors;

        return Result.Deleted;
    }
}
