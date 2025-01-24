#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.MediaLibrary.Management.Services.Scanning.Jobs.Common;
using System.Threading.Channels;
#endregion

namespace Lumina.Application.Core.MediaLibrary.Management.Services.Scanning;

/// <summary>
/// Interface for the message queue for handling <see cref="MediaScanJob"/> instances.
/// </summary>
internal interface IMediaScanQueue
{
    /// <summary>
    /// Gets the writer for the queue, allowing producers to enqueue <see cref="MediaScanJob"/> instances.
    /// </summary>
    public ChannelWriter<MediaScanJob> Writer { get; }

    /// <summary>
    /// Gets the reader for the queue, allowing consumers to dequeue <see cref="MediaScanJob"/> instances.
    /// </summary>
    public ChannelReader<MediaScanJob> Reader { get; }
}
