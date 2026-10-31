#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Events;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Cancellation;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Jobs;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Progress;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Scanners;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.UserManagementBoundedContext.UserAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services;

/// <summary>
/// Service used to scan a media library.
/// </summary>
internal class MediaLibraryScanningService : IMediaLibraryScanningService
{
    private readonly IMediaLibraryScannerFactory _mediaLibraryScannerFactory;
    private readonly IMediaLibrariesScanCancellationTracker _mediaLibrariesScanCancellationTracker;
    private readonly IMediaLibrariesScanProgressTracker _mediaLibrariesScanProgressTracker;
    private readonly IDomainEventPublisher _domainEventPublisher;

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaLibraryScanningService"/> class.
    /// </summary>
    /// <param name="mediaLibraryScannerFactory">Injected factory for creating media library scanners.</param>
    /// <param name="mediaLibrariesScanCancellationTracker">Injected tracker used for canceling media library scans.</param>
    /// <param name="mediaLibrariesScanProgressTracker">Injected tracker used for media library scans progress.</param>
    /// <param name="domainEventPublisher">The domain event publisher used to publish events.</param>
    public MediaLibraryScanningService(
        IMediaLibraryScannerFactory mediaLibraryScannerFactory,
        IMediaLibrariesScanCancellationTracker mediaLibrariesScanCancellationTracker,
        IMediaLibrariesScanProgressTracker mediaLibrariesScanProgressTracker,
        IDomainEventPublisher domainEventPublisher)
    {
        _mediaLibraryScannerFactory = mediaLibraryScannerFactory;
        _mediaLibrariesScanCancellationTracker = mediaLibrariesScanCancellationTracker;
        _mediaLibrariesScanProgressTracker = mediaLibrariesScanProgressTracker;
        _domainEventPublisher = domainEventPublisher;
    }

    /// <summary>
    /// Starts <paramref name="scan"/> and prepares its job graph.
    /// </summary>
    /// <param name="scan">The media library scan to start.</param>
    /// <param name="libraryType">The type of the media library to be scanned.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either the file system discovery job, which is the only top level job of the
    /// media library scan job graph and whose execution completes the entire scan, or an error.
    /// </returns>
    public async Task<Result<IMediaLibraryScanDiscoveryJob>> StartScanAsync(LibraryScan scan, LibraryType libraryType, CancellationToken cancellationToken)
    {
        // Resolve the scanner before changing any state, so that a media library type without a scanner fails its own scan,
        // without leaving it started and without affecting the other scans that are queued behind it.
        Result<IMediaTypeScanner> getScannerResult = _mediaLibraryScannerFactory.CreateLibraryScanner(libraryType);
        if (getScannerResult.IsFailure)
            return getScannerResult.Errors;

        // Build the job graph before the scan starts, so that its jobs already carry the scan identity when they report their progress,
        // and so that the total number of jobs is known for the progress tracking.
        IMediaLibraryScanDiscoveryJob rootJob = getScannerResult.Value.CreateScanJobsForLibrary(scan.LibraryId);
        SetScanPropertiesForJobChain(rootJob, scan.Id, scan.UserId);
        int totalJobs = CountTotalJobs(rootJob);

        // Mark the scan itself as started.
        Result<Success> startScanResult = scan.StartScan();
        if (startScanResult.IsFailure)
            return startScanResult.Errors;

        MediaLibraryScanCompositeId compositeId = MediaLibraryScanCompositeId.Create(scan.Id, scan.UserId);

        // Register the scan in the tracker for cancellation tokens.
        _mediaLibrariesScanCancellationTracker.RegisterScan(compositeId);

        // Initialize the progress of the scan before publishing its events, so that the handlers of the started event can already report it.
        Result<Created> initializeScanProgressResult = _mediaLibrariesScanProgressTracker.InitializeScanProgress(scan.LibraryId, compositeId, totalJobs);
        if (initializeScanProgressResult.IsFailure)
            return initializeScanProgressResult.Errors;

        foreach (IDomainEvent domainEvent in scan.GetDomainEvents())
            await _domainEventPublisher.PublishAsync(domainEvent, cancellationToken).ConfigureAwait(false);

        return Result.From<IMediaLibraryScanDiscoveryJob>(rootJob);
    }

    /// <summary>
    /// Cancels <paramref name="scan"/>.
    /// </summary>
    /// <param name="scan">The media library scan to cancel.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public Result<Success> CancelScan(LibraryScan scan)
    {
        // Trigger the process that cancels the jobs of the scan.
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
        // Recursively set properties for all children.
        foreach (IMediaLibraryScanJob childJob in job.Children)
            SetScanPropertiesForJobChain(childJob, scanId, userId);
    }

    /// <summary>
    /// Counts the total number of unique media library scan jobs in the job graph that starts at <paramref name="rootJob"/>.
    /// </summary>
    /// <param name="rootJob">The file system discovery job, which is the only top level job of the scan job graph.</param>
    /// <returns>The total count of unique media library scan jobs.</returns>
    private static int CountTotalJobs(IMediaLibraryScanDiscoveryJob rootJob)
    {
        HashSet<IMediaLibraryScanJob> uniqueJobs = [];
        TraverseJobGraph(rootJob, uniqueJobs);
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
