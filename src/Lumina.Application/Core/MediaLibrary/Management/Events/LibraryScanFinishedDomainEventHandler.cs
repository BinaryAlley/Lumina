#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Repositories.MediaLibrary;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Mapping.MediaLibrary.Management;
using Lumina.Application.Core.MediaLibrary.Management.Progress;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Exceptions;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Events;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Cancellation;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Progress;
using Mediator;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Core.MediaLibrary.Management.Events;

/// <summary>
/// Handler for the event raised when a media library scan is finished.
/// </summary>
public class LibraryScanFinishedDomainEventHandler : INotificationHandler<LibraryScanFinishedDomainEvent>
{
    private readonly IMediaLibrariesScanProgressTracker _mediaLibrariesScanProgressTracker;
    private readonly IMediaLibrariesScanCancellationTracker _mediaLibrariesScanCancellationTracker;
    private readonly IMediaLibraryScanProgressNotifier _debouncedLibraryScanProgressNotifier;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the <see cref="LibraryScanFinishedDomainEventHandler"/> class.
    /// </summary>
    /// <param name="mediaLibrariesScanProgressTracker">Injected service for tracking the progress of media library scans.</param>
    /// <param name="mediaLibrariesScanCancellationTracker">Injected tracker used for canceling media library scans.</param>
    /// <param name="debouncedLibraryScanProgressNotifier">Injected service for notifying media libraries scan progress changes to third parties.</param>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    public LibraryScanFinishedDomainEventHandler(
        IMediaLibrariesScanProgressTracker mediaLibrariesScanProgressTracker,
        IMediaLibrariesScanCancellationTracker mediaLibrariesScanCancellationTracker,
        IMediaLibraryScanProgressNotifier debouncedLibraryScanProgressNotifier,
        IUnitOfWork unitOfWork)
    {
        _mediaLibrariesScanProgressTracker = mediaLibrariesScanProgressTracker;
        _mediaLibrariesScanCancellationTracker = mediaLibrariesScanCancellationTracker;
        _debouncedLibraryScanProgressNotifier = debouncedLibraryScanProgressNotifier;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Handles the event raised when a media library scan is finished.
    /// </summary>
    /// <param name="domainEvent">The domain event to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public async ValueTask Handle(LibraryScanFinishedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        ILibraryScanRepository libraryScanRepository = _unitOfWork.GetRepository<ILibraryScanRepository>();
       
        // get the library scan from the repository
        ErrorOr<LibraryScanEntity?> getLibraryScansResult = await libraryScanRepository.GetByIdAsync(
            domainEvent.MediaLibraryScanCompositeId.ScanId.Value, cancellationToken).ConfigureAwait(false);
        if (getLibraryScansResult.IsError)
            throw new EventualConsistencyException(getLibraryScansResult.FirstError, getLibraryScansResult.Errors);
        if (getLibraryScansResult.Value is null)
            throw new EventualConsistencyException(Errors.LibraryScanning.LibraryScanNotFound);

        // convert the repository scan to a domain object
        ErrorOr<LibraryScan> libraryScanDomainResult = getLibraryScansResult.Value.ToDomainEntity();
        if (libraryScanDomainResult.IsError)
            throw new EventualConsistencyException(libraryScanDomainResult.FirstError, libraryScanDomainResult.Errors);

        // mark the media library scan as finished
        ErrorOr<Success> finishScanResult = libraryScanDomainResult.Value.FinishScan();
        if (finishScanResult.IsError)
            throw new EventualConsistencyException(finishScanResult.FirstError, finishScanResult.Errors);

        // update the library scan in the repository
        ErrorOr<Updated> updateLibraryScanResult = await libraryScanRepository.UpdateAsync(libraryScanDomainResult.Value.ToRepositoryEntity(), cancellationToken).ConfigureAwait(false);
        if (updateLibraryScanResult.IsError)
            throw new EventualConsistencyException(updateLibraryScanResult.FirstError, updateLibraryScanResult.Errors);

        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        // mark the scan progress as finished
        _mediaLibrariesScanProgressTracker.UpdateScanProgress(libraryScanDomainResult.Value.LibraryId, domainEvent.MediaLibraryScanCompositeId);
        // notify SignalR clients that the library scan finished
        await _debouncedLibraryScanProgressNotifier.SendLibraryScanFinishedEventAsync(domainEvent.MediaLibraryScanCompositeId, cancellationToken).ConfigureAwait(false);
        // only when everything is done, this needs to be called in order to dispose cancellation token sources, etc
        _mediaLibrariesScanCancellationTracker.RemoveScan(domainEvent.MediaLibraryScanCompositeId);
    }
}
