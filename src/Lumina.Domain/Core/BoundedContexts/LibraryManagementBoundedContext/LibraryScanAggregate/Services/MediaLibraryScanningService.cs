#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Domain.Common.Enums.MediaLibrary;
using Lumina.Domain.Common.Events;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Cancellation;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Jobs;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Progress;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Queue;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Scanners;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.UserManagementBoundedContext.UserAggregate.ValueObjects;
using Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services;

/// <summary>
/// Service used to scan a media library.
/// </summary>
internal class MediaLibraryScanningService : IMediaLibraryScanningService
{
    private readonly IMediaLibrariesScanQueue _mediaLibrariesScanQueue;
    private readonly IMediaLibraryScannerFactory _mediaLibraryScannerFactory;
    private readonly IMediaLibrariesScanCancellationTracker _mediaLibrariesScanCancellationTracker;
    private readonly IMediaLibrariesScanProgressTracker _mediaLibrariesScanProgressTracker;
    private readonly IPublisher _publisher;

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaLibraryScanningService"/> class.
    /// </summary>
    /// <param name="mediaLibrariesScanQueue">Injected queue used for processing media libraries scan jobs.</param>
    /// <param name="mediaLibraryScannerFactory">Injected factory for creating media library scanners.</param>
    /// <param name="mediaLibrariesScanCancellationTracker">Injected tracker used for canceling media library scans.</param>
    /// <param name="mediaLibrariesScanProgressTracker">Injected tracker used for media library scans progress.</param>
    /// <param name="publisher">The domain event publisher used to publish events.</param>
    public MediaLibraryScanningService(
        IMediaLibrariesScanQueue mediaLibrariesScanQueue,
        IMediaLibraryScannerFactory mediaLibraryScannerFactory,
        IMediaLibrariesScanCancellationTracker mediaLibrariesScanCancellationTracker,
        IMediaLibrariesScanProgressTracker mediaLibrariesScanProgressTracker,
        IPublisher publisher)
    {
        _mediaLibrariesScanQueue = mediaLibrariesScanQueue;
        _mediaLibraryScannerFactory = mediaLibraryScannerFactory;
        _mediaLibrariesScanCancellationTracker = mediaLibrariesScanCancellationTracker;
        _mediaLibrariesScanProgressTracker = mediaLibrariesScanProgressTracker;
        _publisher = publisher;
    }

    /// <summary>
    /// Starts <paramref name="scan"/>.
    /// </summary>
    /// <param name="scan">The media library scan to start.</param>
    /// <param name="libraryType">The type of the media library to be scanned.</param>
    /// <param name="downloadMedatadaFromWeb">Whether the library permits downloading data from the web, or not.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<ErrorOr<Success>> StartScanAsync(LibraryScan scan, LibraryType libraryType, bool downloadMedatadaFromWeb, CancellationToken cancellationToken)
    {
        // mark the scan itself as started
        ErrorOr<Success> startScanResult = scan.StartScan();
        if (startScanResult.IsError)
            return startScanResult.Errors;

        foreach (IDomainEvent domainEvent in scan.GetDomainEvents())
            await _publisher!.Publish(domainEvent, cancellationToken).ConfigureAwait(false);
        
        // register the scan in the tracker for cancellation tokens
        _mediaLibrariesScanCancellationTracker.RegisterScan(MediaLibraryScanCompositeId.Create(scan.Id, scan.UserId));

        try
        {
            // link the user's cancellation token with the scan's token
            CancellationTokenSource linkedSource = CancellationTokenSource
               .CreateLinkedTokenSource(cancellationToken, _mediaLibrariesScanCancellationTracker.GetTokenForScan(MediaLibraryScanCompositeId.Create(scan.Id, scan.UserId)));

            // get a media library scanner for the provided media library type
            IMediaTypeScanner scanner = _mediaLibraryScannerFactory.CreateLibraryScanner(libraryType);
            // get the list of scan jobs for the retrieved scanner
            List<IMediaLibraryScanJob> jobs = scanner.CreateScanJobsForLibrary(scan.LibraryId, downloadMedatadaFromWeb).ToList();

            // count total jobs in the chain by traversing the job graph
            int totalJobs = CountTotalJobs(jobs);
            _mediaLibrariesScanProgressTracker.InitializeScanProgress(scan.LibraryId, MediaLibraryScanCompositeId.Create(scan.Id, scan.UserId), totalJobs);

            // enqueue the jobs on the channel from where they will be processed
            foreach (IMediaLibraryScanJob job in jobs)
            {
                // set the Id of the scan and of the user initiating the scan to all jobs in the scan job graph
                SetScanPropertiesForJobChain(job, scan.Id, scan.UserId);
                await _mediaLibrariesScanQueue.Writer.WriteAsync(job, linkedSource.Token).ConfigureAwait(false);
            }
        }
        catch (NotImplementedException) { }
        return Result.Success;
    }

    /// <summary>
    /// Cancels <paramref name="scan"/>.
    /// </summary>
    /// <param name="scan">The media library scan to cancel.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> representing either a successful operation, or an error.</returns>
    public ErrorOr<Success> CancelScan(LibraryScan scan)
    {
        // trigger the process that cancels the jobs of the scan
        _mediaLibrariesScanCancellationTracker.CancelScan(MediaLibraryScanCompositeId.Create(scan.Id, scan.UserId));
        return Result.Success;
    }

    /// <summary>
    /// Sets the scan properties for a job and all its child jobs in a recursive manner.
    /// </summary>
    /// <param name="job">The media library scan job whose properties are to be set.</param>
    /// <param name="scanId">The object representing the unique identifier for the scan.</param>
    /// <param name="userId">The object representing the unique identifier for the user that initiated the scan.</param>
    public static void SetScanPropertiesForJobChain(IMediaLibraryScanJob job, ScanId scanId, UserId userId)
    {
        job.ScanId = scanId;
        job.UserId = userId;
        // recursively set properties for all children
        foreach (IMediaLibraryScanJob childJob in job.Children)
            SetScanPropertiesForJobChain(childJob, scanId, userId);
    }

    /// <summary>
    /// Counts the total number of unique media library scan jobs in a list of root jobs.
    /// </summary>
    /// <param name="rootJobs">A list of root media library scan jobs to traverse.</param>
    /// <returns>The total count of unique media library scan jobs.</returns>
    private static int CountTotalJobs(List<IMediaLibraryScanJob> rootJobs)
    {
        HashSet<IMediaLibraryScanJob> uniqueJobs = [];
        foreach (IMediaLibraryScanJob job in rootJobs)
            TraverseJobGraph(job, uniqueJobs);
        return uniqueJobs.Count;
    }

    /// <summary>
    /// Traverses the job graph recursively and adds each job to a set of unique jobs.
    /// </summary>
    /// <param name="job">The current media library scan job being traversed.</param>
    /// <param name="uniqueJobs">A hash set that stores unique media library scan jobs.</param>
    private static void TraverseJobGraph(IMediaLibraryScanJob job, HashSet<IMediaLibraryScanJob> uniqueJobs)
    {
        uniqueJobs.Add(job);
        foreach (IMediaLibraryScanJob child in job.Children)
            TraverseJobGraph(child, uniqueJobs);
    }
}
