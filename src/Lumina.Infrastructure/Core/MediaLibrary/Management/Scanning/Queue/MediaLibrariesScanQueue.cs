#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Jobs;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Queue;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using System.Threading.Channels;
#endregion

namespace Lumina.Infrastructure.Core.MediaLibrary.Management.Scanning.Queue;

/// <summary>
/// Provides an in-memory message queue for handling the media library scans that are waiting to execute.
/// </summary>
internal class MediaLibrariesScanQueue : IMediaLibrariesScanQueue
{
    private readonly Channel<MediaLibraryScanCompositeId> _channel = Channel.CreateUnbounded<MediaLibraryScanCompositeId>();

    /// <summary>
    /// Gets the writer for the queue, allowing producers to enqueue the scans that are waiting to execute.
    /// </summary>
    public ChannelWriter<MediaLibraryScanCompositeId> Writer => _channel.Writer;

    /// <summary>
    /// Gets the reader for the queue, allowing consumers to dequeue the scans that are waiting to execute.
    /// </summary>
    public ChannelReader<MediaLibraryScanCompositeId> Reader => _channel.Reader;
}
