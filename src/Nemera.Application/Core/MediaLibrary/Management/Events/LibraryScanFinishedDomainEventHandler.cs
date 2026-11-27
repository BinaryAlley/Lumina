#region ========================================================================= USING =====================================================================================
using Nemera.Domain.Common.Primitives;
using Nemera.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Nemera.Application.Common.DataAccess.Repositories.MediaLibrary;
using Nemera.Application.Common.DataAccess.UoW;
using Nemera.Application.Common.Mapping.MediaLibrary.Management;
using Nemera.Application.Core.MediaLibrary.Management.Progress;
using Nemera.Domain.Common.Errors;
using Nemera.Domain.Common.Events;
using Nemera.Domain.Common.Exceptions;
using Nemera.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate;
using Nemera.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Events;
using Nemera.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Cancellation;
using Nemera.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Progress;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Nemera.Application.Core.MediaLibrary.Management.Events;

/// <summary>
/// Handler for the domain event raised when a media library scan is finished.
/// </summary>
public class LibraryScanFinishedDomainEventHandler : IDomainEventHandler<LibraryScanFinishedDomainEvent>
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
    public async ValueTask HandleAsync(LibraryScanFinishedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        // Get the library scan from the repository.
        Result<LibraryScanEntity?> getLibraryScansResult = await _unitOfWork.LibraryScanRepository.GetByIdAsync(
            domainEvent.MediaLibraryScanCompositeId.ScanId.Value, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (getLibraryScansResult.IsFailure)
            throw new EventualConsistencyException(getLibraryScansResult.FirstError, getLibraryScansResult.Errors);
        if (getLibraryScansResult.Value is null)
            throw new EventualConsistencyException(Errors.LibraryScanning.LibraryScanNotFound);

        // Convert the repository scan to a domain object.
        Result<LibraryScan> libraryScanDomainResult = getLibraryScansResult.Value.ToDomainEntity();
        if (libraryScanDomainResult.IsFailure)
            throw new EventualConsistencyException(libraryScanDomainResult.FirstError, libraryScanDomainResult.Errors);

        // Mark the media library scan as finished.
        Result<Success> finishScanResult = libraryScanDomainResult.Value.FinishScan();
        if (finishScanResult.IsFailure)
            throw new EventualConsistencyException(finishScanResult.FirstError, finishScanResult.Errors);

        // Update the library scan in the repository.
        Result<Updated> updateLibraryScanResult = await _unitOfWork.LibraryScanRepository.UpdateAsync(libraryScanDomainResult.Value.ToRepositoryEntity(), cancellationToken).ConfigureAwait(false);
        if (updateLibraryScanResult.IsFailure)
            throw new EventualConsistencyException(updateLibraryScanResult.FirstError, updateLibraryScanResult.Errors);

        Result<Success> saveChangesResult = await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        if (saveChangesResult.IsFailure)
            throw new EventualConsistencyException(saveChangesResult.FirstError, saveChangesResult.Errors);

        // Mark the scan progress as finished.
        _mediaLibrariesScanProgressTracker.UpdateScanProgress(libraryScanDomainResult.Value.LibraryId, domainEvent.MediaLibraryScanCompositeId);
        // Notify SignalR clients that the library scan finished.
        await _debouncedLibraryScanProgressNotifier.SendLibraryScanFinishedEventAsync(domainEvent.MediaLibraryScanCompositeId, cancellationToken).ConfigureAwait(false);
        // Only when everything is done, this needs to be called in order to dispose cancellation token sources, etc.
        _mediaLibrariesScanCancellationTracker.RemoveScan(domainEvent.MediaLibraryScanCompositeId);
    }
}
