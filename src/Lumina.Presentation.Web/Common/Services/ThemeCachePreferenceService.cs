#region ========================================================================= USING =====================================================================================
using Microsoft.Extensions.Caching.Hybrid;
using System;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Common.Services;

/// <summary>
/// Service that stores and retrieves the per-user preference for the theme cache, so the theme caching can be toggled for a user from the settings page.
/// The preference is stored in the user settings and kept in the cache so the theme cache handler can apply it on every request without querying the database.
/// </summary>
public class ThemeCachePreferenceService
{
    private const string PREFERENCE_KEY_PREFIX = "theme-cache-enabled:";
    private static readonly TimeSpan s_preferenceLifetime = TimeSpan.FromDays(30);

    private readonly HybridCache _hybridCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="ThemeCachePreferenceService"/> class.
    /// </summary>
    /// <param name="hybridCache">The hybrid caching mechanism used to store and retrieve the theme cache preference.</param>
    public ThemeCachePreferenceService(HybridCache hybridCache)
    {
        _hybridCache = hybridCache;
    }

    /// <summary>
    /// Gets whether the theme cache is enabled for the user identified by <paramref name="userId"/>.
    /// </summary>
    /// <param name="userId">The unique identifier of the user whose preference is read.</param>
    /// <param name="defaultValue">The value used when the user did not express a preference yet.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns><see langword="true"/> when the theme cache is enabled for the user, <see langword="false"/> otherwise.</returns>
    public async Task<bool> GetAsync(Guid userId, bool defaultValue, CancellationToken cancellationToken = default)
    {
        bool? preference = await _hybridCache.GetOrCreateAsync(
            BuildKey(userId),
            (cancellationToken) => new ValueTask<bool?>(defaultValue),
            new HybridCacheEntryOptions()
            {
                Expiration = s_preferenceLifetime
            },
            cancellationToken: cancellationToken).ConfigureAwait(false);
        return preference ?? defaultValue;
    }

    /// <summary>
    /// Sets whether the theme cache is enabled for the user identified by <paramref name="userId"/>.
    /// </summary>
    /// <param name="userId">The unique identifier of the user whose preference is written.</param>
    /// <param name="isEnabled">Whether the theme cache is enabled for the user, or not.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An awaitable task representing the asynchronous operation.</returns>
    public async Task SetAsync(Guid userId, bool isEnabled, CancellationToken cancellationToken = default)
    {
        // the value is stored as a nullable bool, matching the read type, since HybridCache keys entries by their stored type
        await _hybridCache.SetAsync(BuildKey(userId), (bool?)isEnabled, new HybridCacheEntryOptions()
        {
            Expiration = s_preferenceLifetime
        }, cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Builds the cache key of the theme cache preference of the user identified by <paramref name="userId"/>.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <returns>The cache key of the preference.</returns>
    private static string BuildKey(Guid userId)
    {
        return $"{PREFERENCE_KEY_PREFIX}{userId}";
    }
}
