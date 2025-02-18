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
using Lumina.Domain.Common.Events;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.UserManagementBoundedContext.UserAggregate.ValueObjects;
using Mediator;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
#endregion

namespace Lumina.Application.Core.MediaLibrary.Management.Commands.ScanLibrary;

/// <summary>
/// Handler for the command for initiating the scan of a media library.
/// </summary>
public class ScanLibraryCommandHandler : IRequestHandler<ScanLibraryCommand, ErrorOr<MediaLibraryScanResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuthorizationService _authorizationService;
    private readonly IDomainEventsQueue _domainEventsQueue;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScanLibraryCommandHandler"/> class.
    /// </summary>
    /// <param name="authorizationService">Injected service for authorization related functionality.</param>
    /// <param name="currentUserService">Injected service to retrieve the current user information.</param>
    /// <param name="domainEventsQueue">Injected service for the queue of domain events.</param>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    public ScanLibraryCommandHandler(
        IAuthorizationService authorizationService,
        ICurrentUserService currentUserService,
        IDomainEventsQueue domainEventsQueue,
        IUnitOfWork unitOfWork)
    {
        _authorizationService = authorizationService;
        _currentUserService = currentUserService;
        _domainEventsQueue = domainEventsQueue;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Handles the command for initiating the scan of a media library.
    /// </summary>
    /// <param name="request">The request to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> representing either a successful operation, or an error.</returns>
    public async ValueTask<ErrorOr<MediaLibraryScanResponse>> Handle(ScanLibraryCommand request, CancellationToken cancellationToken)
    {
        ILibraryRepository libraryRepository = _unitOfWork.GetRepository<ILibraryRepository>();
        ILibraryScanRepository libraryScanRepository = _unitOfWork.GetRepository<ILibraryScanRepository>();

        // get the media library from the persistence medium
        ErrorOr<LibraryEntity?> getLibraryResult = await libraryRepository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);
        if (getLibraryResult.IsError)
            return getLibraryResult.Errors;

        if (getLibraryResult.Value is null)
            return DomainErrors.Library.LibraryNotFound;

        // if the user that wants to scan the library is not an Admin or is not the owner of the library, they do not have the right to scan it
        if (getLibraryResult.Value.UserId != _currentUserService.UserId ||
            !await _authorizationService.IsInRoleAsync(_currentUserService.UserId!.Value, "Admin", cancellationToken).ConfigureAwait(false))
            return ApplicationErrors.Authorization.NotAuthorized;

        // check if the library is enabled and unlocked, before scanning it
        if (!getLibraryResult.Value.IsEnabled)
            return DomainErrors.Library.CannotScanDisabledLibrary;

        if (getLibraryResult.Value.IsLocked)
            return DomainErrors.Library.CannotScanLockedLibrary;

        // convert the persistence library to a domain entity
        ErrorOr<Library> domainLibraryResult = getLibraryResult.Value.ToDomainEntity();
        if (domainLibraryResult.IsError)
            return domainLibraryResult.Errors;

        // get the past month's scans for this library
        ErrorOr<IEnumerable<LibraryScanEntity>> pastLibraryScansResult = await libraryScanRepository.GetPastMonthScansByLibraryIdAsync(request.Id, cancellationToken).ConfigureAwait(false);
        if (pastLibraryScansResult.IsError)
            return pastLibraryScansResult.Errors;
        
        // convert the repository scans history to domain objects
        IEnumerable<ErrorOr<LibraryScan>> pastLibraryScansDomainResult = pastLibraryScansResult.Value.ToDomainEntities();
        foreach (ErrorOr<LibraryScan> pastLibraryScanDomainResult in pastLibraryScansDomainResult)
            if (pastLibraryScanDomainResult.IsError)
                return pastLibraryScanDomainResult.Errors;

        // queue the media library scan
        ErrorOr<LibraryScan> libraryScanResult = LibraryScan.Create(
            LibraryId.Create(request.Id), 
            UserId.Create(_currentUserService.UserId!.Value),
            [.. pastLibraryScansDomainResult.Select(pastLibraryScanDomainResult => pastLibraryScanDomainResult.Value)]
        );
        if (libraryScanResult.IsError)
            return libraryScanResult.Errors;
        
        ErrorOr<Success> queueScanResult = libraryScanResult.Value.QueueScan();
        if (queueScanResult.IsError)
            return queueScanResult.Errors;

        // add the library scan to the persistence medium
        ErrorOr<Created> insertLibraryScanResult = await libraryScanRepository.InsertAsync(libraryScanResult.Value.ToRepositoryEntity(), cancellationToken).ConfigureAwait(false);
        if (insertLibraryScanResult.IsError)
            return insertLibraryScanResult.Errors;

        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        
        // queue any domain events
        foreach (IDomainEvent domainEvent in libraryScanResult.Value.GetDomainEvents())
            _domainEventsQueue.Enqueue(domainEvent);

        return new MediaLibraryScanResponse(libraryScanResult.Value.Id.Value, domainLibraryResult.Value.Id.Value);
    }
}
