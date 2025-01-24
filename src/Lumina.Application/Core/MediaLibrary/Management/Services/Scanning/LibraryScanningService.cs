#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.MediaLibrary.Management.Services.Scanning.Jobs.Common;
using Lumina.Application.Core.MediaLibrary.Management.Services.Scanning.Scanners.Common;
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
internal class LibraryScanningService : ILibraryScanningService
{
    private readonly IMediaScanQueue _mediaScanQueue;
    private readonly ILibraryScannerFactory _libraryScannerFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="LibraryScanningService"/> class.
    /// </summary>
    /// <param name="mediaScanQueue">Injected queue used for processing media libraries scan jobs.</param>
    /// <param name="libraryScannerFactory">Injected factory for creating media library scanners.</param>
    public LibraryScanningService(IMediaScanQueue mediaScanQueue, ILibraryScannerFactory libraryScannerFactory)
    {
        _mediaScanQueue = mediaScanQueue;
        _libraryScannerFactory = libraryScannerFactory;
    }

    /// <summary>
    /// Starts the scan of <paramref name="library"/>.
    /// </summary>
    /// <param name="library">The media library to scan.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public async Task StartScanAsync(Library library, CancellationToken cancellationToken)
    {
        try
        {
            // get a media library scanne for the provided media library type
            IMediaTypeScanner scanner = _libraryScannerFactory.CreateLibraryScanner(library.LibraryType);
            // get the list of scan jobs for the retrieved scanner
            IEnumerable<MediaScanJob> jobs = scanner.CreateScanJobsForLibrary(library);
            // enqueue the jobs on the channel from where they will be processed
            foreach (MediaScanJob job in jobs)
                await _mediaScanQueue.Writer.WriteAsync(job, cancellationToken).ConfigureAwait(false);
        }
        catch (NotImplementedException) { }
    }
}
