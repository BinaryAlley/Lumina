#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using System.Threading.Channels;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Queue;

/// <summary>
/// Interface for the message queue for handling the media library scans that are waiting to execute.
/// Only one media library scan is executed at a time, so the queue guarantees that scans run consecutively.
/// </summary>
public interface IMediaLibrariesScanQueue
{
    /// <summary>
    /// Gets the writer for the queue, allowing producers to enqueue the scans that are waiting to execute.
    /// </summary>
    ChannelWriter<MediaLibraryScanCompositeId> Writer { get; }

    /// <summary>
    /// Gets the reader for the queue, allowing consumers to dequeue the scans that are waiting to execute.
    /// </summary>
    ChannelReader<MediaLibraryScanCompositeId> Reader { get; }
}
