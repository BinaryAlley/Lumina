#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Repositories.MediaLibrary;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Mapping.MediaLibrary.Management;
using Lumina.Application.Core.MediaLibrary.Management.Progress;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Events;
using Lumina.Domain.Common.Exceptions;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Events;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Cancellation;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Progress;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Core.MediaLibrary.Management.Events;

/// <summary>
/// Handler for the domain event raised when a media library scan has failed.
/// </summary>
public class LibraryScanFailedDomainEventHandler : IDomainEventHandler<LibraryScanFailedDomainEvent>
{
    private readonly IMediaLibraryScanProgressNotifier _debouncedLibraryScanProgressNotifier;
    private readonly IMediaLibrariesScanCancellationTracker _mediaLibrariesScanCancellationTracker;
    private readonly IMediaLibrariesScanProgressTracker _mediaLibrariesScanProgressTracker;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the <see cref="LibraryScanFailedDomainEventHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    /// <param name="mediaLibrariesScanCancellationTracker">Injected tracker used for canceling media library scans.</param>
    /// <param name="mediaLibrariesScanProgressTracker">Injected tracker used for media library scans progress.</param>
    /// <param name="debouncedLibraryScanProgressNotifier">Injected service for notifying media libraries scan progress changes to third parties.</param>
    public LibraryScanFailedDomainEventHandler(
        IMediaLibraryScanProgressNotifier debouncedLibraryScanProgressNotifier,
        IMediaLibrariesScanCancellationTracker mediaLibrariesScanCancellationTracker,
        IMediaLibrariesScanProgressTracker mediaLibrariesScanProgressTracker,
        IUnitOfWork unitOfWork)
    {
        _debouncedLibraryScanProgressNotifier = debouncedLibraryScanProgressNotifier;
        _mediaLibrariesScanCancellationTracker = mediaLibrariesScanCancellationTracker;
        _mediaLibrariesScanProgressTracker = mediaLibrariesScanProgressTracker;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Handles the event raised when a media library scan has failed.
    /// </summary>
    /// <param name="domainEvent">The domain event to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public async ValueTask HandleAsync(LibraryScanFailedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        // get the library scan from the repository
        Result<LibraryScanEntity?> getLibraryScansResult = await _unitOfWork.LibraryScanRepository.GetByIdAsync(
            domainEvent.MediaLibraryScanCompositeId.ScanId.Value, cancellationToken).ConfigureAwait(false);
        if (getLibraryScansResult.IsFailure)
            throw new EventualConsistencyException(getLibraryScansResult.FirstError, getLibraryScansResult.Errors);
        if (getLibraryScansResult.Value is null)
            throw new EventualConsistencyException(Errors.LibraryScanning.LibraryScanNotFound);

        // convert the repository scan to a domain object
        Result<LibraryScan> libraryScanDomainResult = getLibraryScansResult.Value.ToDomainEntity();
        if (libraryScanDomainResult.IsFailure)
            throw new EventualConsistencyException(libraryScanDomainResult.FirstError, libraryScanDomainResult.Errors);

        // mark the media library scan as failed
        Result<Success> failScanResult = libraryScanDomainResult.Value.FailScan();
        // we're going to ignore errors in this point, because scan jobs run in parallel, and two concurrent jobs of the same scan might trigger this domain event,
        // trying to set as failed a scan that has already been marked as failed by a concurrent job
        if (!failScanResult.IsFailure)
        {
            // update the status of the library scan in the repository
            Result<Updated> updateLibraryScanResult = await _unitOfWork.LibraryScanRepository.UpdateAsync(libraryScanDomainResult.Value.ToRepositoryEntity(), cancellationToken).ConfigureAwait(false);
            if (updateLibraryScanResult.IsFailure)
                throw new EventualConsistencyException(updateLibraryScanResult.FirstError, updateLibraryScanResult.Errors);

            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        // release the scan processing resources, regardless of whether the scan was already marked as failed by a concurrent job
        _mediaLibrariesScanCancellationTracker.RemoveScan(domainEvent.MediaLibraryScanCompositeId);
        _mediaLibrariesScanProgressTracker.RemoveScanProgress(domainEvent.MediaLibraryScanCompositeId);
        Result<Success> clearStagingResult = await _unitOfWork.LibraryScanStagingResultsRepository.ClearForScanAsync(domainEvent.MediaLibraryScanCompositeId.ScanId.Value, cancellationToken).ConfigureAwait(false);
        if (clearStagingResult.IsFailure)
            throw new EventualConsistencyException(clearStagingResult.FirstError, clearStagingResult.Errors);

        // notify SignalR clients that the library scan failed
        await _debouncedLibraryScanProgressNotifier.SendLibraryScanFailedEventAsync(domainEvent.MediaLibraryScanCompositeId, cancellationToken).ConfigureAwait(false);
    }
}
