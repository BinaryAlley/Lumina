#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Core.MediaLibrary.Management.Progress;

/// <summary>
/// Interface for the class used to notify SignalR clients about changes in the progress of a media library scan.
/// </summary>
public interface IMediaLibraryScanProgressNotifier
{
    /// <summary>
    /// Notifies the SignalR clients about a change in the progress of a media library scan.
    /// </summary>
    /// <param name="mediaLibraryScanCompositeId">The object representing the unique identifier for the media library scan whose progress changed.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    Task SendLibraryProgressUpdateEventAsync(MediaLibraryScanCompositeId mediaLibraryScanCompositeId, CancellationToken cancellationToken);

    /// <summary>
    /// Notifies the SignalR clients about the completion of a media library scan.
    /// </summary>
    /// <param name="mediaLibraryScanCompositeId">The object representing the unique identifier for the media library scan that completed.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    Task SendLibraryScanFinishedEventAsync(MediaLibraryScanCompositeId mediaLibraryScanCompositeId, CancellationToken cancellationToken);

    /// <summary>
    /// Notifies the SignalR clients about the failure of a media library scan.
    /// </summary>
    /// <param name="mediaLibraryScanCompositeId">The object representing the unique identifier for the media library scan that failed.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    Task SendLibraryScanFailedEventAsync(MediaLibraryScanCompositeId mediaLibraryScanCompositeId, CancellationToken cancellationToken);
}
