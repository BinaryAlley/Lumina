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
using Lumina.Application.Common.Mapping.Common.Metadata;
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Artists;
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Common;
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using Lumina.Contracts.DTO.Common;
using Lumina.Contracts.DTO.MediaContributors;
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
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Services;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
#endregion

namespace Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Commands.AddTrack;

/// <summary>
/// Handler for the command to add a track to an album.
/// </summary>
public class AddTrackCommandHandler : ICommandHandler<AddTrackCommand, Result<TrackResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthorizationService _authorizationService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IValidator<AddTrackCommand> _validator;
    private readonly IPathService _pathService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AddTrackCommandHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    /// <param name="authorizationService">Injected service for authorization related functionality.</param>
    /// <param name="currentUserService">Injected service to retrieve the current user information.</param>
    /// <param name="validator">Injected validator for application validation rules.</param>
    /// <param name="pathService">Injected service for file system path related functionality.</param>
    public AddTrackCommandHandler(IUnitOfWork unitOfWork, IAuthorizationService authorizationService, ICurrentUserService currentUserService, IValidator<AddTrackCommand> validator, IPathService pathService)
    {
        _unitOfWork = unitOfWork;
        _authorizationService = authorizationService;
        _currentUserService = currentUserService;
        _validator = validator;
        _pathService = pathService;
    }

    /// <summary>
    /// Handles the command to add a track to an album.
    /// </summary>
    /// <param name="command">The command to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a successfully created <see cref="TrackResponse"/>, or an error message.
    /// </returns>
    public async Task<Result<TrackResponse>> HandleAsync(AddTrackCommand command, CancellationToken cancellationToken)
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

        // A track is a child of the artist aggregate, so the whole aggregate is loaded and the track is added within it.
        Result<ArtistEntity?> getArtistResult = await _unitOfWork.ArtistRepository.GetByIdAsync(artistId, shouldIncludeNavigationProperties: true, shouldTrackEntities: false, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (getArtistResult.IsFailure)
            return getArtistResult.Errors;
        if (getArtistResult.Value is null)
            return Errors.Music.ArtistNotFound;
        ArtistEntity existingArtist = getArtistResult.Value;

        // Resource scoping: the track must belong to an artist of the library named by the route, so that a track can never be added through
        // another library's route; the mismatch is reported as not found, without disclosing that the artist exists in another library.
        if (existingArtist.LibraryId != libraryId)
            return Errors.Music.ArtistNotFound;

        // Admins can add tracks to the albums of all libraries; for everyone else, only to the albums of the libraries they own.
        bool canAccessLibrary = await _authorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(
            userId, new LibraryOwnershipPolicyContext(existingArtist.LibraryId), cancellationToken).ConfigureAwait(false);
        if (!canAccessLibrary)
            return ApplicationErrors.Authorization.NotAuthorized;

        AlbumEntity? existingAlbum = existingArtist.Albums.FirstOrDefault(album => album.Id == albumId);
        if (existingAlbum is null)
            return Errors.Music.AlbumNotFound;

        // A track can only belong to a library that exists, so a client can never register an entry that points to a library of the host that is not there.
        Result<LibraryEntity?> getLibraryResult = await _unitOfWork.LibraryRepository.GetByIdAsync(libraryId, shouldTrackEntities: false, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (getLibraryResult.IsFailure)
            return getLibraryResult.Errors;
        if (getLibraryResult.Value is null)
            return Errors.Library.LibraryNotFound;

        // A track is only readable if it is stored inside one of the content locations of its library, so a client can never register an entry
        // that points the reading pipeline at an arbitrary file of the host.
        if (!AreTrackPathsWithinLibraryContentLocations(getLibraryResult.Value, command))
            return Errors.Music.TrackPathMustBeWithinLibraryContentLocations;

        // Media contributors are referenced by Id and are never created implicitly by adding a track, so each one must already exist.
        Result<IReadOnlyList<MediaContributorEntity>> getContributorsResult = await GetExistingContributorsAsync(command, cancellationToken).ConfigureAwait(false);
        if (getContributorsResult.IsFailure)
            return getContributorsResult.Errors;

        // The write path goes through the aggregate root: a track is a child entity of the artist aggregate, unlike Book and Artist,
        // which are aggregate roots and are created directly with their own Create method. This difference is intentional.
        Result<Artist> artistResult = existingArtist.ToDomainEntity();
        if (artistResult.IsFailure)
            return artistResult.Errors;

        Result<Track> createTrackResult = command.ToDomainEntity();
        if (createTrackResult.IsFailure)
            return createTrackResult.Errors;
        // The album is an entity inside the artist aggregate, so it is referenced by object, not by id; the aggregate member is located here and passed through.
        Album? domainAlbum = artistResult.Value.Albums.FirstOrDefault(album => album.Id.Value == albumId);
        if (domainAlbum is null)
            return Errors.Music.AlbumNotFound;
        Result<Created> addTrackResult = artistResult.Value.AddTrackToAlbum(domainAlbum, createTrackResult.Value);
        if (addTrackResult.IsFailure)
            return addTrackResult.Errors;

        Result<Updated> updateResult = await _unitOfWork.ArtistRepository.UpdateAsync(artistResult.Value.ToRepositoryEntity(), cancellationToken).ConfigureAwait(false);
        if (updateResult.IsFailure)
            return updateResult.Errors;
        Result<Success> saveChangesResult = await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        if (saveChangesResult.IsFailure)
            return saveChangesResult.Errors;

        // Reading does not need the aggregate: the track is read directly and mapped to the response.
        Result<TrackEntity?> getPersistedTrackResult = await _unitOfWork.TrackRepository.GetByIdAsync(createTrackResult.Value.Id.Value, shouldTrackEntities: false, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (getPersistedTrackResult.IsFailure)
            return getPersistedTrackResult.Errors;
        if (getPersistedTrackResult.Value is null)
            return Errors.Music.TrackNotFound;

        return getPersistedTrackResult.Value.ToResponse();
    }

    /// <summary>
    /// Determines whether the track path referenced by <paramref name="command"/> is stored inside one of the content locations of <paramref name="library"/>.
    /// </summary>
    /// <param name="library">The media library that owns the track.</param>
    /// <param name="command">The command carrying the referenced track.</param>
    /// <returns><see langword="true"/> when the referenced track path is within a content location of the library; otherwise, <see langword="false"/>.</returns>
    private bool AreTrackPathsWithinLibraryContentLocations(LibraryEntity library, AddTrackCommand command)
    {
        return library.ContentLocations.Any(contentLocation => _pathService.IsPathWithin(command.Path!, contentLocation.Path));
    }

    /// <summary>
    /// Gets the media contributors referenced by <paramref name="command"/>, requiring each referenced Id to already exist.
    /// </summary>
    /// <param name="command">The command carrying the referenced media contributors.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either the existing media contributors, or an error when at least one referenced contributor does not exist.
    /// </returns>
    private async Task<Result<IReadOnlyList<MediaContributorEntity>>> GetExistingContributorsAsync(AddTrackCommand command, CancellationToken cancellationToken)
    {
        List<Guid> contributorIds = [.. CollectContributorIds(command).Distinct()];
        if (contributorIds.Count == 0)
            return Result.From<IReadOnlyList<MediaContributorEntity>>([]);

        Result<IReadOnlyList<MediaContributorEntity>> getContributorsResult = await _unitOfWork.MediaContributorRepository
            .GetByIdsAsync(contributorIds, cancellationToken).ConfigureAwait(false);
        if (getContributorsResult.IsFailure)
            return getContributorsResult.Errors;
        if (getContributorsResult.Value.Count != contributorIds.Count)
            return Errors.MediaContributor.MediaContributorNotFound;
        return Result.From(getContributorsResult.Value);
    }

    /// <summary>
    /// Collects the unique identifiers of every media contributor referenced by <paramref name="command"/>.
    /// </summary>
    /// <param name="command">The command carrying the referenced media contributors.</param>
    /// <returns>The collected media contributor identifiers.</returns>
    private static IEnumerable<Guid> CollectContributorIds(AddTrackCommand command)
    {
        foreach (MediaContributorReferenceDto contributor in command.Contributors!)
            yield return contributor.ContributorId;
    }
}
