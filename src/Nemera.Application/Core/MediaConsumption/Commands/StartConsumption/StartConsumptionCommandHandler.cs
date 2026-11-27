#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.DataAccess.Entities.MediaConsumption;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.DomainEvents;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.LibraryOwnership;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Mapping.MediaConsumption;
using Lumina.Application.Common.Mapping.MediaLibrary.Management;
using Lumina.Contracts.Responses.MediaConsumption;
using Lumina.Domain.Common.Events;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.Common.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.ExternalIdentifiers.MediaLibraryBoundedContext.MediaItemAggregate;
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.ExternalIdentifiers.UserManagementBoundedContext.UserAggregate;
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.MediaConsumptionRecordAggregate;
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.MediaResumePointAggregate;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
using ConsumptionLibraryId = Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.ExternalIdentifiers.LibraryManagementBoundedContext.LibraryAggregate.LibraryId;
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
#endregion

namespace Lumina.Application.Core.MediaConsumption.Commands.StartConsumption;

/// <summary>
/// Handler for the command to start a consumption session of a media item by the current user.
/// </summary>
public class StartConsumptionCommandHandler : ICommandHandler<StartConsumptionCommand, Result<ConsumptionRecordResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuthorizationService _authorizationService;
    private readonly IDomainEventsQueue _domainEventsQueue;
    private readonly IValidator<StartConsumptionCommand> _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="StartConsumptionCommandHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    /// <param name="currentUserService">Injected service to retrieve the current user information.</param>
    /// <param name="authorizationService">Injected service for authorization related functionality.</param>
    /// <param name="domainEventsQueue">Injected service for the queue of domain events.</param>
    /// <param name="validator">Injected validator for application validation rules.</param>
    public StartConsumptionCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IAuthorizationService authorizationService,
        IDomainEventsQueue domainEventsQueue,
        IValidator<StartConsumptionCommand> validator)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _authorizationService = authorizationService;
        _domainEventsQueue = domainEventsQueue;
        _validator = validator;
    }

    /// <summary>
    /// Handles the command to start a consumption session of a media item by the current user.
    /// </summary>
    /// <param name="command">The command to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either the created <see cref="ConsumptionRecordResponse"/>, or an error message.
    /// </returns>
    public async Task<Result<ConsumptionRecordResponse>> HandleAsync(StartConsumptionCommand command, CancellationToken cancellationToken)
    {
        List<Error> validationResult = _validator.Validate(command);
        if (validationResult.Count > 0)
            return validationResult;

        // An authenticated request must always carry a user identity.
        Guid? currentUserId = _currentUserService.UserId;
        if (currentUserId is null)
            return ApplicationErrors.Authorization.NotAuthorized;
        Guid userId = currentUserId.Value;

        // The validator guarantees that the route identifiers are non-empty Guids and the media kind is a defined value before this point.
        Guid libraryId = Guid.Parse(command.LibraryId!);
        Guid mediaItemId = Guid.Parse(command.MediaItemId!);
        MediaKind mediaKind = Enum.Parse<MediaKind>(command.MediaKind!, ignoreCase: true);

        // Admins can start sessions in the libraries of all users; for everyone else, only in the libraries they own. Authorization is evaluated
        // before the kind is checked or any library or media item is read, so an unauthorized caller cannot probe whether a resource exists.
        bool canAccessLibrary = await _authorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(userId, new LibraryOwnershipPolicyContext(libraryId), cancellationToken).ConfigureAwait(false);
        if (!canAccessLibrary)
            return ApplicationErrors.Authorization.NotAuthorized;

        // A kind that has no consumption rule, and a kind whose catalogue does not exist yet, cannot be consumed.
        Optional<MediaConsumptionRule> ruleResult = MediaConsumptionRule.For(mediaKind);
        if (!ruleResult.HasValue || !ruleResult.Value.IsSupportedByCurrentCatalogues)
            return DomainErrors.MediaConsumption.UnsupportedMediaKind;
        MediaConsumptionRule rule = ruleResult.Value;

        // The library is loaded after authorization, so an unauthorized caller cannot probe whether it exists. For an authorized caller, a missing
        // library is reported as not found.
        Result<LibraryEntity?> getLibraryResult = await _unitOfWork.LibraryRepository
            .GetByIdAsync(libraryId, shouldIncludeNavigationProperties: false, shouldTrackEntities: false, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (getLibraryResult.IsFailure)
            return getLibraryResult.Errors;
        if (getLibraryResult.Value is null)
            return DomainErrors.Library.LibraryNotFound;
        LibraryConsumptionSettings consumptionSettings = getLibraryResult.Value.ToDomainValueObject();

        // The item is resolved through the catalogue repository of its kind, and the route library is enforced on the loaded row. A wrong-library
        // item is reported exactly like a missing item, so the route cannot be used to probe items of other libraries.
        if (mediaKind == MediaKind.MusicTrack)
        {
            Result<TrackEntity?> getTrackResult = await _unitOfWork.TrackRepository
                .GetByIdAsync(mediaItemId, shouldIncludeNavigationProperties: false, shouldTrackEntities: false, cancellationToken: cancellationToken).ConfigureAwait(false);
            if (getTrackResult.IsFailure)
                return getTrackResult.Errors;
            if (getTrackResult.Value is null || getTrackResult.Value.LibraryId != libraryId)
                return DomainErrors.Music.TrackNotFound;
        }
        else if (mediaKind == MediaKind.Book)
        {
            Result<BookEntity?> getBookResult = await _unitOfWork.BookRepository
                .GetByIdAsync(mediaItemId, shouldIncludeNavigationProperties: false, shouldTrackEntities: false, cancellationToken: cancellationToken).ConfigureAwait(false);
            if (getBookResult.IsFailure)
                return getBookResult.Errors;
            if (getBookResult.Value is null || getBookResult.Value.LibraryId != libraryId)
                return DomainErrors.WrittenContent.BookNotFound;
        }
        else
            return DomainErrors.MediaConsumption.UnsupportedMediaKind;

        // When the library tracks progress and the kind can resume, the new session continues from the stored resume point. The consumed duration
        // always starts at zero, because it measures this session only.
        Optional<MediaPosition> initialPosition = Optional<MediaPosition>.None();
        double initialProgressFraction = 0;
        if (consumptionSettings.ShouldTrackConsumptionProgress && rule.CanResume)
        {
            Result<MediaResumePointEntity?> getResumePointResult = await _unitOfWork.MediaResumePointRepository
                .GetForUserAndMediaItemAsync(userId, mediaKind, mediaItemId, shouldTrackEntities: false, cancellationToken: cancellationToken).ConfigureAwait(false);
            if (getResumePointResult.IsFailure)
                return getResumePointResult.Errors;
            if (getResumePointResult.Value is not null)
            {
                Result<MediaResumePoint> toDomainEntityResult = getResumePointResult.Value.ToDomainEntity();
                if (toDomainEntityResult.IsFailure)
                    return toDomainEntityResult.Errors;
                initialPosition = Optional<MediaPosition>.Some(toDomainEntityResult.Value.LastPosition);
                initialProgressFraction = toDomainEntityResult.Value.LastProgressFraction;
            }
        }

        // A one-shot kind such as a photo completes in the same call, because a single view does not have a start and a stop request.
        Result<MediaConsumptionRecord> createRecordResult = rule.IsOneShot
            ? MediaConsumptionRecord.StartCompleted(UserId.Create(userId), ConsumptionLibraryId.Create(libraryId), MediaItemId.Create(mediaItemId), mediaKind)
            : MediaConsumptionRecord.Start(UserId.Create(userId), ConsumptionLibraryId.Create(libraryId), MediaItemId.Create(mediaItemId), mediaKind, initialPosition, initialProgressFraction);
        if (createRecordResult.IsFailure)
            return createRecordResult.Errors;
        MediaConsumptionRecord record = createRecordResult.Value;

        Result<Created> insertResult = await _unitOfWork.MediaConsumptionRecordRepository.InsertAsync(record.ToRepositoryEntity(), cancellationToken).ConfigureAwait(false);
        if (insertResult.IsFailure)
            return insertResult.Errors;

        Result<Success> saveChangesResult = await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        if (saveChangesResult.IsFailure)
            return saveChangesResult.Errors;

        // Queue any domain events, to be published after the request has completed successfully.
        foreach (IDomainEvent domainEvent in record.GetDomainEvents())
            _domainEventsQueue.Enqueue(domainEvent);

        // The persisted row is read back, so the response carries the timestamps the auditing interceptor actually stored.
        Result<MediaConsumptionRecordEntity?> getPersistedRecordResult = await _unitOfWork.MediaConsumptionRecordRepository
            .GetByIdForUserAsync(record.Id.Value, userId, shouldTrackEntities: false, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (getPersistedRecordResult.IsFailure)
            return getPersistedRecordResult.Errors;
        if (getPersistedRecordResult.Value is null)
            return DomainErrors.MediaConsumption.ConsumptionRecordNotFound;

        return getPersistedRecordResult.Value.ToDto().ToResponse();
    }
}
