#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.DataAccess.Entities.MediaContributors;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.LibraryOwnership;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Artists;
using Lumina.Contracts.DTO.MediaContributors;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Albums.Commands.AddAlbum;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Commands.AddTrack;
using Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Artists;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
#endregion

namespace Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Commands.UpdateArtist;

/// <summary>
/// Handler for the command to update an artist.
/// </summary>
public class UpdateArtistCommandHandler : ICommandHandler<UpdateArtistCommand, Result<ArtistResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthorizationService _authorizationService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IValidator<UpdateArtistCommand> _validator;
    private readonly IPathService _pathService;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateArtistCommandHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    /// <param name="authorizationService">Injected service for authorization related functionality.</param>
    /// <param name="currentUserService">Injected service to retrieve the current user information.</param>
    /// <param name="validator">Injected validator for application validation rules.</param>
    /// <param name="pathService">Injected service for file system path related functionality.</param>
    public UpdateArtistCommandHandler(IUnitOfWork unitOfWork, IAuthorizationService authorizationService, ICurrentUserService currentUserService, IValidator<UpdateArtistCommand> validator, IPathService pathService)
    {
        _unitOfWork = unitOfWork;
        _authorizationService = authorizationService;
        _currentUserService = currentUserService;
        _validator = validator;
        _pathService = pathService;
    }

    /// <summary>
    /// Handles the command to update an artist.
    /// </summary>
    /// <param name="command">The command to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a successfully updated <see cref="ArtistResponse"/>, or an error message.
    /// </returns>
    public async Task<Result<ArtistResponse>> HandleAsync(UpdateArtistCommand command, CancellationToken cancellationToken)
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

        // Get the existing artist with all its related data.
        Result<ArtistEntity?> getArtistResult = await _unitOfWork.ArtistRepository.GetByIdAsync(artistId, cancellationToken: cancellationToken).ConfigureAwait(false);
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

        // An artist can only belong to a library that exists, so a client can never update an artist that points to a library of the host that is not there.
        Result<LibraryEntity?> getLibraryResult = await _unitOfWork.LibraryRepository.GetByIdAsync(libraryId, shouldTrackEntities: false, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (getLibraryResult.IsFailure)
            return getLibraryResult.Errors;
        if (getLibraryResult.Value is null)
            return DomainErrors.Library.LibraryNotFound;

        // An artist's tracks are only readable if they are stored inside one of the content locations of their library, so a client can never
        // edit an entry to point the reading pipeline at an arbitrary file of the host.
        if (!AreTrackPathsWithinLibraryContentLocations(getLibraryResult.Value, command))
            return DomainErrors.Music.TrackPathMustBeWithinLibraryContentLocations;

        // Media contributors are referenced by Id and are never created implicitly by editing an artist, so each one must already exist.
        Result<IReadOnlyList<MediaContributorEntity>> getContributorsResult = await GetExistingContributorsAsync(command, cancellationToken).ConfigureAwait(false);
        if (getContributorsResult.IsFailure)
            return getContributorsResult.Errors;

        // Convert the command to a domain aggregate to enforce invariants, preserving the identity and creation metadata of the stored artist.
        Result<Artist> updateArtistResult = command.ToDomainEntity(existingArtist);
        if (updateArtistResult.IsFailure)
            return updateArtistResult.Errors;

        // Map the updated domain artist onto a fresh repository entity, ready for the repository to replace the stored data.
        ArtistEntity updatedArtist = updateArtistResult.Value.ToRepositoryEntity();

        Result<Updated> updateResult = await _unitOfWork.ArtistRepository.UpdateAsync(updatedArtist, cancellationToken).ConfigureAwait(false);
        if (updateResult.IsFailure)
            return updateResult.Errors;

        Result<Success> saveChangesResult = await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        if (saveChangesResult.IsFailure)
            return saveChangesResult.Errors;

        // Returns the artist as it was actually persisted, instead of the in-memory aggregate, so the response reflects every value the persistence medium applied on save.
        Result<ArtistEntity?> getPersistedArtistResult = await _unitOfWork.ArtistRepository.GetByIdAsync(existingArtist.Id, shouldTrackEntities: false, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (getPersistedArtistResult.IsFailure)
            return getPersistedArtistResult.Errors;
        if (getPersistedArtistResult.Value is null)
            return DomainErrors.Music.ArtistNotFound;

        return getPersistedArtistResult.Value.ToResponse();
    }

    /// <summary>
    /// Determines whether every track path referenced by <paramref name="command"/> is stored inside one of the content locations of <paramref name="library"/>.
    /// </summary>
    /// <param name="library">The media library that owns the artist.</param>
    /// <param name="command">The command carrying the referenced tracks.</param>
    /// <returns><see langword="true"/> when every referenced track path is within a content location of the library; otherwise, <see langword="false"/>.</returns>
    private bool AreTrackPathsWithinLibraryContentLocations(LibraryEntity library, UpdateArtistCommand command)
    {
        foreach (AddAlbumCommand album in command.Albums!)
            foreach (AddTrackCommand track in album.Tracks!)
                if (!library.ContentLocations.Any(contentLocation => _pathService.IsPathWithin(track.Path!, contentLocation.Path)))
                    return false;
        return true;
    }

    /// <summary>
    /// Gets the media contributors referenced by <paramref name="command"/>, requiring each referenced Id to already exist.
    /// </summary>
    /// <param name="command">The command carrying the referenced media contributors.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either the existing media contributors, or an error when at least one referenced contributor does not exist.
    /// </returns>
    private async Task<Result<IReadOnlyList<MediaContributorEntity>>> GetExistingContributorsAsync(UpdateArtistCommand command, CancellationToken cancellationToken)
    {
        List<Guid> contributorIds = [.. CollectContributorIds(command).Distinct()];
        if (contributorIds.Count == 0)
            return Result.From<IReadOnlyList<MediaContributorEntity>>([]);

        Result<IReadOnlyList<MediaContributorEntity>> getContributorsResult = await _unitOfWork.MediaContributorRepository
            .GetByIdsAsync(contributorIds, cancellationToken).ConfigureAwait(false);
        if (getContributorsResult.IsFailure)
            return getContributorsResult.Errors;
        if (getContributorsResult.Value.Count != contributorIds.Count)
            return DomainErrors.MediaContributor.MediaContributorNotFound;
        return Result.From(getContributorsResult.Value);
    }

    /// <summary>
    /// Collects the unique identifiers of every media contributor referenced by <paramref name="command"/>, be it directly on the artist,
    /// or indirectly through one of its albums or the tracks of those albums.
    /// </summary>
    /// <param name="command">The command carrying the referenced media contributors.</param>
    /// <returns>The collected media contributor identifiers.</returns>
    private static IEnumerable<Guid> CollectContributorIds(UpdateArtistCommand command)
    {
        foreach (MediaContributorReferenceDto contributor in command.Contributors!)
            yield return contributor.ContributorId;

        foreach (AddAlbumCommand album in command.Albums!)
        {
            foreach (MediaContributorReferenceDto contributor in album.Contributors!)
                yield return contributor.ContributorId;

            foreach (AddTrackCommand track in album.Tracks!)
                foreach (MediaContributorReferenceDto contributor in track.Contributors!)
                    yield return contributor.ContributorId;
        }
    }
}
