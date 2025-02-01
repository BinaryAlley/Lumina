#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.MediaLibrary.Management.Services.Scanning.Jobs.Common;
using System.Threading.Channels;
#endregion

namespace Lumina.Application.Core.MediaLibrary.Management.Services.Scanning.Queue;

/// <summary>
/// Provides an in-memory message queue for handling <see cref="MediaLibraryScanJob"/> instances.
/// </summary>
internal class MediaLibrariesScanQueue : IMediaLibrariesScanQueue
{
    private readonly Channel<MediaLibraryScanJob> _channel = Channel.CreateUnbounded<MediaLibraryScanJob>();

    /// <summary>
    /// Gets the writer for the queue, allowing producers to enqueue <see cref="MediaLibraryScanJob"/> instances.
    /// </summary>
    public ChannelWriter<MediaLibraryScanJob> Writer
    {
        get { return _channel.Writer; }
    }

    /// <summary>
    /// Gets the reader for the queue, allowing consumers to dequeue <see cref="MediaLibraryScanJob"/> instances.
    /// </summary>
    public ChannelReader<MediaLibraryScanJob> Reader
    {
        get { return _channel.Reader; }
    }
}
