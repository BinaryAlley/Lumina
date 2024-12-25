#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Models.Common;
using Microsoft.Extensions.Caching.Hybrid;
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
            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        // otherwise, check the hybrid cache to see if there is a cached authorization
        CachedResponseModel response = await _hybridCache.GetOrCreateAsync(
            AUTHORIZATION_ENDPOINT,
            async (cancellationToken) =>
            {
                // perform the actual API call if cache is empty or expired
                HttpResponseMessage result = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
                string content = await result.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                // cache both status code and content
                return new CachedResponseModel
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
        if (response.StatusCode == HttpStatusCode.Unauthorized)
            await _hybridCache.RemoveAsync(AUTHORIZATION_ENDPOINT, cancellationToken).ConfigureAwait(false);
        return new HttpResponseMessage(response.StatusCode)
        {
            Content = new StringContent(response.Content)
        };
    }
}
