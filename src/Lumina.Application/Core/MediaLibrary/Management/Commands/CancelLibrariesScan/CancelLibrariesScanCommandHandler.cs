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
using Lumina.Application.Common.Errors;
using Lumina.Domain.Common.Events;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Cancellation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Core.MediaLibrary.Management.Commands.CancelLibrariesScan;

/// <summary>
/// Handler for the command for canceling the scan of all media libraries.
/// </summary>
public class CancelLibrariesScanCommandHandler : ICommandHandler<CancelLibrariesScanCommand, Result<Success>>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuthorizationService _authorizationService;
    private readonly IDomainEventsQueue _domainEventsQueue;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the <see cref="CancelLibrariesScanCommandHandler"/> class.
    /// </summary>
    /// <param name="currentUserService">Injected service to retrieve the current user information.</param>
    /// <param name="authorizationService">Injected service for authorization related functionality.</param>
    /// <param name="domainEventsQueue">Injected service for the queue of domain events.</param>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    public CancelLibrariesScanCommandHandler(
        ICurrentUserService currentUserService, 
        IAuthorizationService authorizationService, 
        IDomainEventsQueue domainEventsQueue, 
        IUnitOfWork unitOfWork)
    {
        _currentUserService = currentUserService;
        _authorizationService = authorizationService;
        _domainEventsQueue = domainEventsQueue;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Handles the command for canceling the scan of all media libraries.
    /// </summary>
    /// <param name="command">The command to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<Result<Success>> HandleAsync(CancelLibrariesScanCommand command, CancellationToken cancellationToken)
    {
        // an authenticated request must always carry a user identity
        Guid? currentUserId = _currentUserService.UserId;
        if (currentUserId is null)
            return Errors.Authorization.NotAuthorized;
        Guid userId = currentUserId.Value;

        // get the running library scans of the current user from the repository
        Result<IEnumerable<LibraryScanEntity>> getRunningLibraryScansResult = await _unitOfWork.LibraryScanRepository.GetRunningScansAsync(cancellationToken).ConfigureAwait(false);
        if (getRunningLibraryScansResult.IsFailure)
            return getRunningLibraryScansResult.Errors;

        // filter the library scans to process
        List<LibraryScanEntity> authorizedLibraryScans = [];
        // admins can see all library scans
        if (await _authorizationService.IsInRoleAsync(userId, "Admin", cancellationToken).ConfigureAwait(false))
            authorizedLibraryScans.AddRange(getRunningLibraryScansResult.Value);
        else // for regular users, only take the library scans that belong to them
            authorizedLibraryScans.AddRange(getRunningLibraryScansResult.Value.Where(libraryScan => libraryScan.UserId == userId));

        // convert persistence library scans to domain entities
        IEnumerable<Result<LibraryScan>> libraryScansDomainResult = authorizedLibraryScans.ToDomainEntities();

        List<IDomainEvent> domainEvents = [];

        // for each library scan, perform the cancellation
        foreach (Result<LibraryScan> libraryScanDomainResult in libraryScansDomainResult)
        {
            if (libraryScanDomainResult.IsFailure)
                return libraryScanDomainResult.Errors;

            // cancel the media library scan
            Result<Success> cancelScanResult = libraryScanDomainResult.Value.CancelScan();
            if (cancelScanResult.IsFailure)
                return cancelScanResult.Errors;

            // update the status of the library scan in the repository
            Result<Updated> updateLibraryScanResult = await _unitOfWork.LibraryScanRepository.UpdateAsync(libraryScanDomainResult.Value.ToRepositoryEntity(), cancellationToken).ConfigureAwait(false);
            if (updateLibraryScanResult.IsFailure)
                return updateLibraryScanResult.Errors;

            // collect all the domain events created during this library scan start
            domainEvents.AddRange(libraryScanDomainResult.Value.GetDomainEvents());
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        // queue any domain events
        foreach (IDomainEvent domainEvent in domainEvents)
            _domainEventsQueue.Enqueue(domainEvent);

        return await ValueTask.FromResult(Result.Success);
    }
}
