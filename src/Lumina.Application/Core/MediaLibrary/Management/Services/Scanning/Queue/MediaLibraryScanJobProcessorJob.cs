#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Utilities;
using Lumina.Application.Core.MediaLibrary.Management.Services.Scanning.Jobs.Common;
using Lumina.Application.Core.MediaLibrary.Management.Services.Scanning.Tracking;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Core.MediaLibrary.Management.Services.Scanning.Queue;

/// <summary>
/// Background service for processing media library scan jobs from an in-memory message queue.
/// </summary>
internal sealed class MediaLibraryScanJobProcessorJob : BackgroundService
{
    private readonly IMediaLibrariesScanQueue _mediaScanQueue;
    private readonly IMediaLibrariesScanTracker _librariesScanTracker;

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaLibraryScanJobProcessorJob"/> class.
    /// </summary>
    /// <param name="mediaScanQueue">Injected queue used for processing media libraries scan jobs.</param>
    public MediaLibraryScanJobProcessorJob(IMediaLibrariesScanQueue mediaScanQueue, IMediaLibrariesScanTracker librariesScanTracker)
    {
        _mediaScanQueue = mediaScanQueue;
        _librariesScanTracker = librariesScanTracker;
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
        await foreach (MediaLibraryScanJob mediaScanJob in _mediaScanQueue.Reader.ReadAllAsync(cancellationToken).ConfigureAwait(false))
        {
            // get the cancellation token that was associated with this library scan
            CancellationToken scanToken = _librariesScanTracker.GetTokenForScan(mediaScanJob.ScanId, mediaScanJob.UserId);
            // create linked token to handle both scan cancellation and service shutdown
            CancellationTokenSource linkedSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, scanToken);
            mediaScanJob.ExecuteAsync(Guid.NewGuid(), new { }, linkedSource.Token).FireAndForgetSafeAsync(() => linkedSource.Dispose());
        }
    }
}
