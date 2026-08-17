#region ========================================================================= USING =====================================================================================
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Hybrid;
using System;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Common.Services;

/// <summary>
/// Delegating handler that intercepts HTTP requests to an authorization endpoint and caches the response. All other requests are forwarded directly.
/// </summary>
public class CachedAuthorizationHandler : DelegatingHandler
{
    private readonly HybridCache _hybridCache;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private const string AUTHORIZATION_ENDPOINT = "/auth/get-authorization";
    private const string LOGIN_ENDPOINT = "/api/v1/auth/login";

    /// <summary>
    /// Initializes a new instance of the <see cref="CachedAuthorizationHandler"/> class.
    /// </summary>
    /// <param name="hybridCache">The hybrid caching mechanism to store and retrieve authorization responses.</param>
    /// <param name="httpContextAccessor">The <see cref="IHttpContextAccessor"/> used to access the currently authenticated user.</param>
    public CachedAuthorizationHandler(HybridCache hybridCache, IHttpContextAccessor httpContextAccessor)
    {
        _hybridCache = hybridCache;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Processes HTTP requests and intercepts calls to the authorization endpoint to provide a cached response.
    /// Other requests are forwarded to the next handler in the pipeline.
    /// </summary>
    /// <param name="request">The HTTP request message to process.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The HTTP response message from either the cache or the original API call.</returns>
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // get the called API endpoint
        string requestPath = request.RequestUri!.AbsolutePath;
        string cacheKey = GetAuthorizationCacheKey();
        // if it's any endpoint other than the one for the authorization request, fire it away towards the original API service
        if (!requestPath.EndsWith(AUTHORIZATION_ENDPOINT, StringComparison.OrdinalIgnoreCase))
        {
            if (requestPath.EndsWith(LOGIN_ENDPOINT)) // if login endpoint was hit, a new user is now present, delete previous cached permissions
                await _hybridCache.RemoveAsync(cacheKey, cancellationToken).ConfigureAwait(false);
            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
        // otherwise, check the hybrid cache to see if there is a cached authorization
        CachedResponse response = await _hybridCache.GetOrCreateAsync(
            cacheKey,
            async (cancellationToken) =>
            {
                // perform the actual API call if cache is empty or expired
                HttpResponseMessage result = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
                string content = await result.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                // cache both status code and content
                return new CachedResponse
                {
                    Content = content,
                    StatusCode = result.StatusCode
                };
            },
            new HybridCacheEntryOptions()
            {
                Expiration = TimeSpan.FromMinutes(5) // cache for the next five minutes
            },
            cancellationToken: cancellationToken
        );
        // if the API returned 401 Unauthorized (i.e. token expired), DO NOT cache this response - it will be used for authorization endpoints even after successful login,
        // resulting in incorrect login redirection cycles!
        if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.NotFound)
            await _hybridCache.RemoveAsync(cacheKey, cancellationToken).ConfigureAwait(false);
        return new HttpResponseMessage(response.StatusCode)
        {
            Content = new StringContent(response.Content)
        };
    }

    /// <summary>
    /// Builds the hybrid cache key for the authorization response, scoped to the currently authenticated user.
    /// </summary>
    /// <returns>The cache key for the currently authenticated user's authorization response.</returns>
    private string GetAuthorizationCacheKey()
    {
        // key the cache entry by the authenticated user's id to prevent one user's authorization response from being served to another user
        string? userId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return userId is null ? AUTHORIZATION_ENDPOINT : $"{AUTHORIZATION_ENDPOINT}:{userId}";
    }
}
