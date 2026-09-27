#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.LibraryOwnership;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Artists;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
#endregion

namespace Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Commands.DeleteTrack;

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

        // A track is a child of the artist aggregate, so the whole aggregate is loaded and the track is removed within it.
        Result<ArtistEntity?> getArtistResult = await _unitOfWork.ArtistRepository
            .GetByIdAsync(artistId, shouldIncludeNavigationProperties: true, shouldTrackEntities: false, cancellationToken: cancellationToken).ConfigureAwait(false);
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

        AlbumEntity? existingAlbum = existingArtist.Albums.FirstOrDefault(album => album.Id == albumId);
        if (existingAlbum is null)
            return DomainErrors.Music.AlbumNotFound;
        TrackEntity? existingTrack = existingAlbum.Tracks.FirstOrDefault(track => track.Id == trackId);
        if (existingTrack is null)
            return DomainErrors.Music.TrackNotFound;

        // A track can only belong to a library that exists, so a client can never delete a track of a library of the host that is not there.
        Result<LibraryEntity?> getLibraryResult = await _unitOfWork.LibraryRepository
            .GetByIdAsync(libraryId, shouldTrackEntities: false, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (getLibraryResult.IsFailure)
            return getLibraryResult.Errors;
        if (getLibraryResult.Value is null)
            return DomainErrors.Library.LibraryNotFound;

        // The write path goes through the aggregate root: a track is a child entity of the artist aggregate, unlike Book and Artist,
        // which are aggregate roots and are deleted directly through their own repositories. This difference is intentional.
        Result<Artist> artistResult = existingArtist.ToDomainEntity();
        if (artistResult.IsFailure)
            return artistResult.Errors;
        // The album and the track are entities inside the artist aggregate, so they are referenced by object, not by id; the aggregate members are located here and passed through.
        Album? domainAlbum = artistResult.Value.Albums.FirstOrDefault(album => album.Id.Value == albumId);
        if (domainAlbum is null)
            return DomainErrors.Music.AlbumNotFound;
        Track? domainTrack = domainAlbum.Tracks.FirstOrDefault(track => track.Id.Value == trackId);
        if (domainTrack is null)
            return DomainErrors.Music.TrackNotFound;
        Result<Deleted> removeTrackResult = artistResult.Value.RemoveTrackFromAlbum(domainAlbum, domainTrack);
        if (removeTrackResult.IsFailure)
            return removeTrackResult.Errors;

        Result<Updated> updateResult = await _unitOfWork.ArtistRepository.UpdateAsync(artistResult.Value.ToRepositoryEntity(), cancellationToken).ConfigureAwait(false);
        if (updateResult.IsFailure)
            return updateResult.Errors;

        Result<Success> saveChangesResult = await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        if (saveChangesResult.IsFailure)
            return saveChangesResult.Errors;

        return Result.Deleted;
    }
}
