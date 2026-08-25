#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.DomainEvents;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Mapping.MediaLibrary.Management;
using Lumina.Contracts.Responses.MediaLibrary.Management;
using Lumina.Domain.Common.Events;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Strategies.Environment;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate;
using Lumina.Domain.Core.BoundedContexts.UserManagementBoundedContext.UserAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.Authorization;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.PhotoLibrary;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
#endregion

namespace Lumina.Application.Core.MediaLibrary.Management.Commands.AddLibrary;

/// <summary>
/// Handler for the command to add a media library.
/// </summary>
public class AddLibraryCommandHandler : ICommandHandler<AddLibraryCommand, Result<LibraryResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDomainEventsQueue _domainEventsQueue;
    private readonly IEnvironmentContext _environmentContext;
    private readonly IAuthorizationService _authorizationService;
    private readonly IValidator<AddLibraryCommand> _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="AddLibraryCommandHandler"/> class.
    /// </summary>
    /// <param name="authorizationService">Injected service for authorization related functionality.</param>
    /// <param name="currentUserService">Injected service to retrieve the current user information.</param>
    /// <param name="domainEventsQueue">Injected service for the queue of domain events.</param>
    /// <param name="environmentContext">Injected facade service for environment contextual services.</param>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    /// <param name="validator">Injected validator for application validation rules.</param>
    public AddLibraryCommandHandler(
        IAuthorizationService authorizationService,
        ICurrentUserService currentUserService, 
        IDomainEventsQueue domainEventsQueue,
        IEnvironmentContext environmentContext,
        IUnitOfWork unitOfWork,
        IValidator<AddLibraryCommand> validator)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _domainEventsQueue = domainEventsQueue;
        _environmentContext = environmentContext;
        _authorizationService = authorizationService;
        _validator = validator;
    }

    /// <summary>
    /// Handles the command to add a media library.
    /// </summary>
    /// <param name="command">The command to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a successfully created <see cref="LibraryResponse"/>, or an error message.
    /// </returns>
    public async Task<Result<LibraryResponse>> HandleAsync(AddLibraryCommand command, CancellationToken cancellationToken)
    {
        List<Error> validationResult = _validator.Validate(command);
        if (validationResult.Count > 0)
            return validationResult;

        // an authenticated request must always carry a user identity
        Guid? currentUserId = _currentUserService.UserId;
        if (currentUserId is null)
            return ApplicationErrors.Authorization.NotAuthorized;
        Guid userId = currentUserId.Value;

        // only admins or users with the permission to manage media libraries can create them
        if (!await _authorizationService.IsInRoleAsync(userId, "Admin", cancellationToken).ConfigureAwait(false) &&
            !await _authorizationService.HasPermissionAsync(userId, AuthorizationPermission.CanCreateLibraries, cancellationToken).ConfigureAwait(false))
            return ApplicationErrors.Authorization.NotAuthorized;

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

        // create a domain library object
        Result<Library> createLibraryResult = Library.Create(
            UserId.Create(userId),
            command.Title!,
            Enum.Parse<LibraryType>(command.LibraryType!),
            command.ContentLocations!,
            command.CoverImage,
            command.IsEnabled,
            command.IsLocked,
            command.CanDownloadMetadataFromWeb,
            command.ShouldSaveMetadataInMediaDirectories,
            command.ShouldSkipUnchangedDirectoriesDuringScan,
            []
        );

        if (createLibraryResult.IsFailure)
            return createLibraryResult.Errors;

        // convert the domain library entity to a repository library entity
        LibraryEntity persistenceLibrary = createLibraryResult.Value.ToRepositoryEntity();
        // insert the repository entity and save changes
        Result<Created> insertLibraryResult = await _unitOfWork.LibraryRepository.InsertAsync(persistenceLibrary, cancellationToken).ConfigureAwait(false);
        if (insertLibraryResult.IsFailure)
            return insertLibraryResult.Errors;
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        // retrieve the newly saved media library from the persistence medium and return it
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
