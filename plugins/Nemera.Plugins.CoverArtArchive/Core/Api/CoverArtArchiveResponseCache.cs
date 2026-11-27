#region ========================================================================= USING =====================================================================================
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
#endregion

namespace Lumina.Plugins.CoverArtArchive.Core.Api;

/// <summary>
/// In-memory cache of the responses returned by the Cover Art Archive API, keyed by the requested URL.
/// It removes the repeated lookups of the same entity during a run, which is significant because the releases of an edition heavy library share their
/// release groups, and it expires its entries after a bounded time.
/// </summary>
internal sealed class CoverArtArchiveResponseCache
{
    /// <summary>
    /// The maximum number of responses the cache keeps.
    /// </summary>
    internal const int MAXIMUM_ENTRY_COUNT = 20_000;

    /// <summary>
    /// The maximum total number of characters of the cached response bodies.
    /// </summary>
    internal const long MAXIMUM_CHARACTER_COUNT = 100_000_000;

    /// <summary>
    /// The percentage of a budget that the cache is trimmed down to when it exceeds that budget. Trimming to a lower water mark keeps the
    /// eviction from running on every write once a budget is reached.
    /// </summary>
    private const int TRIM_TARGET_PERCENT = 90;

    private static readonly TimeSpan s_timeToLive = TimeSpan.FromMinutes(15);

    private readonly int _maximumEntryCount;
    private readonly long _maximumCharacterCount;
    private readonly TimeSpan _timeToLive;
    private readonly ConcurrentDictionary<string, CacheEntry> _entries = new(StringComparer.Ordinal);

    // The dictionary is thread safe on its own, but the character budget is a read modify write over it, so the size bookkeeping and the eviction
    // are guarded by a lock, while reads stay lock free.
    private readonly Lock _sizeGate = new();
    private long _cachedCharacterCount;

    // Monotonically increasing insertion stamp, used to evict the oldest entries first.
    private long _sequence;

    /// <summary>
    /// Initializes a new instance of the <see cref="CoverArtArchiveResponseCache"/> class with the default budgets and time to live.
    /// </summary>
    public CoverArtArchiveResponseCache()
    {
        _maximumEntryCount = MAXIMUM_ENTRY_COUNT;
        _maximumCharacterCount = MAXIMUM_CHARACTER_COUNT;
        _timeToLive = s_timeToLive;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CoverArtArchiveResponseCache"/> class with the provided budgets and time to live.
    /// </summary>
    /// <param name="maximumEntryCount">The maximum number of responses the cache keeps.</param>
    /// <param name="maximumCharacterCount">The maximum total number of characters of the cached response bodies.</param>
    /// <param name="timeToLive">The amount of time a cached response stays valid.</param>
    internal CoverArtArchiveResponseCache(int maximumEntryCount, long maximumCharacterCount, TimeSpan timeToLive)
    {
        _maximumEntryCount = maximumEntryCount;
        _maximumCharacterCount = maximumCharacterCount;
        _timeToLive = timeToLive;
    }

    /// <summary>
    /// Tries to get the cached response body for <paramref name="key"/>.
    /// </summary>
    /// <param name="key">The key of the cached response.</param>
    /// <param name="json">The cached response body, when found.</param>
    /// <returns><see langword="true"/> when an unexpired response was found, otherwise <see langword="false"/>.</returns>
    public bool TryGet(string key, out string? json)
    {
        // An expired entry is treated as a miss instead of being removed here, so readers never take the lock; a later write reclaims it.
        if (_entries.TryGetValue(key, out CacheEntry? entry) && entry.ExpiresOnUtc > DateTime.UtcNow)
        {
            json = entry.Json;
            return true;
        }
        json = null;
        return false;
    }

    /// <summary>
    /// Adds or replaces the cached response body for <paramref name="key"/>, then trims the cache back to its budgets.
    /// </summary>
    /// <param name="key">The key of the cached response.</param>
    /// <param name="json">The response body to cache.</param>
    public void Set(string key, string json)
    {
        // A single response larger than the whole character budget can never be admitted, so it is rejected before it disturbs the cache.
        if (json.Length > _maximumCharacterCount)
            return;

        lock (_sizeGate)
        {
            if (_entries.TryRemove(key, out CacheEntry? existing))
                _cachedCharacterCount -= existing.Json.Length;

            _entries[key] = new CacheEntry(json, DateTime.UtcNow.Add(_timeToLive), ++_sequence);
            _cachedCharacterCount += json.Length;

            if (_entries.Count > _maximumEntryCount || _cachedCharacterCount > _maximumCharacterCount)
                TrimToBudget();
        }
    }

    /// <summary>
    /// Removes the expired entries, and then the oldest unexpired entries, until the cache is back within its entry count and character budgets.
    /// </summary>
    private void TrimToBudget()
    {
        DateTime utcNow = DateTime.UtcNow;
        bool isOverEntryCount = _entries.Count > _maximumEntryCount;
        bool isOverCharacterCount = _cachedCharacterCount > _maximumCharacterCount;
        int entryTarget = Math.Max(1, _maximumEntryCount * TRIM_TARGET_PERCENT / 100);
        long characterTarget = Math.Max(1, _maximumCharacterCount * TRIM_TARGET_PERCENT / 100);

        // Entries are evicted in insertion order, so the most recently cached responses survive; an expired entry is always removed, even when
        // a budget was not exceeded, so the cache keeps reclaiming dead entries.
        foreach (KeyValuePair<string, CacheEntry> candidate in _entries.OrderBy(entry => entry.Value.Sequence))
        {
            bool isExpired = candidate.Value.ExpiresOnUtc <= utcNow;
            bool needsEntryTrim = isOverEntryCount && _entries.Count > entryTarget;
            bool needsCharacterTrim = isOverCharacterCount && _cachedCharacterCount > characterTarget;
            if (!isExpired && !needsEntryTrim && !needsCharacterTrim)
                break;

            if (_entries.TryRemove(candidate.Key, out CacheEntry? removed))
                _cachedCharacterCount -= removed.Json.Length;
        }
    }

    private sealed record CacheEntry(string Json, DateTime ExpiresOnUtc, long Sequence);
}
