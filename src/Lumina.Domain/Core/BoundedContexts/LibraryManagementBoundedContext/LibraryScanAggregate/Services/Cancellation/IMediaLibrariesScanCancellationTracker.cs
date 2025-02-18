#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using System.Threading;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Cancellation;

/// <summary>
/// Interface for the tracker used for canceling in-memory media library scans.
/// </summary>
public interface IMediaLibrariesScanCancellationTracker
{
    /// <summary>
    /// Gets the cancellation token for a scan identified by <paramref name="mediaLibraryScanCompositeId"/>.
    /// </summary>
    /// <param name="mediaLibraryScanCompositeId">Model for tracking media library scans.</param>
    /// <returns>A cancellation token for the specified scan.</returns>
    CancellationToken GetTokenForScan(MediaLibraryScanCompositeId mediaLibraryScanCompositeId);

    /// <summary>
    /// Registers a new scan operation for tracking.
    /// </summary>
    /// <param name="mediaLibraryScanCompositeId">Model for tracking media library scans.</param>
    void RegisterScan(MediaLibraryScanCompositeId mediaLibraryScanCompositeId);
    
    /// <summary>
    /// Removes a scan operation from tracking.
    /// </summary>
    /// <param name="mediaLibraryScanCompositeId">Model for tracking media library scans.</param>
    void RemoveScan(MediaLibraryScanCompositeId mediaLibraryScanCompositeId);

    /// <summary>
    /// Cancels a scan identified by <paramref name="mediaLibraryScanCompositeId"/>.
    /// </summary>
    /// <param name="mediaLibraryScanCompositeId">Model for tracking media library scans.</param>
    void CancelScan(MediaLibraryScanCompositeId mediaLibraryScanCompositeId);
}
