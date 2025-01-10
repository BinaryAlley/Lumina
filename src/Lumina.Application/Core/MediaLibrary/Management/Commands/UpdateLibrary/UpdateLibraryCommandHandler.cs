#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Repositories.MediaLibrary;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.DomainEvents;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Mapping.MediaLibrary.Management;
using Lumina.Contracts.Responses.MediaLibrary.Management;
using Lumina.Domain.Common.Enums.Authorization;
using Lumina.Domain.Common.Enums.MediaLibrary;
using Lumina.Domain.Common.Enums.PhotoLibrary;
using Lumina.Domain.Common.Events;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Strategies.Environment;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Mediator;
using System;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
#endregion

namespace Lumina.Application.Core.MediaLibrary.Management.Commands.UpdateLibrary;

/// <summary>
/// Handler for the command to update a media library.
/// </summary>
public class UpdateLibraryCommandHandler : IRequestHandler<UpdateLibraryCommand, ErrorOr<LibraryResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuthorizationService _authorizationService;
    private readonly IDomainEventsQueue _domainEventsQueue;
    private readonly IEnvironmentContext _environmentContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateLibraryCommandHandler"/> class.
    /// </summary>
    /// <param name="authorizationService">Injected service for authorization related functionality.</param>
    /// <param name="currentUserService">Injected service to retrieve the current user information.</param>
    /// <param name="domainEventsQueue">Injected service for the queue of domain events.</param>
    /// <param name="environmentContext">Injected facade service for environment contextual services.</param>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    public UpdateLibraryCommandHandler(
        IAuthorizationService authorizationService, 
        ICurrentUserService currentUserService,
        IDomainEventsQueue domainEventsQueue,
        IEnvironmentContext environmentContext,
        IUnitOfWork unitOfWork)
    {
        _authorizationService = authorizationService;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
        _domainEventsQueue = domainEventsQueue;
        _environmentContext = environmentContext;
    }

    /// <summary>
    /// Handles the command to update a media library.
    /// </summary>
    /// <param name="request">The request to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="ErrorOr{TValue}"/> containing either a successfully updated <see cref="LibraryResponse"/>, or an error message.
    /// </returns>
    public async ValueTask<ErrorOr<LibraryResponse>> Handle(UpdateLibraryCommand request, CancellationToken cancellationToken)
    {
        // make sure the file is an actual supported image
        if (request.CoverImage is not null)
        {
            ErrorOr<FileSystemPathId> fileSystemPathIdResult = FileSystemPathId.Create(request.CoverImage);
            if (fileSystemPathIdResult.IsError)
                return fileSystemPathIdResult.Errors;

            ErrorOr<ImageType> imageCheckResult = await _environmentContext.FileTypeService.GetImageTypeAsync(fileSystemPathIdResult.Value, cancellationToken).ConfigureAwait(false);
            if (imageCheckResult.IsError)
                return imageCheckResult.Errors;
            if (imageCheckResult.Value == ImageType.None)
                return DomainErrors.Library.CoverFileMustBeAnImage;
        }

        // get a library repository
        ILibraryRepository libraryRepository = _unitOfWork.GetRepository<ILibraryRepository>();
        ErrorOr<LibraryEntity?> getLibraryResult = await libraryRepository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);
        if (getLibraryResult.IsError)
            return getLibraryResult.Errors;
        else if (getLibraryResult.Value is null)
            return DomainErrors.Library.LibraryNotFound;
        // if the user that made the request is not an Admin or is not the owner of the library, they do not have the right to update it
        if (getLibraryResult.Value.UserId != _currentUserService.UserId || 
            (!await _authorizationService.IsInRoleAsync(_currentUserService.UserId!.Value, "Admin", cancellationToken).ConfigureAwait(false) &&
             !await _authorizationService.HasPermissionAsync(_currentUserService.UserId!.Value, AuthorizationPermission.canCreateLibraries, cancellationToken)))
            return ApplicationErrors.Authorization.NotAuthorized;

        // create a domain library object
        ErrorOr<Library> createLibraryResult = Library.Create(
            LibraryId.Create(request.Id),
            request.OwnerId,
            request.Title!,
            Enum.Parse<LibraryType>(request.LibraryType!),
            request.ContentLocations!,
            request.CoverImage
        );
        if (createLibraryResult.IsError)
            return createLibraryResult.Errors;
        // convert the domain library entity to a repository library entity
        LibraryEntity persistenceLibrary = createLibraryResult.Value.ToRepositoryEntity();

        // update the repository entity and save changes
        ErrorOr<Updated> updateLibraryResult = await libraryRepository.UpdateAsync(persistenceLibrary, cancellationToken).ConfigureAwait(false);
        if (updateLibraryResult.IsError)
            return updateLibraryResult.Errors;
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        // queue any domain events
        foreach (IDomainEvent domainEvent in createLibraryResult.Value.PopDomainEvents())
            _domainEventsQueue.Enqueue(domainEvent);

        // retrieve the updated media library from the persistence medium and return it
        ErrorOr<LibraryEntity?> getCreatedLibraryResult = await libraryRepository.GetByIdAsync(createLibraryResult.Value.Id.Value, cancellationToken).ConfigureAwait(false);
        if (getCreatedLibraryResult.IsError)
            return getCreatedLibraryResult.Errors;
        if (getCreatedLibraryResult.Value is null)
            return ApplicationErrors.Persistence.ErrorPersistingMediaLibrary;
        return getCreatedLibraryResult.Value.ToResponse();
    }
}
