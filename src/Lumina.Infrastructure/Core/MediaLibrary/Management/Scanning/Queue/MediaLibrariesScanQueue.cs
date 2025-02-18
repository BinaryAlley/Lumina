#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Jobs;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Queue;
using System.Threading.Channels;
#endregion

namespace Lumina.Application.Core.MediaLibrary.Management.Services.Scanning.Queue;

/// <summary>
/// Provides an in-memory message queue for handling <see cref="IMediaLibraryScanJob"/> instances.
/// </summary>
internal class MediaLibrariesScanQueue : IMediaLibrariesScanQueue
{
    private readonly Channel<IMediaLibraryScanJob> _channel = Channel.CreateUnbounded<IMediaLibraryScanJob>();

    /// <summary>
    /// Gets the writer for the queue, allowing producers to enqueue <see cref="IMediaLibraryScanJob"/> instances.
    /// </summary>
    public ChannelWriter<IMediaLibraryScanJob> Writer => _channel.Writer;

    /// <summary>
    /// Gets the reader for the queue, allowing consumers to dequeue <see cref="IMediaLibraryScanJob"/> instances.
    /// </summary>
    public ChannelReader<IMediaLibraryScanJob> Reader => _channel.Reader;
}
