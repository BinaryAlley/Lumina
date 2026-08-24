#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.DomainEvents;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.LibraryOwnership;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Mapping.MediaLibrary.Management;
using Lumina.Contracts.Responses.MediaLibrary.Management;
using Lumina.Domain.Common.Events;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Strategies.Environment;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.UserManagementBoundedContext.UserAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.Authorization;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.PhotoLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
#endregion

namespace Lumina.Application.Core.MediaLibrary.Management.Commands.UpdateLibrary;

/// <summary>
/// Handler for the command to update a media library.
/// </summary>
public class UpdateLibraryCommandHandler : ICommandHandler<UpdateLibraryCommand, Result<LibraryResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuthorizationService _authorizationService;
    private readonly IDomainEventsQueue _domainEventsQueue;
    private readonly IEnvironmentContext _environmentContext;
    private readonly IValidator<UpdateLibraryCommand> _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateLibraryCommandHandler"/> class.
    /// </summary>
    /// <param name="authorizationService">Injected service for authorization related functionality.</param>
    /// <param name="currentUserService">Injected service to retrieve the current user information.</param>
    /// <param name="domainEventsQueue">Injected service for the queue of domain events.</param>
    /// <param name="environmentContext">Injected facade service for environment contextual services.</param>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    /// <param name="validator">Injected validator for application validation rules.</param>
    public UpdateLibraryCommandHandler(
        IAuthorizationService authorizationService, 
        ICurrentUserService currentUserService,
        IDomainEventsQueue domainEventsQueue,
        IEnvironmentContext environmentContext,
        IUnitOfWork unitOfWork,
        IValidator<UpdateLibraryCommand> validator)
    {
        _authorizationService = authorizationService;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
        _domainEventsQueue = domainEventsQueue;
        _environmentContext = environmentContext;
        _validator = validator;
    }

    /// <summary>
    /// Handles the command to update a media library.
    /// </summary>
    /// <param name="command">The command to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a successfully updated <see cref="LibraryResponse"/>, or an error message.
    /// </returns>
    public async Task<Result<LibraryResponse>> HandleAsync(UpdateLibraryCommand command, CancellationToken cancellationToken)
    {
        List<Error> validationResult = _validator.Validate(command);
        if (validationResult.Count > 0)
            return validationResult;

        // an authenticated request must always carry a user identity
        Guid? currentUserId = _currentUserService.UserId;
        if (currentUserId is null)
            return ApplicationErrors.Authorization.NotAuthorized;
        Guid userId = currentUserId.Value;

        // make sure the file is an actual supported image
        if (command.CoverImage is not null)
        {
            Result<FileSystemPathId> fileSystemPathIdResult = FileSystemPathId.Create(command.CoverImage);
            if (fileSystemPathIdResult.IsFailure)
                return fileSystemPathIdResult.Errors;

            Result<ImageType> imageCheckResult = await _environmentContext.FileTypeService.GetImageTypeAsync(fileSystemPathIdResult.Value, cancellationToken).ConfigureAwait(false);
            if (imageCheckResult.IsFailure)
                return imageCheckResult.Errors;
            if (imageCheckResult.Value == ImageType.None)
                return DomainErrors.Library.CoverFileMustBeAnImage;
        }

        // get a library repository and retrieve the library to update
        Result<LibraryEntity?> getLibraryResult = await _unitOfWork.LibraryRepository.GetByIdAsync(command.Id, cancellationToken).ConfigureAwait(false);
        if (getLibraryResult.IsFailure)
            return getLibraryResult.Errors;
        else if (getLibraryResult.Value is null)
            return DomainErrors.Library.LibraryNotFound;

        // admins or users with the permission to manage media libraries can update any library; other users can only update the libraries they own
        bool hasManagePermission = await _authorizationService.HasPermissionAsync(userId, AuthorizationPermission.CanCreateLibraries, cancellationToken).ConfigureAwait(false);
        bool canAccessLibrary = await _authorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(
            userId, new LibraryOwnershipPolicyContext(command.Id), cancellationToken).ConfigureAwait(false);
        if (!hasManagePermission && !canAccessLibrary)
            return ApplicationErrors.Authorization.NotAuthorized;

        // create a domain library object
        Result<Library> createLibraryResult = Library.Create(
            LibraryId.Create(command.Id),
            UserId.Create(command.OwnerId),
            command.Title!,
            Enum.Parse<LibraryType>(command.LibraryType!),
            command.ContentLocations!,
            command.CoverImage,
            command.IsEnabled,
            command.IsLocked,
            command.CanDownloadMetadataFromWeb,
            command.ShouldSaveMetadataInMediaDirectories,
            command.ShouldSkipUnchangedDirectoriesDuringScan,
            [.. getLibraryResult.Value.LibraryScans.Select(libraryScan => ScanId.Create(libraryScan.Id))]
        );
        if (createLibraryResult.IsFailure)
            return createLibraryResult.Errors;
        // convert the domain library entity to a repository library entity
        LibraryEntity persistenceLibrary = createLibraryResult.Value.ToRepositoryEntity();

        // update the repository entity and save changes
        Result<Updated> updateLibraryResult = await _unitOfWork.LibraryRepository.UpdateAsync(persistenceLibrary, cancellationToken).ConfigureAwait(false);
        if (updateLibraryResult.IsFailure)
            return updateLibraryResult.Errors;
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        // retrieve the updated media library from the persistence medium and return it
        Result<LibraryEntity?> getCreatedLibraryResult = await _unitOfWork.LibraryRepository.GetByIdAsync(createLibraryResult.Value.Id.Value, cancellationToken).ConfigureAwait(false);
        if (getCreatedLibraryResult.IsFailure)
            return getCreatedLibraryResult.Errors;
        if (getCreatedLibraryResult.Value is null)
            return ApplicationErrors.Persistence.ErrorPersistingMediaLibrary;

        // mark the media library as saved
        createLibraryResult.Value.Save();

        // queue any domain events
        foreach (IDomainEvent domainEvent in createLibraryResult.Value.GetDomainEvents())
            _domainEventsQueue.Enqueue(domainEvent);

        return getCreatedLibraryResult.Value.ToResponse();
    }
}
