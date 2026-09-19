#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.LibraryOwnership;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Mapping.Common.Metadata;
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Artists;
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Common;
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using Lumina.Contracts.DTO.Common;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Common.ValueObjects.Metadata;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.Common.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.ExternalIdentifiers.LibraryManagementBoundedContext.LibraryAggregate;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.Entities;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
#endregion

namespace Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Commands.UpdateTrack;

/// <summary>
/// Handler for the command to update a track.
/// </summary>
public class UpdateTrackCommandHandler : ICommandHandler<UpdateTrackCommand, Result<TrackResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthorizationService _authorizationService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IValidator<UpdateTrackCommand> _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateTrackCommandHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    /// <param name="authorizationService">Injected service for authorization related functionality.</param>
    /// <param name="currentUserService">Injected service to retrieve the current user information.</param>
    /// <param name="validator">Injected validator for application validation rules.</param>
    public UpdateTrackCommandHandler(IUnitOfWork unitOfWork, IAuthorizationService authorizationService, ICurrentUserService currentUserService, IValidator<UpdateTrackCommand> validator)
    {
        _unitOfWork = unitOfWork;
        _authorizationService = authorizationService;
        _currentUserService = currentUserService;
        _validator = validator;
    }

    /// <summary>
    /// Handles the command to update a track.
    /// </summary>
    /// <param name="command">The command to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a successfully updated <see cref="TrackResponse"/>, or an error message.
    /// </returns>
    public async Task<Result<TrackResponse>> HandleAsync(UpdateTrackCommand command, CancellationToken cancellationToken)
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

        // A track is a child of the artist aggregate, so the whole aggregate is loaded and the track is edited within it.
        Result<ArtistEntity?> getArtistResult = await _unitOfWork.ArtistRepository.GetByIdAsync(artistId, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (getArtistResult.IsFailure)
            return getArtistResult.Errors;

        // Resource scoping: the track must belong to the library, the artist and the album named by the route, so that a track can never be
        // edited through another library's, artist's or album's route; the mismatch is reported as not found.
        if (getArtistResult.Value is null || getArtistResult.Value.LibraryId != libraryId)
            return Errors.Music.TrackNotFound;
        ArtistEntity existingArtist = getArtistResult.Value;
        AlbumEntity? existingAlbum = existingArtist.Albums.FirstOrDefault(album => album.Id == albumId);
        TrackEntity? existingTrack = existingAlbum?.Tracks.FirstOrDefault(track => track.Id == trackId);
        if (existingTrack is null)
            return Errors.Music.TrackNotFound;

        // Admins can update the tracks of all libraries; for everyone else, only the tracks of the libraries they own.
        bool canAccessLibrary = await _authorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(
            userId, new LibraryOwnershipPolicyContext(existingArtist.LibraryId), cancellationToken).ConfigureAwait(false);
        if (!canAccessLibrary)
            return ApplicationErrors.Authorization.NotAuthorized;

        // A track can only belong to a library that exists, so a client can never update a track of a library of the host that is not there.
        Result<LibraryEntity?> getLibraryResult = await _unitOfWork.LibraryRepository.GetByIdAsync(libraryId, shouldTrackEntities: false, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (getLibraryResult.IsFailure)
            return getLibraryResult.Errors;
        if (getLibraryResult.Value is null)
            return Errors.Library.LibraryNotFound;

        // The write path goes through the aggregate root: a track is a child entity of the artist aggregate, unlike Book and Artist,
        // which are aggregate roots and are reconstituted directly with their own Create method. This difference is intentional.
        Result<Artist> artistResult = existingArtist.ToDomainEntity();
        if (artistResult.IsFailure)
            return artistResult.Errors;

        Result<Artist> updatedArtistResult = command.ToDomainEntity(artistResult.Value);
        if (updatedArtistResult.IsFailure)
            return updatedArtistResult.Errors;

        Result<Updated> updateResult = await _unitOfWork.ArtistRepository.UpdateAsync(updatedArtistResult.Value.ToRepositoryEntity(), cancellationToken).ConfigureAwait(false);
        if (updateResult.IsFailure)
            return updateResult.Errors;
        Result<Success> saveChangesResult = await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        if (saveChangesResult.IsFailure)
            return saveChangesResult.Errors;

        // Reading does not need the aggregate: the track is read directly and mapped to the response.
        Result<TrackEntity?> getPersistedTrackResult = await _unitOfWork.TrackRepository.GetByIdAsync(trackId, shouldTrackEntities: false, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (getPersistedTrackResult.IsFailure)
            return getPersistedTrackResult.Errors;
        if (getPersistedTrackResult.Value is null)
            return Errors.Music.TrackNotFound;

        return getPersistedTrackResult.Value.ToResponse();
    }
}
