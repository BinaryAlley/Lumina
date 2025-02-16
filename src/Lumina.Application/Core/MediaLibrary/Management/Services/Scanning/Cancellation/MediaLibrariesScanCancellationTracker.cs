#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Models.Scanning;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Cancellation;
using System;
using System.Collections.Concurrent;
using System.Threading;
#endregion

namespace Lumina.Application.Core.MediaLibrary.Management.Services.Scanning.Cancellation;

/// <summary>
/// Tracker used for canceling in-memory media library scans.
/// </summary>
internal class MediaLibrariesScanCancellationTracker : IMediaLibrariesScanCancellationTracker, IDisposable
{
    private readonly ConcurrentDictionary<MediaScanTrackerModel, CancellationTokenSource> _runningScans = new();
    private bool _disposed;

    /// <summary>
    /// Registers a new scan operation for tracking.
    /// </summary>
    /// <param name="scanId">The unique identifier of the scan operation to track.</param>
    /// <param name="userId">The unique identifier of the user who initiated the scan to track.</param>
    public void RegisterScan(Guid scanId, Guid userId)
    {
        _runningScans.TryAdd(new MediaScanTrackerModel(scanId, userId), new CancellationTokenSource());
    }

    /// <summary>
    /// Gets the cancellation token for a scan identified by <paramref name="scanId"/> and <paramref name="userId"/>.
    /// </summary>
    /// <param name="scanId">The unique identifier of the scan to get the cancellation token for<./param>
    /// <param name="userId">The unique identifier of the user who initiated the scan for which to get the cancellation token.</param>
    /// <returns>A cancellation token for the specified scan.</returns>
    public CancellationToken GetTokenForScan(Guid scanId, Guid userId)
    {
        return _runningScans.TryGetValue(new MediaScanTrackerModel(scanId, userId), out CancellationTokenSource? scan)
            ? scan.Token
            : CancellationToken.None;
    }

    /// <summary>
    /// Cancels a scan identified by <paramref name="scanId"/> and <paramref name="userId"/>.
    /// </summary>
    /// <param name="scanId">The unique identifier of the scan operation to cancel.</param>
    /// <param name="userId">The unique identifier of the user requesting the cancellation.</param>
    public void CancelScan(Guid scanId, Guid userId)
    {
        foreach (MediaScanTrackerModel key in _runningScans.Keys)
        {
            if (key.ScanId == scanId && key.UserId == userId)
            {
                if (_runningScans.TryGetValue(key, out CancellationTokenSource? scan))
                {
                    scan.Cancel();
                    // remove and dispose after cancellation
                    if (_runningScans.TryRemove(key, out CancellationTokenSource? removedScan))
                        removedScan?.Dispose();
                }
            }
        }
    }

    /// <summary>
    /// Cancels all active scan operations for a user identified by <paramref name="userId"/>.
    /// </summary>
    /// <param name="userId">The unique identifier of the user whose scans should be canceled.</param>
    public void CancelUserScans(Guid userId)
    {
        foreach (MediaScanTrackerModel key in _runningScans.Keys)
        {
            if (key.UserId == userId)
            {
                if (_runningScans.TryGetValue(key, out CancellationTokenSource? scan))
                {
                    scan.Cancel();
                    // remove and dispose after cancellation
                    if (_runningScans.TryRemove(key, out CancellationTokenSource? removedScan))
                        removedScan?.Dispose();
                }
            }
        }
    }

    /// <summary>
    /// Disposes the tracker and cancels all media library scans.
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            foreach (CancellationTokenSource scan in _runningScans.Values)
            {
                scan.Cancel();
                scan.Dispose();
            }
            _runningScans.Clear();
            _disposed = true;
        }
    }
}
