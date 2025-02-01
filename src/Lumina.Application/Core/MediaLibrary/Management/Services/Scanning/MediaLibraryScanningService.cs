#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Core.MediaLibrary.Management.Services.Scanning.Jobs.Common;
using Lumina.Application.Core.MediaLibrary.Management.Services.Scanning.Queue;
using Lumina.Application.Core.MediaLibrary.Management.Services.Scanning.Scanners.Common;
using Lumina.Application.Core.MediaLibrary.Management.Services.Scanning.Tracking;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Core.MediaLibrary.Management.Services.Scanning;

/// <summary>
/// Service used to scan a media library.
/// </summary>
internal class MediaLibraryScanningService : IMediaLibraryScanningService
{
    private readonly IMediaLibrariesScanQueue _mediaScanQueue;
    private readonly IMediaLibraryScannerFactory _libraryScannerFactory;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMediaLibrariesScanTracker _librariesScanTracker;

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaLibraryScanningService"/> class.
    /// </summary>
    /// <param name="mediaScanQueue">Injected queue used for processing media libraries scan jobs.</param>
    /// <param name="libraryScannerFactory">Injected factory for creating media library scanners.</param>
    public MediaLibraryScanningService(
        IMediaLibrariesScanQueue mediaScanQueue, 
        IMediaLibraryScannerFactory libraryScannerFactory, 
        IMediaLibrariesScanTracker librariesScanTracker, 
        ICurrentUserService currentUserService)
    {
        _mediaScanQueue = mediaScanQueue;
        _libraryScannerFactory = libraryScannerFactory;
        _currentUserService = currentUserService;
        _librariesScanTracker = librariesScanTracker;
    }

    /// <summary>
    /// Starts the scan of <paramref name="library"/>.
    /// </summary>
    /// <param name="library">The media library to scan.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The Id of the media library scan.</returns>
    public async Task<Guid> StartScanAsync(Library library, CancellationToken cancellationToken)
    {
        Guid scanId = Guid.NewGuid();
        _librariesScanTracker.RegisterScan(scanId, _currentUserService.UserId!.Value);
        
        try
        {
            // link the user's cancellation token with the scan's token
             CancellationTokenSource linkedSource = CancellationTokenSource
                .CreateLinkedTokenSource(cancellationToken, _librariesScanTracker.GetTokenForScan(scanId, _currentUserService.UserId!.Value));

            // get a media library scanner for the provided media library type
            IMediaTypeScanner scanner = _libraryScannerFactory.CreateLibraryScanner(library.LibraryType);
            // get the list of scan jobs for the retrieved scanner
            IEnumerable<MediaLibraryScanJob> jobs = scanner.CreateScanJobsForLibrary(library);
            // enqueue the jobs on the channel from where they will be processed
            foreach (MediaLibraryScanJob job in jobs)
            {
                job.ScanId = scanId;
                job.UserId = _currentUserService.UserId!.Value;
                await _mediaScanQueue.Writer.WriteAsync(job, linkedSource.Token).ConfigureAwait(false);
            }
        }
        catch (NotImplementedException) { }
        return scanId;
    }

    /// <summary>
    /// Cancels the scan identified by <paramref name="scanId"/>.
    /// </summary>
    /// <param name="scanId">The Id of the scan to cancel.</param>
    public void CancelScan(Guid scanId)
    {
        _librariesScanTracker.CancelScan(scanId, _currentUserService.UserId!.Value);
    }
}
