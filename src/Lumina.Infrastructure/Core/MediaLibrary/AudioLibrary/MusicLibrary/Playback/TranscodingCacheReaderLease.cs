#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Infrastructure.Core.MediaLibrary.AudioLibrary.MusicLibrary.Playback;

/// <summary>
/// Represents a reader's lease on a cache entry, which keeps the entry from being evicted until the lease is released.
/// </summary>
[DebuggerDisplay("CacheId: {_cacheId}")]
internal sealed class TranscodingCacheReaderLease : IDisposable
{
    private readonly TranscodingCacheCoordinator _coordinator;
    private readonly string _cacheId;
    private bool _isDisposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="TranscodingCacheReaderLease"/> class.
    /// </summary>
    /// <param name="coordinator">The coordinator that accounts for the lease.</param>
    /// <param name="cacheId">The identifier of the cache entry the lease holds.</param>
    internal TranscodingCacheReaderLease(TranscodingCacheCoordinator coordinator, string cacheId)
    {
        _coordinator = coordinator;
        _cacheId = cacheId;
    }

    /// <summary>
    /// Releases the lease, so the cache entry may be evicted again.
    /// </summary>
    public void Dispose()
    {
        if (_isDisposed)
            return;
        _isDisposed = true;
        _coordinator.ReleaseReaderLease(_cacheId);
    }
}
