#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Mapping.MediaLibrary.Management;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Cancellation;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Jobs;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Queue;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Infrastructure.Core.MediaLibrary.Management.Scanning.Jobs.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Infrastructure.Core.MediaLibrary.Management.Scanning.Queue;

/// <summary>
/// Background service that executes the queued media library scans one at a time, in the order in which they were queued.
/// </summary>
internal sealed class MediaLibraryScanProcessorJob : BackgroundService
{
    private readonly IMediaLibrariesScanQueue _mediaLibrariesScanQueue;
    private readonly IMediaLibrariesScanCancellationTracker _mediaLibrariesScanCancellationTracker;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<MediaLibraryScanProcessorJob> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaLibraryScanProcessorJob"/> class.
    /// </summary>
    /// <param name="mediaLibrariesScanQueue">Injected queue used for processing the media library scans that are waiting to execute.</param>
    /// <param name="mediaLibrariesScanCancellationTracker">Injected tracker used for canceling media library scans.</param>
    /// <param name="serviceScopeFactory">Injected factory for creating scopes in which services are requested.</param>
    /// <param name="logger">Injected service for logging.</param>
    public MediaLibraryScanProcessorJob(
        IMediaLibrariesScanQueue mediaLibrariesScanQueue,
        IMediaLibrariesScanCancellationTracker mediaLibrariesScanCancellationTracker,
        IServiceScopeFactory serviceScopeFactory,
        ILogger<MediaLibraryScanProcessorJob> logger)
    {
        _mediaLibrariesScanQueue = mediaLibrariesScanQueue;
        _mediaLibrariesScanCancellationTracker = mediaLibrariesScanCancellationTracker;
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    /// <summary>
    /// Method called when the background service starts.
    /// </summary>
    /// <param name="stoppingToken">Cancellation token that can be used to stop the execution.</param>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // The queue is only drained here, one scan at a time: only after the job graph of the current scan completes is the next scan dequeued,
        // so at most one media library scan executes at any moment, regardless of how many scans were queued at once.
        await foreach (MediaLibraryScanCompositeId compositeId in _mediaLibrariesScanQueue.Reader.ReadAllAsync(stoppingToken).ConfigureAwait(false))
        {
            try
            {
                await ProcessScanAsync(compositeId, stoppingToken).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                // A scan that fails must never stop the scans that are queued behind it, so its failure is logged and the queue keeps draining.
                _logger.LogError(exception, "Failed to process the media library scan '{ScanId}'.", compositeId.ScanId.Value);
            }
        }
    }

    /// <summary>
    /// Processes a single queued media library scan, from its preparation to the completion of its job graph.
    /// </summary>
    /// <param name="compositeId">Model for tracking the queued media library scan.</param>
    /// <param name="stoppingToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task ProcessScanAsync(MediaLibraryScanCompositeId compositeId, CancellationToken stoppingToken)
    {
        // Get the token that cancels this scan, and do not start the scan at all when it was already cancelled while it was waiting for its turn.
        CancellationToken scanToken = _mediaLibrariesScanCancellationTracker.GetTokenForScan(compositeId);
        if (scanToken.IsCancellationRequested)
            return;

        // The scan must stop both when the user cancels it and when the application shuts down, so the two are linked into a single token
        // that is passed to everything the scan does. Disposing the source when the scan is over releases its registrations on both tokens.
        using CancellationTokenSource linkedSource = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, scanToken);

        LibraryId? libraryId = null;

        try
        {
            LibraryScan libraryScan;
            LibraryType libraryType;

            // Resolve the scan in a short lived scope, so that its unit of work and its DbContext are released before the scan starts executing.
            await using (AsyncServiceScope readScope = _serviceScopeFactory.CreateAsyncScope())
            {
                IUnitOfWork unitOfWork = readScope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                Result<LibraryScanEntity?> getLibraryScanResult = await unitOfWork.LibraryScanRepository.GetByIdAsync(
                    compositeId.ScanId.Value, shouldTrackEntities: false, cancellationToken: linkedSource.Token).ConfigureAwait(false);
                if (getLibraryScanResult.IsFailure)
                {
                    _logger.LogError("Failed to read the media library scan '{ScanId}': {Error}", compositeId.ScanId.Value, getLibraryScanResult.FirstError.Description);
                    return;
                }

                LibraryScanEntity? libraryScanEntity = getLibraryScanResult.Value;
                // A scan that is not pending any more was cancelled or failed while it was waiting for its turn.
                if (libraryScanEntity is null || libraryScanEntity.Status != LibraryScanJobStatus.Pending)
                    return;

                Result<LibraryScan> libraryScanDomainResult = libraryScanEntity.ToDomainEntity();
                if (libraryScanDomainResult.IsFailure)
                {
                    await ReportScanFailureAsync(compositeId, LibraryId.Create(libraryScanEntity.LibraryId), new InvalidOperationException(libraryScanDomainResult.FirstError.Description), linkedSource.Token).ConfigureAwait(false);
                    return;
                }

                libraryScan = libraryScanDomainResult.Value;
                libraryType = libraryScanEntity.Library.LibraryType;
                libraryId = libraryScan.LibraryId;
            }

            IMediaLibraryScanDiscoveryJob rootJob;
            // The job graph is prepared in its own short lived scope: the jobs create their own scopes for their persistence work.
            // See docs/technical/architecture/architecture-knowledge-management/architecture-decision-log/architecture-decision-record-0001.md for details.
            await using (AsyncServiceScope prepareScope = _serviceScopeFactory.CreateAsyncScope())
            {
                IMediaLibraryScanningService mediaLibraryScanningService = prepareScope.ServiceProvider.GetRequiredService<IMediaLibraryScanningService>();
                Result<IMediaLibraryScanDiscoveryJob> startScanResult = await mediaLibraryScanningService.StartScanAsync(libraryScan, libraryType, linkedSource.Token).ConfigureAwait(false);
                if (startScanResult.IsFailure)
                {
                    // The failure is reported for this scan only, so the scans that are queued behind it are still processed.
                    await ReportScanFailureAsync(compositeId, libraryScan.LibraryId, new InvalidOperationException(startScanResult.FirstError.Description), linkedSource.Token).ConfigureAwait(false);
                    return;
                }
                rootJob = startScanResult.Value;
            }

            _logger.LogInformation("Started the media library scan '{ScanId}' of the media library '{LibraryId}'.", compositeId.ScanId.Value, libraryScan.LibraryId.Value);

            // The file system discovery job is the only top level job of the scan job graph, and every job awaits its children, so awaiting it
            // completes the entire scan. The next queued scan is only dequeued once this call returns.
            await rootJob.ExecuteAsync(Guid.NewGuid(), new { }, linkedSource.Token).ConfigureAwait(false);

            _logger.LogInformation("Completed the media library scan '{ScanId}' of the media library '{LibraryId}'.", compositeId.ScanId.Value, libraryScan.LibraryId.Value);
        }
        catch (OperationCanceledException)
        {
            // The scan was cancelled, or the application is shutting down, and the cancellation handler already reported it. Either way, the queue moves on.
        }
        catch (Exception exception)
        {
            // An unexpected preparation failure must still leave the scan failed instead of pending forever, and must not affect the scans queued behind it.
            // The failure is reported without a cancellation token, because the scan token may already be cancelled and the report must still be published.
            if (libraryId is null)
                _logger.LogError(exception, "Failed to process the media library scan '{ScanId}'.", compositeId.ScanId.Value);
            else
                await ReportScanFailureAsync(compositeId, libraryId, exception, CancellationToken.None).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Reports the failure of a single media library scan, without affecting the scans that are queued behind it.
    /// </summary>
    /// <param name="compositeId">Model for tracking the failed media library scan.</param>
    /// <param name="libraryId">The unique identifier of the library whose scan failed.</param>
    /// <param name="exception">The exception that describes the failure.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task ReportScanFailureAsync(MediaLibraryScanCompositeId compositeId, LibraryId libraryId, Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Failed to start the media library scan '{ScanId}'.", compositeId.ScanId.Value);
        await ScanFailurePublisher.PublishAsync(_serviceScopeFactory, libraryId, compositeId, exception, cancellationToken).ConfigureAwait(false);
    }
}
