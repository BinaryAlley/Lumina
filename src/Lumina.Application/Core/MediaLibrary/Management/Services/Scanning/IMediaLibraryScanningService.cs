#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate;
using System;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Core.MediaLibrary.Management.Services.Scanning;

/// <summary>
/// Interface for the service used to scan a media library.
/// </summary>
public interface IMediaLibraryScanningService
{
    /// <summary>
    /// Starts the scan of <paramref name="library"/>.
    /// </summary>
    /// <param name="library">The media library to scan.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The Id of the media library scan.</returns>
    Task<Guid> StartScanAsync(Library library, CancellationToken cancellationToken);

    /// <summary>
    /// Cancels the scan identified by <paramref name="scanId"/>.
    /// </summary>
    /// <param name="scanId">The Id of the scan to cancel.</param>
    void CancelScan(Guid scanId);
}
