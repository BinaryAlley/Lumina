#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using Lumina.Application.Common.CQRS;
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
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Core.MediaLibrary.Management.Commands.ScanLibraries;

/// <summary>
/// Handler for the command for initiating the scan of all media libraries.
/// </summary>
public class ScanLibrariesCommandHandler : ICommandHandler<ScanLibrariesCommand, Result<IEnumerable<MediaLibraryScanResponse>>>
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
    /// <param name="command">The command to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<Result<IEnumerable<MediaLibraryScanResponse>>> HandleAsync(ScanLibrariesCommand command, CancellationToken cancellationToken)
    {
        ILibraryRepository libraryRepository = _unitOfWork.GetRepository<ILibraryRepository>();
        ILibraryScanRepository libraryScanRepository = _unitOfWork.GetRepository<ILibraryScanRepository>();
        
        // get all media libraries that are enabled and unlocked from the persistence medium
        Result<IEnumerable<LibraryEntity>> getLibrariesResult = await libraryRepository.GetAllEnabledAndUnlockedAsync(cancellationToken).ConfigureAwait(false);
        if (getLibrariesResult.IsFailure)
            return getLibrariesResult.Errors;

        // convert persistence libraries to domain entities
        IEnumerable<Result<Library>> domainEntitiesResult = getLibrariesResult.Value.ToDomainEntities();

        // if the current user is not an admin, they can only scan the libraries that belong to them
        if (!await _authorizationService.IsInRoleAsync(_currentUserService.UserId!.Value, "Admin", cancellationToken).ConfigureAwait(false))
            domainEntitiesResult = domainEntitiesResult.Where(libraryResult => libraryResult.Value.UserId.Value == _currentUserService.UserId!.Value);

        List<MediaLibraryScanResponse> responses = [];
        List<IDomainEvent> domainEvents = [];

        // for each library, start the scan
        foreach (Result<Library> domainLibraryResult in domainEntitiesResult)
        {
            if (domainLibraryResult.IsFailure)
                return domainLibraryResult.Errors;

            // get the past month's scans for this library
            Result<IEnumerable<LibraryScanEntity>> pastLibraryScansResult = 
                await libraryScanRepository.GetPastMonthScansByLibraryIdAsync(domainLibraryResult.Value.Id.Value, cancellationToken).ConfigureAwait(false);
            if (pastLibraryScansResult.IsFailure)
                return pastLibraryScansResult.Errors;

            // convert the repository scans history to domain objects
            IEnumerable<Result<LibraryScan>> pastLibraryScansDomainResult = pastLibraryScansResult.Value.ToDomainEntities();
            foreach (Result<LibraryScan> pastLibraryScanDomainResult in pastLibraryScansDomainResult)
                if (pastLibraryScanDomainResult.IsFailure)
                    return pastLibraryScanDomainResult.Errors;

            // start the media library scan
            Result<LibraryScan> libraryScanResult = LibraryScan.Create(
                LibraryId.Create(domainLibraryResult.Value.Id.Value),
                UserId.Create(_currentUserService.UserId!.Value),
                [.. pastLibraryScansDomainResult.Select(pastLibraryScanDomainResult => pastLibraryScanDomainResult.Value)]
            );
            if (libraryScanResult.IsFailure)
                return libraryScanResult.Errors;

            Result<Success> startScanResult = libraryScanResult.Value.QueueScan();
            // when the user demands a "scan all libraries" action, it would be annoying to receive an error if a library is already being scanned.
            // instead, just don't start the scan on such a library, and start it on the others that can be started
            if (startScanResult.IsFailure)
                continue; 

            // add the library scan to the persistence medium
            Result<Created> insertLibraryScanResult = await libraryScanRepository.InsertAsync(libraryScanResult.Value.ToRepositoryEntity(), cancellationToken).ConfigureAwait(false);
            if (insertLibraryScanResult.IsFailure)
                return insertLibraryScanResult.Errors;

            // collect all the domain events created during this library scan start
            domainEvents.AddRange(libraryScanResult.Value.GetDomainEvents());
           
            responses.Add(new MediaLibraryScanResponse(libraryScanResult.Value.Id.Value, domainLibraryResult.Value.Id.Value));
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        // queue any domain events
        foreach (IDomainEvent domainEvent in domainEvents)
            _domainEventsQueue.Enqueue(domainEvent);

        return responses;
    }
}
