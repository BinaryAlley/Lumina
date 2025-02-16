#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Jobs;
using System.Threading.Channels;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Queue;

/// <summary>
/// Interface for the message queue for handling <see cref="IMediaLibraryScanJob"/> instances.
/// </summary>
public interface IMediaLibrariesScanQueue
{
    /// <summary>
    /// Gets the writer for the queue, allowing producers to enqueue <see cref="IMediaLibraryScanJob"/> instances.
    /// </summary>
    public ChannelWriter<IMediaLibraryScanJob> Writer { get; }

    /// <summary>
    /// Gets the reader for the queue, allowing consumers to dequeue <see cref="IMediaLibraryScanJob"/> instances.
    /// </summary>
    public ChannelReader<IMediaLibraryScanJob> Reader { get; }
}
