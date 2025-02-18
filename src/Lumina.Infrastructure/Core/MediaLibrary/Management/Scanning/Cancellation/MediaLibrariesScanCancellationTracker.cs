#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Cancellation;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using System;
using System.Collections.Concurrent;
using System.Threading;
#endregion

namespace Lumina.Infrastructure.Core.MediaLibrary.Management.Scanning.Cancellation;

/// <summary>
/// Tracker used for canceling in-memory media library scans.
/// </summary>
internal class MediaLibrariesScanCancellationTracker : IMediaLibrariesScanCancellationTracker, IDisposable
{
    private readonly ConcurrentDictionary<MediaLibraryScanCompositeId, CancellationTokenSource> _runningScans = new();
    private bool _disposed;

    /// <summary>
    /// Registers a new scan operation for tracking.
    /// </summary>
    /// <param name="mediaLibraryScanCompositeId">Model for tracking media library scans.</param>
    public void RegisterScan(MediaLibraryScanCompositeId mediaLibraryScanCompositeId)
    {
        _runningScans.TryAdd(mediaLibraryScanCompositeId, new CancellationTokenSource());
    }

    /// <summary>
    /// Removes a scan operation from tracking.
    /// </summary>
    /// <param name="mediaLibraryScanCompositeId">Model for tracking media library scans.</param>
    public void RemoveScan(MediaLibraryScanCompositeId mediaLibraryScanCompositeId)
    {
        if (_runningScans.TryGetValue(mediaLibraryScanCompositeId, out CancellationTokenSource? scan))
        {
            scan.Cancel();
            // remove and dispose after cancellation
            if (_runningScans.TryRemove(mediaLibraryScanCompositeId, out CancellationTokenSource? removedScan))
                removedScan?.Dispose();
        }
    }

    /// <summary>
    /// Gets the cancellation token for a scan identified by <paramref name="mediaLibraryScanCompositeId"/>.
    /// </summary>
    /// <param name="mediaLibraryScanCompositeId">Model for tracking media library scans.</param>
    /// <returns>A cancellation token for the specified scan.</returns>
    public CancellationToken GetTokenForScan(MediaLibraryScanCompositeId mediaLibraryScanCompositeId)
    {
        return _runningScans.TryGetValue(mediaLibraryScanCompositeId, out CancellationTokenSource? scan)
            ? scan.Token
            : CancellationToken.None;
    }

    /// <summary>
    /// Cancels a scan identified by <paramref name="mediaLibraryScanCompositeId"/>.
    /// </summary>
    /// <param name="mediaLibraryScanCompositeId">Model for tracking media library scans.</param>
    public void CancelScan(MediaLibraryScanCompositeId mediaLibraryScanCompositeId)
    {
        if (_runningScans.TryGetValue(mediaLibraryScanCompositeId, out CancellationTokenSource? scan))
        {
            scan.Cancel();
            // remove and dispose after cancellation
            if (_runningScans.TryRemove(mediaLibraryScanCompositeId, out CancellationTokenSource? removedScan))
                removedScan?.Dispose();
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
