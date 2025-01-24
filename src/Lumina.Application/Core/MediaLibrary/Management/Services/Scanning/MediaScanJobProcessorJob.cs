#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Utilities;
using Lumina.Application.Core.MediaLibrary.Management.Services.Scanning.Jobs.Common;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Core.MediaLibrary.Management.Services.Scanning;

/// <summary>
/// Background service for processing media library scan jobs from an in-memory message queue.
/// </summary>
internal sealed class MediaScanJobProcessorJob : BackgroundService
{
    private readonly IMediaScanQueue _mediaScanQueue;

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaScanJobProcessorJob"/> class.
    /// </summary>
    /// <param name="mediaScanQueue">Injected queue used for processing media libraries scan jobs.</param>
    public MediaScanJobProcessorJob(IMediaScanQueue mediaScanQueue)
    {
        _mediaScanQueue = mediaScanQueue;
    }

    /// <summary>
    /// Method called when the background service starts.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        // continuously processes and executes media library scan jobs from the queue until the application shuts down
        // remark: since only top level scan jobs are placed on the bus, but they themselves trigger the execution of their children jobs, and so on,
        // their completion should not be awaited here due to some of them being long running tasks; therefor they are just fired synchronously
        // without being awaited, while still properly handling any exceptions they might throw in their asynchronous execution
        await foreach (MediaScanJob mediaScanJob in _mediaScanQueue.Reader.ReadAllAsync(cancellationToken).ConfigureAwait(false))
            mediaScanJob.ExecuteAsync(Guid.NewGuid(), new {}, cancellationToken).FireAndForgetSafeAsync();
    }
}
