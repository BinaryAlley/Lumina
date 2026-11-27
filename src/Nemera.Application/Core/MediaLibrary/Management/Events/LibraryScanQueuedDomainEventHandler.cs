#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Events;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Events;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Cancellation;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Progress;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Queue;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Core.MediaLibrary.Management.Events;

/// <summary>
/// Handler for the domain event raised when a media library scan is queued.
/// </summary>
public class LibraryScanQueuedDomainEventHandler : IDomainEventHandler<LibraryScanQueuedDomainEvent>
{
    private readonly IMediaLibrariesScanQueue _mediaLibrariesScanQueue;
    private readonly IMediaLibrariesScanCancellationTracker _mediaLibrariesScanCancellationTracker;
    private readonly IMediaLibrariesScanProgressTracker _mediaLibrariesScanProgressTracker;

    /// <summary>
    /// Initializes a new instance of the <see cref="LibraryScanQueuedDomainEventHandler"/> class.
    /// </summary>
    /// <param name="mediaLibrariesScanQueue">Injected queue used for processing the media library scans that are waiting to execute.</param>
    /// <param name="mediaLibrariesScanCancellationTracker">Injected tracker used for canceling media library scans.</param>
    /// <param name="mediaLibrariesScanProgressTracker">Injected tracker used for media library scans progress.</param>
    public LibraryScanQueuedDomainEventHandler(
        IMediaLibrariesScanQueue mediaLibrariesScanQueue,
        IMediaLibrariesScanCancellationTracker mediaLibrariesScanCancellationTracker,
        IMediaLibrariesScanProgressTracker mediaLibrariesScanProgressTracker)
    {
        _mediaLibrariesScanQueue = mediaLibrariesScanQueue;
        _mediaLibrariesScanCancellationTracker = mediaLibrariesScanCancellationTracker;
        _mediaLibrariesScanProgressTracker = mediaLibrariesScanProgressTracker;
    }

    /// <summary>
    /// Handles the event raised when a media library scan is queued.
    /// </summary>
    /// <param name="domainEvent">The domain event to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public async ValueTask HandleAsync(LibraryScanQueuedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        MediaLibraryScanCompositeId compositeId = MediaLibraryScanCompositeId.Create(domainEvent.ScanId, domainEvent.UserId);

        // Register the scan for cancellation before it is queued, so that a cancellation that arrives while the scan is still
        // waiting for its turn is honored when the scan processor finally picks it up.
        _mediaLibrariesScanCancellationTracker.RegisterScan(compositeId);

        // Report the scan as queued, so clients can show it while it waits for the scans ahead of it to finish.
        // This is only a display concern, so it must never prevent the scan from being queued.
        _ = _mediaLibrariesScanProgressTracker.InitializeQueuedScanProgress(domainEvent.LibraryId, compositeId);

        // Only the scan itself is queued, not its jobs. The scan processor prepares and executes the jobs of one scan at a time,
        // so the scans run consecutively, in the order in which they were queued.
        await _mediaLibrariesScanQueue.Writer.WriteAsync(compositeId, cancellationToken).ConfigureAwait(false);
    }
}
