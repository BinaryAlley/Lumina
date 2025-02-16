#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Repositories.MediaLibrary;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.DomainEvents;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Mapping.MediaLibrary.Management;
using Lumina.Domain.Common.Events;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate;
using Mediator;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
#endregion

namespace Lumina.Application.Core.MediaLibrary.Management.Commands.CancelLibraryScan;

/// <summary>
/// Handler for the command for canceling the scan of a media library.
/// </summary>
public class CancelLibraryScanCommandHandler : IRequestHandler<CancelLibraryScanCommand, ErrorOr<Success>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuthorizationService _authorizationService;
    private readonly IDomainEventsQueue _domainEventsQueue;

    /// <summary>
    /// Initializes a new instance of the <see cref="CancelLibraryScanCommandHandler"/> class.
    /// </summary>
    /// <param name="authorizationService">Injected service for authorization related functionality.</param>
    /// <param name="currentUserService">Injected service to retrieve the current user information.</param>
    /// <param name="domainEventsQueue">Injected service for the queue of domain events.</param>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    public CancelLibraryScanCommandHandler(
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
    /// Handles the command for canceling the scan of a media library.
    /// </summary>
    /// <param name="request">The request to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> representing either a successful operation, or an error.</returns>
    public async ValueTask<ErrorOr<Success>> Handle(CancelLibraryScanCommand request, CancellationToken cancellationToken)
    {
        ILibraryScanRepository libraryScanRepository = _unitOfWork.GetRepository<ILibraryScanRepository>();

        // get the library scan from the repository
        ErrorOr<LibraryScanEntity?> getLibraryScansResult = await libraryScanRepository.GetByIdAsync(request.ScanId, cancellationToken).ConfigureAwait(false);
        if (getLibraryScansResult.IsError)
            return getLibraryScansResult.Errors;
        if (getLibraryScansResult.Value is null)
            return DomainErrors.LibraryScanning.LibraryScanNotFound;

        // if the user that wants to scan the library is not an Admin or is not the owner of the library, they do not have the right to scan it
        if (getLibraryScansResult.Value.UserId != _currentUserService.UserId ||
            !await _authorizationService.IsInRoleAsync(_currentUserService.UserId!.Value, "Admin", cancellationToken).ConfigureAwait(false))
            return ApplicationErrors.Authorization.NotAuthorized;

        // convert the repository scan to a domain object
        ErrorOr<LibraryScan> libraryScanDomainResult = getLibraryScansResult.Value.ToDomainEntity();
        if (libraryScanDomainResult.IsError)
            return libraryScanDomainResult.Errors;

        // cancel the media library scan
        ErrorOr<Success> cancelScanResult = libraryScanDomainResult.Value.CancelScan();
        if (cancelScanResult.IsError)
            return cancelScanResult.Errors;

        // update the status of the library scan in the repository
        ErrorOr<Updated> updateLibraryScanResult = await libraryScanRepository.UpdateAsync(libraryScanDomainResult.Value.ToRepositoryEntity(), cancellationToken).ConfigureAwait(false);
        if (updateLibraryScanResult.IsError)
            return updateLibraryScanResult.Errors;

        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        // queue any domain events
        foreach (IDomainEvent domainEvent in libraryScanDomainResult.Value.GetDomainEvents())
            _domainEventsQueue.Enqueue(domainEvent);

        return await ValueTask.FromResult(Result.Success);
    }
}
