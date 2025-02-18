#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Events;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Progress;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Mediator;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Core.MediaLibrary.Management.Events;

/// <summary>
/// Handler for the event raised when a media library's scan progress changes.
/// </summary>
public class LibraryScanProgressChangedDomainEventHandler : INotificationHandler<LibraryScanProgressChangedDomainEvent>
{
    private readonly IMediaLibrariesScanProgressTracker _mediaLibrariesScanProgressTracker;

    /// <summary>
    /// Initializes a new instance of the <see cref="LibraryScanProgressChangedDomainEventHandler"/> class.
    /// </summary>
    /// <param name="mediaLibrariesScanProgressTracker">Injected tracker used for media library scans progress.</param>
    public LibraryScanProgressChangedDomainEventHandler(IMediaLibrariesScanProgressTracker mediaLibrariesScanProgressTracker)
    {
        _mediaLibrariesScanProgressTracker = mediaLibrariesScanProgressTracker;
    }

    /// <summary>
    /// Handles the event raised when a media library's scan progress changes.
    /// </summary>
    /// <param name="domainEvent">The domain event to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public async ValueTask Handle(LibraryScanProgressChangedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        _mediaLibrariesScanProgressTracker.UpdateScanProgress(MediaLibraryScanCompositeId.Create(domainEvent.ScanId, domainEvent.UserId));
        await Task.CompletedTask;
    }
}
