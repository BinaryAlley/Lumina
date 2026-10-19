#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.DomainEvents;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.LibraryOwnership;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.PlayHistory;
using Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.PlayHistory;
using Lumina.Domain.Common.Events;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.MusicPlayHistoryBoundedContext.ExternalIdentifiers.AudioLibraryBoundedContext.MusicLibraryAggregate;
using Lumina.Domain.Core.BoundedContexts.MusicPlayHistoryBoundedContext.ExternalIdentifiers.LibraryManagementBoundedContext.LibraryAggregate;
using Lumina.Domain.Core.BoundedContexts.MusicPlayHistoryBoundedContext.ExternalIdentifiers.UserManagementBoundedContext.UserAggregate;
using Lumina.Domain.Core.BoundedContexts.MusicPlayHistoryBoundedContext.MusicPlayRecordAggregate;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
#endregion

namespace Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.PlayHistory.Commands.StartPlay;

/// <summary>
/// Handler for the command to record the start of a play of a track by the current user.
/// </summary>
public class StartPlayCommandHandler : ICommandHandler<StartPlayCommand, Result<PlayRecordResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuthorizationService _authorizationService;
    private readonly IDomainEventsQueue _domainEventsQueue;
    private readonly IValidator<StartPlayCommand> _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="StartPlayCommandHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    /// <param name="currentUserService">Injected service to retrieve the current user information.</param>
    /// <param name="authorizationService">Injected service for authorization related functionality.</param>
    /// <param name="domainEventsQueue">Injected service for the queue of domain events.</param>
    /// <param name="validator">Injected validator for application validation rules.</param>
    public StartPlayCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IAuthorizationService authorizationService,
        IDomainEventsQueue domainEventsQueue,
        IValidator<StartPlayCommand> validator)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _authorizationService = authorizationService;
        _domainEventsQueue = domainEventsQueue;
        _validator = validator;
    }

    /// <summary>
    /// Handles the command to record the start of a play of a track by the current user.
    /// </summary>
    /// <param name="command">The command to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either the created <see cref="PlayRecordResponse"/>, or an error message.
    /// </returns>
    public async Task<Result<PlayRecordResponse>> HandleAsync(StartPlayCommand command, CancellationToken cancellationToken)
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

        // Admins can report plays of the tracks of all libraries; for everyone else, only the tracks of the libraries they own. Authorization
        // is evaluated before any album or track is read, so an unauthorized caller cannot tell whether the route's album or track exists.
        bool canAccessLibrary = await _authorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(
            userId, new LibraryOwnershipPolicyContext(libraryId), cancellationToken).ConfigureAwait(false);
        if (!canAccessLibrary)
            return ApplicationErrors.Authorization.NotAuthorized;

        // The album is loaded first, so that the artist and the library named by the request are enforced before the track itself is read. This is
        // the same resource scoping the track read uses, so a play can never reference a track through another library, artist or album.
        Result<AlbumEntity?> getAlbumResult = await _unitOfWork.AlbumRepository
            .GetByIdAsync(albumId, shouldIncludeNavigationProperties: false, shouldTrackEntities: false, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (getAlbumResult.IsFailure)
            return getAlbumResult.Errors;
        if (getAlbumResult.Value is null)
            return DomainErrors.Music.AlbumNotFound;
        AlbumEntity existingAlbum = getAlbumResult.Value;
        if (existingAlbum.LibraryId != libraryId || existingAlbum.ArtistId != artistId)
            return DomainErrors.Music.AlbumNotFound;

        Result<TrackEntity?> getTrackResult = await _unitOfWork.TrackRepository
            .GetByIdAsync(trackId, shouldIncludeNavigationProperties: false, shouldTrackEntities: false, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (getTrackResult.IsFailure)
            return getTrackResult.Errors;
        if (getTrackResult.Value is null)
            return DomainErrors.Music.TrackNotFound;
        TrackEntity existingTrack = getTrackResult.Value;
        if (existingTrack.AlbumId != albumId || existingTrack.LibraryId != libraryId)
            return DomainErrors.Music.TrackNotFound;

        // Create the play record, which also raises the domain event announcing the start of the play.
        MusicPlayRecord playRecord = MusicPlayRecord.Create(
            UserId.Create(userId),
            TrackId.Create(trackId),
            LibraryId.Create(libraryId),
            AlbumId.Create(albumId),
            ArtistId.Create(artistId));

        Result<Created> insertResult = await _unitOfWork.MusicPlayRecordRepository
            .InsertAsync(playRecord.ToRepositoryEntity(), cancellationToken).ConfigureAwait(false);
        if (insertResult.IsFailure)
            return insertResult.Errors;

        Result<Success> saveChangesResult = await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        if (saveChangesResult.IsFailure)
            return saveChangesResult.Errors;

        // Queue any domain events, to be published after the request has completed successfully.
        foreach (IDomainEvent domainEvent in playRecord.GetDomainEvents())
            _domainEventsQueue.Enqueue(domainEvent);

        // Reading does not need the aggregate: the persisted row is read directly and mapped to the response, so the response carries the
        // timestamps the auditing interceptor actually stored instead of the in-memory aggregate's values.
        Result<MusicPlayRecordEntity?> getPersistedPlayRecordResult = await _unitOfWork.MusicPlayRecordRepository
            .GetByIdForUserAsync(playRecord.Id.Value, userId, shouldTrackEntities: false, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (getPersistedPlayRecordResult.IsFailure)
            return getPersistedPlayRecordResult.Errors;
        if (getPersistedPlayRecordResult.Value is null)
            return DomainErrors.MusicPlayHistory.MusicPlayRecordNotFound;

        return getPersistedPlayRecordResult.Value.ToResponse();
    }
}
