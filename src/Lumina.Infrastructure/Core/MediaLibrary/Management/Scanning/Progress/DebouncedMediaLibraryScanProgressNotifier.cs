#region ========================================================================= USING =====================================================================================
using DebounceThrottle;
using ErrorOr;
using Lumina.Application.Common.Mapping.MediaLibrary.Management;
using Lumina.Application.Core.MediaLibrary.Management.Progress;
using Lumina.Contracts.Responses.MediaLibrary.Management;
using Lumina.Domain.Common.Enums.MediaLibrary;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Progress;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Infrastructure.Core.MediaLibrary.Management.Scanning.Progress;

/// <summary>
/// Class used to notify SignalR clients about changes in the progress of a media library scan.
/// </summary>
public sealed class DebouncedMediaLibraryScanProgressNotifier : IMediaLibraryScanProgressNotifier
{
    private readonly IHubContext<MediaLibraryScanProgressHub> _hubContext;
    private readonly IMediaLibrariesScanProgressTracker _mediaLibrariesScanProgressTracker;
    private readonly ConcurrentDictionary<MediaLibraryScanCompositeId, DebounceDispatcher> _debouncers = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="DebouncedMediaLibraryScanProgressNotifier"/> class.
    /// </summary>
    /// <param name="hubContext">SignalR hub context abstraction.</param>
    /// <param name="mediaLibrariesScanProgressTracker">Injected service for tracking the progress of media library scans.</param>
    public DebouncedMediaLibraryScanProgressNotifier(IHubContext<MediaLibraryScanProgressHub> hubContext, IMediaLibrariesScanProgressTracker mediaLibrariesScanProgressTracker)
    {
        _hubContext = hubContext;
        _mediaLibrariesScanProgressTracker = mediaLibrariesScanProgressTracker;
    }

    /// <summary>
    /// Notifies the SignalR clients about a change in the progress of a media library scan.
    /// </summary>
    /// <param name="mediaLibraryScanCompositeId">The object representing the unique identifier for the media library scan whose progress changed.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public async Task SendLibraryProgressUpdateEventAsync(MediaLibraryScanCompositeId mediaLibraryScanCompositeId, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return;

        // throttle network traffic to no more than 20 updates a second, any more are pointless from a progress report perspective
        DebounceDispatcher debouncer = _debouncers.GetOrAdd(mediaLibraryScanCompositeId, _ => new DebounceDispatcher(TimeSpan.FromMilliseconds(50)));
        await debouncer.DebounceAsync(async () =>
        {
            // the token might be canceled after initial check but before debounced execution starts
            if (cancellationToken.IsCancellationRequested)
                return;

            ErrorOr<MediaLibraryScanProgress> progressResult = _mediaLibrariesScanProgressTracker.GetScanProgress(mediaLibraryScanCompositeId);
            if (progressResult.IsError)
                return;

            MediaLibraryScanProgressResponse progress = progressResult.Value.ToResponse();
            await _hubContext.Clients.Group(mediaLibraryScanCompositeId.ToString()).SendAsync("libraryScanProgressUpdateEvent", progress, cancellationToken).ConfigureAwait(false);
        }, cancellationToken).ConfigureAwait(false);        
    }
   
    /// <summary>
    /// Notifies the SignalR clients about the completion of a media library scan.
    /// </summary>
    /// <param name="mediaLibraryScanCompositeId">The object representing the unique identifier for the media library scan that completed.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public async Task SendLibraryScanFinishedEventAsync(MediaLibraryScanCompositeId mediaLibraryScanCompositeId, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return;
        ErrorOr<MediaLibraryScanProgress> progressResult = _mediaLibrariesScanProgressTracker.RemoveScanProgress(mediaLibraryScanCompositeId);
        
        CleanupDebouncer(mediaLibraryScanCompositeId);

        if (!progressResult.IsError)
        {
            MediaLibraryScanProgressResponse progress = progressResult.Value.ToResponse();
            await _hubContext.Clients.Group(mediaLibraryScanCompositeId.ToString()).SendAsync("libraryScanFinishedEvent", progress, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Notifies the SignalR clients about the failure of a media library scan.
    /// </summary>
    /// <param name="mediaLibraryScanCompositeId">The object representing the unique identifier for the media library scan that failed.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public async Task SendLibraryScanFailedEventAsync(MediaLibraryScanCompositeId mediaLibraryScanCompositeId, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return;
        ErrorOr<MediaLibraryScanProgress> progressResult = _mediaLibrariesScanProgressTracker.RemoveScanProgress(mediaLibraryScanCompositeId);

        CleanupDebouncer(mediaLibraryScanCompositeId);
        
        if (!progressResult.IsError)
        {
            MediaLibraryScanProgressResponse progress = progressResult.Value.ToResponse();
            await _hubContext.Clients.Group(mediaLibraryScanCompositeId.ToString()).SendAsync(
                "libraryScanFailedEvent", progress with { Status = LibraryScanJobStatus.Failed.ToString() }, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Disposes the debouncer that was used in a scan identified by <paramref name="mediaLibraryScanCompositeId"/>, that is now completed.
    /// </summary>
    /// <param name="mediaLibraryScanCompositeId">The object representing the unique identifier for the media library scan whose debouncer needs disposing.</param>
    private void CleanupDebouncer(MediaLibraryScanCompositeId mediaLibraryScanCompositeId)
    {
        if (_debouncers.TryRemove(mediaLibraryScanCompositeId, out DebounceDispatcher? debouncer))
            debouncer.Dispose();
    }
}
