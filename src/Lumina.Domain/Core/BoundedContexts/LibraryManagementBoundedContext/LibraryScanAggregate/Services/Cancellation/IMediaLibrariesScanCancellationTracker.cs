#region ========================================================================= USING =====================================================================================
using System;
using System.Threading;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Cancellation;

/// <summary>
/// Interface for the tracker used for canceling in-memory media library scans.
/// </summary>
public interface IMediaLibrariesScanCancellationTracker
{
    /// <summary>
    /// Gets the cancellation token for a scan identified by <paramref name="scanId"/> and <paramref name="userId"/>.
    /// </summary>
    /// <param name="scanId">The unique identifier of the scan to get the cancellation token for.</param>
    /// <param name="userId">The unique identifier of the user who initiated the scan for which to get the cancellation token.</param>
    /// <returns>A cancellation token for the specified scan.</returns>
    CancellationToken GetTokenForScan(Guid scanId, Guid userId);

    /// <summary>
    /// Registers a new scan operation for tracking.
    /// </summary>
    /// <param name="scanId">The unique identifier of the scan operation to track.</param>
    /// <param name="userId">The unique identifier of the user who initiated the scan to track.</param>
    void RegisterScan(Guid scanId, Guid userId);

    /// <summary>
    /// Cancels a scan identified by <paramref name="scanId"/> and <paramref name="userId"/>.
    /// </summary>
    /// <param name="scanId">The unique identifier of the scan operation to cancel.</param>
    /// <param name="userId">The unique identifier of the user requesting the cancellation.</param>
    void CancelScan(Guid scanId, Guid userId);

    /// <summary>
    /// Cancels all active scan operations for a user identified by <paramref name="userId"/>.
    /// </summary>
    /// <param name="userId">The unique identifier of the user whose scans should be canceled.</param>
    void CancelUserScans(Guid userId);
}
