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
using Lumina.Domain.Core.BoundedContexts.UserManagementBoundedContext.UserAggregate.ValueObjects;
using Mediator;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Core.MediaLibrary.Management.Commands.ScanLibraries;

/// <summary>
/// Handler for the command for initiating the scan of all media libraries.
/// </summary>
public class ScanLibrariesCommandHandler : IRequestHandler<ScanLibrariesCommand, ErrorOr<IEnumerable<ScanLibraryResponse>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuthorizationService _authorizationService;
    private readonly IDomainEventsQueue _domainEventsQueue;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScanLibrariesCommandHandler"/> class.
    /// </summary>
    /// <param name="authorizationService">Injected service for authorization related functionality.</param>
    /// <param name="currentUserService">Injected service to retrieve the current user information.</param>
    /// <param name="domainEventsQueue">Injected service for the queue of domain events.</param>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    public ScanLibrariesCommandHandler(
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
    /// Handles the command for initiating the scan of all media libraries.
    /// </summary>
    /// <param name="request">The request to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> representing either a successful operation, or an error.</returns>
    public async ValueTask<ErrorOr<IEnumerable<ScanLibraryResponse>>> Handle(ScanLibrariesCommand request, CancellationToken cancellationToken)
    {
        ILibraryRepository libraryRepository = _unitOfWork.GetRepository<ILibraryRepository>();
        ILibraryScanRepository libraryScanRepository = _unitOfWork.GetRepository<ILibraryScanRepository>();
        
        // get all media libraries that are enabled and unlocked from the persistence medium
        ErrorOr<IEnumerable<LibraryEntity>> getLibrariesResult = await libraryRepository.GetAllEnabledAndUnlockedAsync(cancellationToken).ConfigureAwait(false);
        if (getLibrariesResult.IsError)
            return getLibrariesResult.Errors;

        // convert persistence libraries to domain entities
        IEnumerable<ErrorOr<Library>> domainEntitiesResult = getLibrariesResult.Value.ToDomainEntities();

        // if the current user is not an admin, they can only scan the libraries that belong to them
        if (!await _authorizationService.IsInRoleAsync(_currentUserService.UserId!.Value, "Admin", cancellationToken).ConfigureAwait(false))
            domainEntitiesResult = domainEntitiesResult.Where(libraryResult => libraryResult.Value.UserId.Value == _currentUserService.UserId!.Value);

        List<ScanLibraryResponse> responses = [];
        List<IDomainEvent> domainEvents = [];

        // for each library, start the scan
        foreach (ErrorOr<Library> domainLibraryResult in domainEntitiesResult)
        {
            if (domainLibraryResult.IsError)
                return domainLibraryResult.Errors;

            // get the past month's scans for this library
            ErrorOr<IEnumerable<LibraryScanEntity>> pastLibraryScansResult = 
                await libraryScanRepository.GetPastMonthScansByLibraryIdAsync(domainLibraryResult.Value.Id.Value, cancellationToken).ConfigureAwait(false);
            if (pastLibraryScansResult.IsError)
                return pastLibraryScansResult.Errors;

            // convert the repository scans history to domain objects
            IEnumerable<ErrorOr<LibraryScan>> pastLibraryScansDomainResult = pastLibraryScansResult.Value.ToDomainEntities();
            foreach (ErrorOr<LibraryScan> pastLibraryScanDomainResult in pastLibraryScansDomainResult)
                if (pastLibraryScanDomainResult.IsError)
                    return pastLibraryScanDomainResult.Errors;

            // start the media library scan
            ErrorOr<LibraryScan> libraryScanResult = LibraryScan.Create(
                LibraryId.Create(domainLibraryResult.Value.Id.Value),
                UserId.Create(_currentUserService.UserId!.Value),
                [.. pastLibraryScansDomainResult.Select(pastLibraryScanDomainResult => pastLibraryScanDomainResult.Value)]
            );
            if (libraryScanResult.IsError)
                return libraryScanResult.Errors;

            ErrorOr<Success> startScanResult = libraryScanResult.Value.QueueScan();
            // when the user demands a "scan all libraries" action, it would be annoying to receive an error if a library is already being scanned.
            // instead, just don't start the scan on such a library, and start it on the others that can be started
            if (startScanResult.IsError)
                continue; 

            // add the library scan to the persistence medium
            ErrorOr<Created> insertLibraryScanResult = await libraryScanRepository.InsertAsync(libraryScanResult.Value.ToRepositoryEntity(), cancellationToken).ConfigureAwait(false);
            if (insertLibraryScanResult.IsError)
                return insertLibraryScanResult.Errors;

            // collect all the domain events created during this library scan start
            domainEvents.AddRange(libraryScanResult.Value.GetDomainEvents());
           
            responses.Add(new ScanLibraryResponse(libraryScanResult.Value.Id.Value, domainLibraryResult.Value.Id.Value));
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        // queue any domain events
        foreach (IDomainEvent domainEvent in domainEvents)
            _domainEventsQueue.Enqueue(domainEvent);

        return responses;
    }
}
