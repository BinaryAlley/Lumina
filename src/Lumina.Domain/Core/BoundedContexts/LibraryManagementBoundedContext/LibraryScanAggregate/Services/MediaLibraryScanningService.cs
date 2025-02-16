#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Domain.Common.Enums.MediaLibrary;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Cancellation;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Jobs;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Queue;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Scanners.Common;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
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
    private readonly IMediaLibrariesScanQueue _mediaScanQueue;
    private readonly IMediaLibraryScannerFactory _libraryScannerFactory;
    private readonly IMediaLibrariesScanCancellationTracker _mediaLibrariesScanCancellationTracker;

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaLibraryScanningService"/> class.
    /// </summary>
    /// <param name="mediaScanQueue">Injected queue used for processing media libraries scan jobs.</param>
    /// <param name="libraryScannerFactory">Injected factory for creating media library scanners.</param>
    public MediaLibraryScanningService(
        IMediaLibrariesScanQueue mediaScanQueue,
        IMediaLibraryScannerFactory libraryScannerFactory,
        IMediaLibrariesScanCancellationTracker mediaLibrariesScanCancellationTracker)
    {
        _mediaScanQueue = mediaScanQueue;
        _libraryScannerFactory = libraryScannerFactory;
        _mediaLibrariesScanCancellationTracker = mediaLibrariesScanCancellationTracker;
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
        // register the scan in the tracker for cancellation tokens
        _mediaLibrariesScanCancellationTracker.RegisterScan(scan.Id.Value, scan.UserId!.Value);

        try
        {
            // link the user's cancellation token with the scan's token
            CancellationTokenSource linkedSource = CancellationTokenSource
               .CreateLinkedTokenSource(cancellationToken, _mediaLibrariesScanCancellationTracker.GetTokenForScan(scan.Id.Value, scan.UserId!.Value));

            // get a media library scanner for the provided media library type
            IMediaTypeScanner scanner = _libraryScannerFactory.CreateLibraryScanner(libraryType);
            // get the list of scan jobs for the retrieved scanner
            List<IMediaLibraryScanJob> jobs = scanner.CreateScanJobsForLibrary(scan.LibraryId, downloadMedatadaFromWeb).ToList();
            // enqueue the jobs on the channel from where they will be processed
            foreach (IMediaLibraryScanJob job in jobs)
            {
                job.ScanId = scan.Id;
                job.UserId = scan.UserId;
                await _mediaScanQueue.Writer.WriteAsync(job, linkedSource.Token).ConfigureAwait(false);
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
        _mediaLibrariesScanCancellationTracker.CancelScan(scan.Id.Value, scan.UserId!.Value);
        return Result.Success;
    }
}
