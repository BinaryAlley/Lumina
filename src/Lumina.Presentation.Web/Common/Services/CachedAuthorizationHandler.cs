#region ========================================================================= USING =====================================================================================
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Net;
using System.Net.Http;
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
    private const string AUTHORIZATION_ENDPOINT = "/auth/get-authorization";

    /// <summary>
    /// Initializes a new instance of the <see cref="CachedAuthorizationHandler"/> class with the specified hybrid cache.
    /// </summary>
    /// <param name="hybridCache">The hybrid caching mechanism to store and retrieve authorization responses.</param>
    public CachedAuthorizationHandler(HybridCache hybridCache)
    {
        _hybridCache = hybridCache;
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
        // if it's any endpoint other than the one for the authorization request, fire it away towards the original API service
        if (!requestPath.EndsWith(AUTHORIZATION_ENDPOINT, StringComparison.OrdinalIgnoreCase))
            return await base.SendAsync(request, cancellationToken);
        // otherwise, check the hybrid cache to see if there is a cached authorization
        string response = await _hybridCache.GetOrCreateAsync(
            AUTHORIZATION_ENDPOINT,
            async (cancellationToken) =>
            {
                // perform the actual API call if cache is empty or expired
                HttpResponseMessage result = await base.SendAsync(request, cancellationToken);
                return await result.Content.ReadAsStringAsync(cancellationToken);
            },
            new HybridCacheEntryOptions()
            {
                Expiration = TimeSpan.FromMinutes(5) // cache for the next five minutes
            },
            cancellationToken: cancellationToken
        );
        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(response)
        };
    }
}
