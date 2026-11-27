#region ========================================================================= USING =====================================================================================
using Nemera.Domain.Common.Primitives;
using Nemera.Application.Common.CQRS;
using Nemera.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Nemera.Application.Common.DataAccess.Repositories.MediaLibrary;
using Nemera.Application.Common.DataAccess.UoW;
using Nemera.Application.Common.DomainEvents;
using Nemera.Application.Common.Infrastructure.Authentication;
using Nemera.Application.Common.Infrastructure.Authorization;
using Nemera.Application.Common.Mapping.MediaLibrary.Management;
using Nemera.Application.Common.Errors;
using Nemera.Domain.Common.Events;
using Nemera.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate;
using Nemera.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Cancellation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Nemera.Application.Core.MediaLibrary.Management.Commands.CancelLibrariesScan;

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
        // An authenticated request must always carry a user identity.
        Guid? currentUserId = _currentUserService.UserId;
        if (currentUserId is null)
            return Errors.Authorization.NotAuthorized;
        Guid userId = currentUserId.Value;

        // Get the scans of the current user that are either executing or still waiting in the scan queue, from the repository.
        Result<IEnumerable<LibraryScanEntity>> getActiveLibraryScansResult = await _unitOfWork.LibraryScanRepository.GetActiveScansAsync(cancellationToken).ConfigureAwait(false);
        if (getActiveLibraryScansResult.IsFailure)
            return getActiveLibraryScansResult.Errors;

        // Filter the library scans to process.
        List<LibraryScanEntity> authorizedLibraryScans = [];
        // Admins can see all library scans.
        if (await _authorizationService.IsInRoleAsync(userId, AuthorizationRoles.ADMIN, cancellationToken).ConfigureAwait(false))
            authorizedLibraryScans.AddRange(getActiveLibraryScansResult.Value);
        else // For regular users, only take the library scans that belong to them.
            authorizedLibraryScans.AddRange(getActiveLibraryScansResult.Value.Where(libraryScan => libraryScan.UserId == userId));

        // Convert persistence library scans to domain entities.
        IEnumerable<Result<LibraryScan>> libraryScansDomainResult = authorizedLibraryScans.ToDomainEntities();

        List<IDomainEvent> domainEvents = [];

        // For each library scan, perform the cancellation.
        foreach (Result<LibraryScan> libraryScanDomainResult in libraryScansDomainResult)
        {
            if (libraryScanDomainResult.IsFailure)
                return libraryScanDomainResult.Errors;

            // Cancel the media library scan.
            Result<Success> cancelScanResult = libraryScanDomainResult.Value.CancelScan();
            if (cancelScanResult.IsFailure)
                return cancelScanResult.Errors;

            // Update the status of the library scan in the repository.
            Result<Updated> updateLibraryScanResult = await _unitOfWork.LibraryScanRepository.UpdateAsync(libraryScanDomainResult.Value.ToRepositoryEntity(), cancellationToken).ConfigureAwait(false);
            if (updateLibraryScanResult.IsFailure)
                return updateLibraryScanResult.Errors;

            // Collect all the domain events created during this library scan start.
            domainEvents.AddRange(libraryScanDomainResult.Value.GetDomainEvents());
        }

        Result<Success> saveChangesResult = await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        if (saveChangesResult.IsFailure)
            return saveChangesResult.Errors;

        // Queue any domain events.
        foreach (IDomainEvent domainEvent in domainEvents)
            _domainEventsQueue.Enqueue(domainEvent);

        return await ValueTask.FromResult(Result.Success);
    }
}
