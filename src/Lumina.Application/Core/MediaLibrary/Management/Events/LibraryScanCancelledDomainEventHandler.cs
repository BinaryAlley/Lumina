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
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Cancellation;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Progress;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.UserManagementBoundedContext.UserAggregate.ValueObjects;
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
    private readonly IMediaLibrariesScanCancellationTracker _mediaLibrariesScanCancellationTracker;
    private readonly IMediaLibrariesScanProgressTracker _mediaLibrariesScanProgressTracker;
    private readonly ILibraryScanRepository _libraryScanRepository;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the <see cref="LibraryScanCancelledDomainEventHandler"/> class.
    /// </summary>
    /// <param name="mediaLibraryScanningService">Injected service for scanning media libraries.</param>
    /// <param name="mediaLibrariesScanCancellationTracker">Injected tracker used for canceling media library scans.</param>
    /// <param name="mediaLibrariesScanProgressTracker">Injected tracker used for media library scans progress.</param>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    public LibraryScanCancelledDomainEventHandler(
        IMediaLibraryScanningService mediaLibraryScanningService,
        IMediaLibrariesScanCancellationTracker mediaLibrariesScanCancellationTracker,
        IMediaLibrariesScanProgressTracker mediaLibrariesScanProgressTracker,
        IUnitOfWork unitOfWork)
    {
        _mediaLibraryScanningService = mediaLibraryScanningService;
        _mediaLibrariesScanCancellationTracker = mediaLibrariesScanCancellationTracker;
        _mediaLibrariesScanProgressTracker = mediaLibrariesScanProgressTracker;
        _libraryScanRepository = unitOfWork.GetRepository<ILibraryScanRepository>();
        _unitOfWork = unitOfWork;
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

        // release the scan processing resources
        ILibraryScanStagingResultsRepository stagingResultsRepository = _unitOfWork.GetRepository<ILibraryScanStagingResultsRepository>();
        MediaLibraryScanCompositeId compositeId = MediaLibraryScanCompositeId.Create(domainEvent.ScanId, UserId.Create(getLibraryScansResult.Value.UserId));
        _mediaLibrariesScanCancellationTracker.RemoveScan(compositeId);
        _mediaLibrariesScanProgressTracker.RemoveScanProgress(compositeId);
        ErrorOr<Success> clearStagingResult = await stagingResultsRepository.ClearForScanAsync(domainEvent.ScanId.Value, cancellationToken).ConfigureAwait(false);
        if (clearStagingResult.IsError)
            throw new EventualConsistencyException(clearStagingResult.FirstError, clearStagingResult.Errors);
    }
}
