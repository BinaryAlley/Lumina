#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Repositories.MediaLibrary;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Mapping.MediaLibrary.Management;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Exceptions;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Events;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services;
using Mediator;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Core.MediaLibrary.Management.Events;

/// <summary>
/// Handler for the event raised when a media library scan is cancelled.
/// </summary>
public class LibraryScanCancelledDomainEventHandler : INotificationHandler<LibraryScanCancelledDomainEvent>
{
    private readonly IMediaLibraryScanningService _mediaLibraryScanningService;
    private readonly ILibraryScanRepository _libraryScanRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="LibraryScanCancelledDomainEventHandler"/> class.
    /// </summary>
    /// <param name="mediaLibraryScanningService">Injected service for scanning media libraries.</param>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    public LibraryScanCancelledDomainEventHandler(IMediaLibraryScanningService mediaLibraryScanningService, IUnitOfWork unitOfWork)
    {
        _mediaLibraryScanningService = mediaLibraryScanningService;
        _libraryScanRepository = unitOfWork.GetRepository<ILibraryScanRepository>();
    }

    /// <summary>
    /// Handles the event raised when a media library scan is cancelled.
    /// </summary>
    /// <param name="domainEvent">The domain event to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public async ValueTask Handle(LibraryScanCancelledDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        // get the library scan from the repository
        ErrorOr<LibraryScanEntity?> getLibraryScansResult = await _libraryScanRepository.GetByIdAsync(domainEvent.ScanId.Value, cancellationToken).ConfigureAwait(false);
        if (getLibraryScansResult.IsError)
            throw new EventualConsistencyException(getLibraryScansResult.FirstError, getLibraryScansResult.Errors);
        if (getLibraryScansResult.Value is null)
            throw new EventualConsistencyException(Errors.LibraryScanning.LibraryScanNotFound);

        // convert the repository scan to a domain object
        ErrorOr<LibraryScan> libraryScanDomainResult = getLibraryScansResult.Value.ToDomainEntity();
        if (libraryScanDomainResult.IsError)
            throw new EventualConsistencyException(libraryScanDomainResult.FirstError, libraryScanDomainResult.Errors);

        // cancel the media library scan
        ErrorOr<Success> cancelScanResult = _mediaLibraryScanningService.CancelScan(libraryScanDomainResult.Value);
        if (cancelScanResult.IsError)
            throw new EventualConsistencyException(cancelScanResult.FirstError, cancelScanResult.Errors);
    }
}
