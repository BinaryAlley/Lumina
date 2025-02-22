#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.MediaLibrary.Management.Progress;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Events;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Progress;
using Mediator;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Core.MediaLibrary.Management.Events;

/// <summary>
/// Handler for the event raised when a media library's scan progress changes.
/// </summary>
public class LibraryScanJobProgressChangedDomainEventHandler : INotificationHandler<LibraryScanJobProgressChangedDomainEvent>
{
    private readonly IMediaLibrariesScanProgressTracker _mediaLibrariesScanProgressTracker;
    private readonly IMediaLibraryScanProgressNotifier _debouncedLibraryScanProgressNotifier;

    /// <summary>
    /// Initializes a new instance of the <see cref="LibraryScanJobProgressChangedDomainEventHandler"/> class.
    /// </summary>
    /// <param name="mediaLibrariesScanProgressTracker">Injected tracker used for media library scans progress.</param>
    /// <param name="debouncedLibraryScanProgressNotifier">Injected service for notifying media libraries scan progress changes to third parties.</param>
    public LibraryScanJobProgressChangedDomainEventHandler(
        IMediaLibrariesScanProgressTracker mediaLibrariesScanProgressTracker,
        IMediaLibraryScanProgressNotifier debouncedLibraryScanProgressNotifier)
    {
        _mediaLibrariesScanProgressTracker = mediaLibrariesScanProgressTracker;
        _debouncedLibraryScanProgressNotifier = debouncedLibraryScanProgressNotifier;
    }

    /// <summary>
    /// Handles the event raised when a media library's scan progress changes.
    /// </summary>
    /// <param name="domainEvent">The domain event to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public async ValueTask Handle(LibraryScanJobProgressChangedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        _mediaLibrariesScanProgressTracker.UpdateScanJobProgress(domainEvent.LibraryId, domainEvent.MediaLibraryScanCompositeId, domainEvent.Progress);
        // notify SignalR clients that the library scan progress changed
        await _debouncedLibraryScanProgressNotifier.SendLibraryProgressUpdateEventAsync(domainEvent.MediaLibraryScanCompositeId, cancellationToken).ConfigureAwait(false);
    }
}
