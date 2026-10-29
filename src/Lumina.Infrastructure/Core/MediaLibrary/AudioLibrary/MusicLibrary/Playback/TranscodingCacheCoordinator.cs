#region ========================================================================= USING =====================================================================================
using Lumina.Infrastructure.Common.Caching;
using Lumina.Infrastructure.Common.Models.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Playback;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
#endregion

namespace Lumina.Infrastructure.Core.MediaLibrary.AudioLibrary.MusicLibrary.Playback;

/// <summary>
/// Coordinates the process wide size accounting, eviction and reader protection of the normalized audio cache.
/// </summary>
/// <remarks>
/// Every cached entry and every running normalization job of a cache root is accounted for under one lock, so the cache size limit is a global limit
/// instead of a per job limit. An entry is protected from eviction while a reader holds a lease on it, and for a grace period after it was produced or
/// last served, so an entry that a request is about to read is never deleted by another job. Replacing and registering a produced entry is atomic under
/// the same lock, so an eviction can never delete a freshly published entry. Directories are never deleted under the lock: an entry that is evicted or
/// replaced is renamed to a unique tombstone under the lock and deleted by a background sweeper, so serving a segment or opening a manifest never waits
/// for a recursive delete. The size limit is enforced against live entries: a tombstone occupies disk until the sweeper removes it. The coordinator is
/// shared by every service instance of the process that uses the same cache root.
/// </remarks>
internal sealed class TranscodingCacheCoordinator
{
    private static readonly ConcurrentDictionary<string, TranscodingCacheCoordinator> s_coordinators = new(StringComparer.Ordinal);
    private static readonly Lock s_registrySync = new();

    private readonly Lock _sync = new();
    private readonly string _cacheRoot;
    private readonly Dictionary<string, TranscodingCacheEntryDto> _entries = new(StringComparer.Ordinal);
    private readonly Dictionary<string, long> _reservations = new(StringComparer.Ordinal);
    private readonly Dictionary<string, int> _readerLeaseCounts = new(StringComparer.Ordinal);
    // Read and written by request threads without _sync, so the index cache is a concurrent dictionary while the accounting dictionaries are guarded by _sync.
    private readonly ConcurrentDictionary<string, NormalizedTrackIndexDto> _indexCache = new(StringComparer.Ordinal);
    // Entries that were over capacity but still leased. The last lease release hands them to the sweeper.
    private readonly HashSet<string> _pendingEvictions = new(StringComparer.Ordinal);
    // Tombstones are consumed by one background reader, so a recursive delete never runs under _sync.
    private readonly Channel<string> _deletionQueue = Channel.CreateUnbounded<string>(new UnboundedChannelOptions { SingleReader = true });
    // The value is unused. Lazy is the once-only gate: the first caller runs the cache walk outside _sync and every other caller waits for it.
    private readonly Lazy<bool> _initialization;
    private int _referenceCount;

    /// <summary>
    /// Initializes a new instance of the <see cref="TranscodingCacheCoordinator"/> class.
    /// </summary>
    /// <param name="cacheRoot">The absolute path of the cache root the coordinator accounts for.</param>
    private TranscodingCacheCoordinator(string cacheRoot)
    {
        _cacheRoot = cacheRoot;
        _initialization = new Lazy<bool>(InitializeCore, LazyThreadSafetyMode.ExecutionAndPublication);
        _ = Task.Run(ProcessDeletionQueueAsync);
    }

    /// <summary>
    /// Acquires the coordinator of a cache root, which is shared by every service instance of the process that uses the same root.
    /// </summary>
    /// <param name="cacheRoot">The absolute path of the cache root.</param>
    /// <returns>The coordinator of the cache root. The caller must call <see cref="Release"/> when it no longer uses it.</returns>
    public static TranscodingCacheCoordinator AcquireForRoot(string cacheRoot)
    {
        string normalizedRoot = Path.GetFullPath(cacheRoot);
        lock (s_registrySync)
        {
            // The registry is process wide because two service instances that use the same cache root must share the same size and lease accounting.
            // The reference count keeps the coordinator alive until the last service instance is disposed.
            if (!s_coordinators.TryGetValue(normalizedRoot, out TranscodingCacheCoordinator? coordinator))
            {
                coordinator = new TranscodingCacheCoordinator(normalizedRoot);
                s_coordinators[normalizedRoot] = coordinator;
            }
            coordinator._referenceCount++;
            return coordinator;
        }
    }

    /// <summary>
    /// Releases the caller's use of the coordinator, removing it from the process registry when the last caller releases it.
    /// </summary>
    public void Release()
    {
        bool isUnused;
        lock (s_registrySync)
        {
            if (_referenceCount > 0)
                _referenceCount--;
            isUnused = _referenceCount == 0;
            if (isUnused)
                s_coordinators.Remove(_cacheRoot, out _);
        }

        if (!isUnused)
            return;
        lock (_sync)
        {
            _entries.Clear();
            _reservations.Clear();
            _readerLeaseCounts.Clear();
            _pendingEvictions.Clear();
            _indexCache.Clear();
        }
        // The sweeper drains the tombstones it already owns and then stops. A deletion that is still in flight is finished, and a tombstone left by a
        // deletion that cannot complete is removed by the startup cleanup.
        _deletionQueue.Writer.TryComplete();
    }

    /// <summary>
    /// Checks whether a value is a well formed opaque cache identifier.
    /// </summary>
    /// <param name="cacheId">The value to check.</param>
    /// <returns><see langword="true"/> when the value is a well formed cache identifier, otherwise <see langword="false"/>.</returns>
    public static bool IsValidCacheId(string cacheId)
    {
        return TranscodingCachePaths.IsValidCacheId(cacheId);
    }

    /// <summary>
    /// Reserves capacity for the output of a normalization job, after evicting the entries that may be evicted to make room for it.
    /// </summary>
    /// <param name="requestedBytes">The estimated size of the output of the job, in bytes.</param>
    /// <param name="maxCacheBytes">The maximum total size of the cache, in bytes.</param>
    /// <param name="gracePeriod">The period during which a recently produced or served entry is protected from eviction.</param>
    /// <param name="reservation">The reservation of the job, when capacity could be reserved.</param>
    /// <returns><see langword="true"/> when capacity could be reserved, otherwise <see langword="false"/>.</returns>
    public bool TryReserve(long requestedBytes, long maxCacheBytes, TimeSpan gracePeriod, out TranscodingCacheReservation? reservation)
    {
        reservation = null;
        if (requestedBytes <= 0 || maxCacheBytes <= 0)
            return false;
        EnsureInitialized();
        lock (_sync)
        {
            long currentSize = GetTotalSize();
            if (requestedBytes > maxCacheBytes || !TryMakeRoom(requestedBytes, currentSize, excludedCacheId: null, maxCacheBytes, gracePeriod))
                return false;

            string reservationId = Guid.NewGuid().ToString("N");
            _reservations[reservationId] = requestedBytes;
            reservation = new TranscodingCacheReservation(this, reservationId);
            return true;
        }
    }

    /// <summary>
    /// Grows the reservation of a running job to the number of bytes it has actually written, after evicting entries when it has to grow.
    /// </summary>
    /// <param name="reservationId">The identifier of the reservation.</param>
    /// <param name="actualBytes">The number of bytes the job has actually written, in bytes.</param>
    /// <param name="maxCacheBytes">The maximum total size of the cache, in bytes.</param>
    /// <param name="gracePeriod">The period during which a recently produced or served entry is protected from eviction.</param>
    /// <returns><see langword="true"/> when the reservation covers the actual size, otherwise <see langword="false"/>.</returns>
    public bool TryGrow(string reservationId, long actualBytes, long maxCacheBytes, TimeSpan gracePeriod)
    {
        if (actualBytes < 0)
            return false;
        EnsureInitialized();
        lock (_sync)
        {
            if (!_reservations.TryGetValue(reservationId, out long reservedBytes))
                return false;
            // A reservation is never shrunk while the job runs: a size of zero read before the first byte is written, or a size that fluctuates
            // while a file is being appended, must not hand the reserved capacity to another job that the running job then cannot reclaim.
            if (actualBytes <= reservedBytes)
                return true;
            if (actualBytes > maxCacheBytes)
                return false;

            long currentSize = GetTotalSize();
            long requiredAdditionalBytes = actualBytes - reservedBytes;
            if (!TryMakeRoom(requiredAdditionalBytes, currentSize, excludedCacheId: null, maxCacheBytes, gracePeriod))
                return false;

            _reservations[reservationId] = actualBytes;
            return true;
        }
    }

    /// <summary>
    /// Publishes the cache entry a finished job produced, replacing any previous entry for the same identifier, and enforces the cache size limit.
    /// </summary>
    /// <param name="reservationId">The identifier of the reservation.</param>
    /// <param name="cacheId">The identifier of the produced cache entry.</param>
    /// <param name="temporaryPath">The temporary directory that holds the complete produced entry.</param>
    /// <param name="size">The total size of the produced cache entry, in bytes.</param>
    /// <param name="index">The validated index of the produced cache entry.</param>
    /// <param name="maxCacheBytes">The maximum total size of the cache, in bytes.</param>
    /// <param name="gracePeriod">The period during which a recently produced or served entry is protected from eviction.</param>
    /// <param name="isKeptOverCapacity">
    /// <see langword="true"/> when the produced entry still exceeds the cache limit after every unprotected entry was detached, so it is kept readable and
    /// becomes eligible for eviction later. The caller should report the production as successful and log the over-budget state.
    /// </param>
    /// <param name="isEntryInUse">
    /// <see langword="true"/> when a reader still holds the previous entry of the same cache identifier, so the replacement was not attempted and the
    /// caller should report a retryable failure instead of a production failure.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when the produced entry is readable and may be served, otherwise <see langword="false"/> when it was not published.
    /// </returns>
    public bool TryPublish(string reservationId, string cacheId, string temporaryPath, long size, NormalizedTrackIndexDto index, long maxCacheBytes, TimeSpan gracePeriod, out bool isKeptOverCapacity, out bool isEntryInUse)
    {
        isKeptOverCapacity = false;
        isEntryInUse = false;
        EnsureInitialized();
        lock (_sync)
        {
            // The reservation is consumed whether or not the entry is published, because the caller never releases it after this call.
            _reservations.Remove(reservationId);

            // The previous directory cannot be replaced while a reader holds it on every platform, so the production is reported as retryable and the
            // previous entry stays intact. The next request retries the production after the reader is gone.
            if (IsLeased(cacheId))
            {
                isEntryInUse = true;
                return false;
            }

            string cachePath = Path.Combine(_cacheRoot, cacheId);
            if (_entries.TryGetValue(cacheId, out TranscodingCacheEntryDto? existingEntry))
            {
                if (!TryDetachEntryForDeletion(cacheId, existingEntry))
                    return false;
            }
            else if (!TryMoveToDeletion(cachePath))
                return false;

            _indexCache.TryRemove(cacheId, out _);
            _pendingEvictions.Remove(cacheId);
            try
            {
                Directory.Move(temporaryPath, cachePath);
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
            {
                return false;
            }
            // Defense in depth against a directory that an external actor removed between the move and the registration.
            if (!Directory.Exists(cachePath))
                return false;

            _entries[cacheId] = new TranscodingCacheEntryDto(cachePath, size, DateTime.UtcNow, DateTime.UtcNow);
            _indexCache[cacheId] = index;

            long currentSize = GetTotalSize();
            if (currentSize <= maxCacheBytes)
                return true;
            if (TryMakeRoom(0, currentSize, cacheId, maxCacheBytes, gracePeriod))
                return true;

            // The entry does not fit even after every other unprotected entry was detached. It is kept readable and becomes eligible for eviction once
            // its grace period ends, so a production that already consumed a full job is not discarded because the estimate was exceeded.
            isKeptOverCapacity = true;
            return true;
        }
    }

    /// <summary>
    /// Releases the reservation of a job that did not produce a cache entry.
    /// </summary>
    /// <param name="reservationId">The identifier of the reservation.</param>
    public void ReleaseReservation(string reservationId)
    {
        lock (_sync)
            _reservations.Remove(reservationId);
    }

    /// <summary>
    /// Records that a cache entry was served, so that the least recently used eviction and the eviction grace period keep it for longer.
    /// </summary>
    /// <param name="cacheId">The identifier of the cache entry.</param>
    /// <param name="throttleInterval">
    /// The minimum interval between two writes of the last use time. A value smaller than the eviction grace period keeps an actively used entry protected.
    /// </param>
    public void Touch(string cacheId, TimeSpan throttleInterval)
    {
        EnsureInitialized();
        lock (_sync)
        {
            if (!_entries.TryGetValue(cacheId, out TranscodingCacheEntryDto? entry))
                return;

            DateTime nowUtc = DateTime.UtcNow;
            // A stream opens a segment every few seconds, so writing the last use time on every request would needlessly serialize the coordinator.
            if (nowUtc - entry.LastUsedUtc < throttleInterval)
                return;
            _entries[cacheId] = entry with { LastUsedUtc = nowUtc };
        }
    }

    /// <summary>
    /// Acquires a lease that prevents a cache entry from being evicted while a reader holds it.
    /// </summary>
    /// <param name="cacheId">The identifier of the cache entry.</param>
    /// <returns>The lease, which must be disposed when the reader is done with the entry.</returns>
    public TranscodingCacheReaderLease AcquireReaderLease(string cacheId)
    {
        EnsureInitialized();
        lock (_sync)
            _readerLeaseCounts[cacheId] = _readerLeaseCounts.TryGetValue(cacheId, out int leaseCount) ? leaseCount + 1 : 1;
        return new TranscodingCacheReaderLease(this, cacheId);
    }

    /// <summary>
    /// Gets the validated index of a cache entry, when it was already read and validated by this process.
    /// </summary>
    /// <param name="cacheId">The identifier of the cache entry.</param>
    /// <param name="index">The validated index of the cache entry, when it is cached.</param>
    /// <returns><see langword="true"/> when the validated index is cached, otherwise <see langword="false"/>.</returns>
    public bool TryGetIndex(string cacheId, out NormalizedTrackIndexDto? index)
    {
        return _indexCache.TryGetValue(cacheId, out index);
    }

    /// <summary>
    /// Caches the validated index of a cache entry, so serving its segments does not re-read and re-validate the whole index on every request.
    /// </summary>
    /// <param name="cacheId">The identifier of the cache entry.</param>
    /// <param name="index">The validated index of the cache entry.</param>
    public void SetIndex(string cacheId, NormalizedTrackIndexDto index)
    {
        _indexCache[cacheId] = index;
    }

    /// <summary>
    /// Releases a lease that a reader held on a cache entry.
    /// </summary>
    /// <param name="cacheId">The identifier of the cache entry.</param>
    internal void ReleaseReaderLease(string cacheId)
    {
        lock (_sync)
        {
            if (!_readerLeaseCounts.TryGetValue(cacheId, out int leaseCount))
                return;
            if (leaseCount > 1)
            {
                _readerLeaseCounts[cacheId] = leaseCount - 1;
                return;
            }

            _readerLeaseCounts.Remove(cacheId);
            // An entry that could not be evicted while a reader was using it is detached only after the last reader is gone. The actual deletion is
            // performed by the sweeper, outside this lock, so releasing a segment stream never waits for a recursive delete.
            if (_pendingEvictions.Contains(cacheId) && _entries.TryGetValue(cacheId, out TranscodingCacheEntryDto? entry))
            {
                _pendingEvictions.Remove(cacheId);
                if (!TryDetachEntryForDeletion(cacheId, entry))
                    _pendingEvictions.Add(cacheId);
            }
        }
    }

    /// <summary>
    /// Gets the total size of a directory, in bytes, without letting a file that disappears mid-walk discard the rest of the measurement.
    /// </summary>
    /// <param name="directoryPath">The directory whose size is computed.</param>
    /// <returns>The total size of the directory, in bytes.</returns>
    internal static long GetDirectorySize(string directoryPath)
    {
        long size = 0;
        Stack<string> pendingDirectories = new();
        pendingDirectories.Push(directoryPath);
        while (pendingDirectories.Count > 0)
        {
            string currentDirectory = pendingDirectories.Pop();
            try
            {
                using (IEnumerator<string> fileEnumerator = Directory.EnumerateFiles(currentDirectory).GetEnumerator())
                {
                    while (true)
                    {
                        bool hasNextFile;
                        try
                        {
                            hasNextFile = fileEnumerator.MoveNext();
                        }
                        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
                        {
                            break;
                        }
                        if (!hasNextFile)
                            break;

                        try
                        {
                            size += new FileInfo(fileEnumerator.Current).Length;
                        }
                        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
                        {
                            // A file that disappears between enumeration and its size read is not counted; the walk continues with the next one.
                        }
                    }
                }
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
            {
                // A directory that cannot be opened is skipped; the rest of the cache is still measured.
            }

            try
            {
                using (IEnumerator<string> directoryEnumerator = Directory.EnumerateDirectories(currentDirectory).GetEnumerator())
                {
                    while (true)
                    {
                        bool hasNextDirectory;
                        try
                        {
                            hasNextDirectory = directoryEnumerator.MoveNext();
                        }
                        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
                        {
                            break;
                        }
                        if (!hasNextDirectory)
                            break;
                        pendingDirectories.Push(directoryEnumerator.Current);
                    }
                }
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
            {
                // A directory that cannot be enumerated is skipped; the rest of the cache is still measured.
            }
        }
        return size;
    }

    /// <summary>
    /// Runs the once-only initialization of the entries that already exist in the cache root, outside the coordinator lock.
    /// </summary>
    private void EnsureInitialized()
    {
        _ = _initialization.Value;
    }

    /// <summary>
    /// Reads the entries that already exist in the cache root, so a cache that survived a restart is still accounted for.
    /// </summary>
    /// <returns><see langword="true"/> when the accounting was populated.</returns>
    /// <remarks>
    /// The walk of the cache root happens outside the coordinator lock, because it stats every file of every entry. The result is published under the lock,
    /// so a request never blocks a manifest or a segment open behind the walk.
    /// </remarks>
    private bool InitializeCore()
    {
        Dictionary<string, TranscodingCacheEntryDto> entries = new(StringComparer.Ordinal);
        List<string> tombstones = [];
        if (Directory.Exists(_cacheRoot))
        {
            try
            {
                foreach (string directoryPath in Directory.EnumerateDirectories(_cacheRoot))
                {
                    string cacheId = Path.GetFileName(directoryPath);
                    if (cacheId.StartsWith(TranscodingCachePaths.DELETION_DIRECTORY_PREFIX, StringComparison.Ordinal))
                    {
                        // A tombstone left by a previous coordinator of the same process is handed to the sweeper instead of being accounted for.
                        tombstones.Add(directoryPath);
                        continue;
                    }
                    if (!IsValidCacheId(cacheId))
                        continue;
                    entries[cacheId] = new TranscodingCacheEntryDto(directoryPath, GetDirectorySize(directoryPath), Directory.GetLastWriteTimeUtc(directoryPath), DateTime.MinValue);
                }
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
            {
                // A cache that cannot be enumerated is treated as empty for this run instead of being retried on every request.
            }
        }

        lock (_sync)
            foreach (KeyValuePair<string, TranscodingCacheEntryDto> entry in entries)
                _entries[entry.Key] = entry.Value;
        foreach (string tombstonePath in tombstones)
            _deletionQueue.Writer.TryWrite(tombstonePath);
        return true;
    }

    /// <summary>
    /// Gets the combined size of the cached entries and the reservations of the running jobs, in bytes.
    /// </summary>
    /// <returns>The combined size of the cached entries and the reservations, in bytes.</returns>
    private long GetTotalSize()
    {
        long entriesSize = _entries.Values.Sum(entry => entry.Size);
        long reservationsSize = _reservations.Values.Sum();
        return entriesSize + reservationsSize;
    }

    /// <summary>
    /// Detaches the least recently used unprotected entries until the required number of bytes fits under the cache size limit.
    /// </summary>
    /// <param name="requiredBytes">The number of additional bytes that must fit.</param>
    /// <param name="currentSize">The current combined size of the entries and reservations, in bytes.</param>
    /// <param name="excludedCacheId">The cache entry that must never be evicted by this pass, or <see langword="null"/> when none is excluded.</param>
    /// <param name="maxCacheBytes">The maximum total size of the cache, in bytes.</param>
    /// <param name="gracePeriod">The period during which a recently produced or served entry is protected from eviction.</param>
    /// <returns><see langword="true"/> when the required number of bytes fits, otherwise <see langword="false"/>.</returns>
    private bool TryMakeRoom(long requiredBytes, long currentSize, string? excludedCacheId, long maxCacheBytes, TimeSpan gracePeriod)
    {
        if (currentSize + requiredBytes <= maxCacheBytes)
            return true;

        DateTime nowUtc = DateTime.UtcNow;
        foreach (TranscodingCacheEntryDto entry in _entries.Values.OrderBy(entry => entry.LastUsedUtc).ToList())
        {
            string cacheId = Path.GetFileName(entry.Path);
            if (string.Equals(cacheId, excludedCacheId, StringComparison.Ordinal))
                continue;

            DateTime lastActivityUtc = entry.LastUsedUtc > entry.ProducedAtUtc ? entry.LastUsedUtc : entry.ProducedAtUtc;
            if (nowUtc - lastActivityUtc < gracePeriod)
                continue;
            if (IsLeased(cacheId))
            {
                // A reader holds the entry, so it is detached when the last lease is released instead of failing the deletion under the lock.
                _pendingEvictions.Add(cacheId);
                continue;
            }
            if (!TryDetachEntryForDeletion(cacheId, entry))
                continue;
            currentSize -= entry.Size;
            if (currentSize + requiredBytes <= maxCacheBytes)
                return true;
        }
        return currentSize + requiredBytes <= maxCacheBytes;
    }

    /// <summary>
    /// Checks whether a reader currently holds a cache entry.
    /// </summary>
    /// <param name="cacheId">The identifier of the cache entry.</param>
    /// <returns><see langword="true"/> when at least one reader holds the entry, otherwise <see langword="false"/>.</returns>
    private bool IsLeased(string cacheId)
    {
        return _readerLeaseCounts.TryGetValue(cacheId, out int leaseCount) && leaseCount > 0;
    }

    /// <summary>
    /// Removes an entry from the cache accounting and schedules its directory for deletion.
    /// </summary>
    /// <param name="cacheId">The identifier of the cache entry.</param>
    /// <param name="entry">The entry to detach.</param>
    /// <returns><see langword="true"/> when the entry was detached, otherwise <see langword="false"/> when its directory could not be renamed.</returns>
    private bool TryDetachEntryForDeletion(string cacheId, TranscodingCacheEntryDto entry)
    {
        if (!TryMoveToDeletion(entry.Path))
            return false;
        _entries.Remove(cacheId);
        _pendingEvictions.Remove(cacheId);
        _indexCache.TryRemove(cacheId, out _);
        return true;
    }

    /// <summary>
    /// Moves a directory out of the live cache and schedules it for deletion.
    /// </summary>
    /// <param name="directoryPath">The directory to detach.</param>
    /// <returns><see langword="true"/> when the directory was detached or no longer exists, otherwise <see langword="false"/>.</returns>
    private bool TryMoveToDeletion(string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
            return true;
        string deletionPath = Path.Combine(_cacheRoot, TranscodingCachePaths.DELETION_DIRECTORY_PREFIX + Guid.NewGuid().ToString("N"));
        try
        {
            // A rename is a metadata operation, so it stays fast under the lock even for an entry with thousands of segments.
            Directory.Move(directoryPath, deletionPath);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            return false;
        }
        // A tombstone that cannot be enqueued because the coordinator was released is removed by the startup cleanup.
        _deletionQueue.Writer.TryWrite(deletionPath);
        return true;
    }

    /// <summary>
    /// Deletes the directories that were detached from the cache.
    /// </summary>
    private async Task ProcessDeletionQueueAsync()
    {
        await foreach (string directoryPath in _deletionQueue.Reader.ReadAllAsync().ConfigureAwait(false))
            TranscodingCachePaths.TryDeleteDirectory(directoryPath);
    }
}
