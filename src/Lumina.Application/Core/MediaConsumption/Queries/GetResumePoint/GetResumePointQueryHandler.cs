#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.DataAccess.Entities.MediaConsumption;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Mapping.MediaConsumption;
using Lumina.Contracts.Responses.MediaConsumption;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.Common.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
#endregion

namespace Lumina.Application.Core.MediaConsumption.Queries.GetResumePoint;

/// <summary>
/// Handler for the query to get the resume point of the current user for a media item.
/// </summary>
public class GetResumePointQueryHandler : IQueryHandler<GetResumePointQuery, Result<ResumePointResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IValidator<GetResumePointQuery> _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetResumePointQueryHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    /// <param name="currentUserService">Injected service to retrieve the current user information.</param>
    /// <param name="validator">Injected validator for application validation rules.</param>
    public GetResumePointQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IValidator<GetResumePointQuery> validator)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _validator = validator;
    }

    /// <summary>
    /// Handles the query to get the resume point of the current user for a media item.
    /// </summary>
    /// <param name="query">The query to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either the requested <see cref="ResumePointResponse"/>, or an error message.
    /// </returns>
    public async Task<Result<ResumePointResponse>> HandleAsync(GetResumePointQuery query, CancellationToken cancellationToken)
    {
        List<Error> validationResult = _validator.Validate(query);
        if (validationResult.Count > 0)
            return validationResult;

        // An authenticated request must always carry a user identity.
        Guid? currentUserId = _currentUserService.UserId;
        if (currentUserId is null)
            return ApplicationErrors.Authorization.NotAuthorized;
        Guid userId = currentUserId.Value;

        // The validator guarantees that the media kind is a defined value and the media item identifier is a non-empty Guid before this point.
        MediaKind mediaKind = Enum.Parse<MediaKind>(query.MediaKind!, ignoreCase: true);
        Guid mediaItemId = Guid.Parse(query.MediaItemId!);

        Optional<MediaConsumptionRule> ruleResult = MediaConsumptionRule.For(mediaKind);
        if (!ruleResult.HasValue)
            return DomainErrors.MediaConsumption.UnsupportedMediaKind;
        if (!ruleResult.Value.CanResume)
            return DomainErrors.MediaConsumption.MediaKindCannotResume;
        // A planned kind cannot have a resume point, because its catalogue does not exist yet and start rejects it. Report it as unsupported instead of
        // as a missing resume point, so the response does not imply that the kind is usable.
        if (!ruleResult.Value.IsSupportedByCurrentCatalogues)
            return DomainErrors.MediaConsumption.UnsupportedMediaKind;

        // The resume point is read without tracking and scoped to the current user, so a resume point of another user is indistinguishable from a missing one.
        Result<MediaResumePointEntity?> getResumePointResult = await _unitOfWork.MediaResumePointRepository
            .GetForUserAndMediaItemAsync(userId, mediaKind, mediaItemId, shouldTrackEntities: false, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (getResumePointResult.IsFailure)
            return getResumePointResult.Errors;
        if (getResumePointResult.Value is null)
            return DomainErrors.MediaConsumption.MediaResumePointNotFound;

        // The media item must still exist in the library the resume point was recorded in. A resume point whose item was deleted, or whose item no
        // longer belongs to that library, is an orphan, so it is removed before the same not-found result as a missing item is returned.
        if (mediaKind == MediaKind.MusicTrack)
        {
            Result<TrackEntity?> getTrackResult = await _unitOfWork.TrackRepository
                .GetByIdAsync(mediaItemId, shouldIncludeNavigationProperties: false, shouldTrackEntities: false, cancellationToken: cancellationToken).ConfigureAwait(false);
            if (getTrackResult.IsFailure)
                return getTrackResult.Errors;
            if (getTrackResult.Value is null || getTrackResult.Value.LibraryId != getResumePointResult.Value.LibraryId)
            {
                Result<Deleted> deleteResult = await _unitOfWork.MediaResumePointRepository.DeleteForUserAndMediaItemAsync(userId, mediaKind, mediaItemId, cancellationToken).ConfigureAwait(false);
                if (deleteResult.IsFailure)
                    return deleteResult.Errors;
                Result<Success> saveChangesResult = await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                // Another request may have deleted the same orphan between the delete and this save. That race is the desired outcome, so a
                // concurrency conflict is treated as success; any other persistence failure is still returned.
                if (saveChangesResult.IsFailure && !saveChangesResult.Errors.Any(error => error == ApplicationErrors.Persistence.ConcurrencyConflict))
                    return saveChangesResult.Errors;
                return DomainErrors.Music.TrackNotFound;
            }
        }
        else if (mediaKind == MediaKind.Book)
        {
            Result<BookEntity?> getBookResult = await _unitOfWork.BookRepository
                .GetByIdAsync(mediaItemId, shouldIncludeNavigationProperties: false, shouldTrackEntities: false, cancellationToken: cancellationToken).ConfigureAwait(false);
            if (getBookResult.IsFailure)
                return getBookResult.Errors;
            if (getBookResult.Value is null || getBookResult.Value.LibraryId != getResumePointResult.Value.LibraryId)
            {
                Result<Deleted> deleteResult = await _unitOfWork.MediaResumePointRepository.DeleteForUserAndMediaItemAsync(userId, mediaKind, mediaItemId, cancellationToken).ConfigureAwait(false);
                if (deleteResult.IsFailure)
                    return deleteResult.Errors;
                Result<Success> saveChangesResult = await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                // Another request may have deleted the same orphan between the delete and this save. That race is the desired outcome, so a
                // concurrency conflict is treated as success; any other persistence failure is still returned.
                if (saveChangesResult.IsFailure && !saveChangesResult.Errors.Any(error => error == ApplicationErrors.Persistence.ConcurrencyConflict))
                    return saveChangesResult.Errors;
                return DomainErrors.WrittenContent.BookNotFound;
            }
        }
        else
            return DomainErrors.MediaConsumption.UnsupportedMediaKind;

        return getResumePointResult.Value.ToDto().ToResponse();
    }
}
