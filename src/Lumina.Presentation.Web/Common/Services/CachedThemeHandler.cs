#region ========================================================================= USING =====================================================================================
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Hybrid;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Common.Services;

/// <summary>
/// Delegating handler that intercepts the HTTP requests to the theme endpoints of the remote API and caches their responses,
/// because the theme data is read on every page render and is only changed by the theme management operations.
/// All other requests are forwarded directly.
/// </summary>
public class CachedThemeHandler : DelegatingHandler
{
    private const string THEMES_SEGMENT = "/api/v1/themes";
    private const string THEME_CACHE_TAG = "themes";
    // responses above this size are never kept in the cache, so large binary assets do not bloat the memory
    private const int MAX_CACHEABLE_BYTES = 512 * 1024;
    // themes only change when the user switches, reinstalls or deletes them, so the cached responses stay valid for a long time
    private static readonly TimeSpan s_cacheExpiration = TimeSpan.FromMinutes(30);

    private readonly HybridCache _hybridCache;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ThemeCachePreferenceService _themeCachePreferenceService;

    /// <summary>
    /// Initializes a new instance of the <see cref="CachedThemeHandler"/> class.
    /// </summary>
    /// <param name="hybridCache">The hybrid caching mechanism used to store and retrieve the theme responses.</param>
    /// <param name="httpContextAccessor">The <see cref="IHttpContextAccessor"/> used to access the currently authenticated user.</param>
    /// <param name="themeCachePreferenceService">The service used to read the per-user theme cache preference.</param>
    public CachedThemeHandler(HybridCache hybridCache, IHttpContextAccessor httpContextAccessor, ThemeCachePreferenceService themeCachePreferenceService)
    {
        _hybridCache = hybridCache;
        _httpContextAccessor = httpContextAccessor;
        _themeCachePreferenceService = themeCachePreferenceService;
    }

    /// <summary>
    /// Processes the HTTP request, caching the responses of the GET requests to theme endpoints and invalidating the cached
    /// responses whenever a theme mutation is performed. All other requests are forwarded to the next handler in the pipeline.
    /// </summary>
    /// <param name="request">The HTTP request message to process.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The HTTP response message from either the cache or the original API call.</returns>
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        string requestPath = request.RequestUri!.AbsolutePath;
        if (!requestPath.StartsWith(THEMES_SEGMENT, StringComparison.OrdinalIgnoreCase))
            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

        // a theme mutation replaces the theme files or the active selection, so every cached theme response becomes stale
        if (!request.Method.Equals(HttpMethod.Get))
        {
            HttpResponseMessage mutationResponse = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
            if (mutationResponse.IsSuccessStatusCode)
                await _hybridCache.RemoveByTagAsync(THEME_CACHE_TAG, cancellationToken).ConfigureAwait(false);
            return mutationResponse;
        }

        // the current user can switch off the theme cache from the settings page, so their requests always receive the freshly edited theme files
        Guid? userId = GetCurrentUserId();
        if (userId is not null)
        {
            bool isCachingEnabledForUser = await _themeCachePreferenceService.GetAsync(userId.Value, defaultValue: true, cancellationToken).ConfigureAwait(false);
            if (!isCachingEnabledForUser)
                return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }

        // key the cached responses by the full request path and query, since each theme endpoint identifies a distinct resource
        string cacheKey = $"themes:{request.RequestUri.PathAndQuery}";
        CachedResponse cached = await _hybridCache.GetOrCreateAsync(
            cacheKey,
            async (cancellationToken) =>
            {
                HttpResponseMessage response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
                byte[] bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
                return new CachedResponse
                {
                    Bytes = bytes,
                    StatusCode = response.StatusCode,
                    ContentType = response.Content.Headers.ContentType?.ToString()
                };
            },
            new HybridCacheEntryOptions()
            {
                Expiration = s_cacheExpiration
            },
            [THEME_CACHE_TAG],
            cancellationToken);

        // failed responses are only served from the call that produced them, so a temporary failure can never be served from a stale entry
        if (!IsSuccessful(cached.StatusCode) || cached.Bytes.Length > MAX_CACHEABLE_BYTES)
            await _hybridCache.RemoveAsync(cacheKey, cancellationToken).ConfigureAwait(false);

        return BuildResponse(cached);
    }

    /// <summary>
    /// Gets the unique identifier of the currently authenticated user, when there is one.
    /// </summary>
    /// <returns>The unique identifier of the current user, or <see langword="null"/> when the request is anonymous.</returns>
    private Guid? GetCurrentUserId()
    {
        string? userId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userId, out Guid parsedUserId) ? parsedUserId : null;
    }

    /// <summary>
    /// Checks whether the given status code represents a successful HTTP response.
    /// </summary>
    /// <param name="statusCode">The status code to check.</param>
    /// <returns><see langword="true"/> when the status code is a successful one, <see langword="false"/> otherwise.</returns>
    private static bool IsSuccessful(HttpStatusCode statusCode)
    {
        return statusCode >= HttpStatusCode.OK && statusCode < HttpStatusCode.MultipleChoices;
    }

    /// <summary>
    /// Rebuilds the HTTP response message from the cached response data.
    /// </summary>
    /// <param name="cached">The cached response data.</param>
    /// <returns>The rebuilt HTTP response message.</returns>
    private static HttpResponseMessage BuildResponse(CachedResponse cached)
    {
        HttpResponseMessage response = new(cached.StatusCode) { Content = new ByteArrayContent(cached.Bytes) };
        if (!string.IsNullOrWhiteSpace(cached.ContentType))
            response.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(cached.ContentType);
        return response;
    }
}
