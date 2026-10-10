#region ========================================================================= USING =====================================================================================
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Infrastructure.Common.Networking;

/// <summary>
/// Message handler that validates the host of the initial request, and follows the redirects of the response only to public HTTPS hosts, so that
/// neither the URL emitted by a provider nor a server controlled redirect can make the host issue a request to a private, loopback, link local or
/// otherwise internal address. It is provider agnostic: it protects every download that is routed through the client it is attached to.
/// </summary>
internal sealed class PublicHostRedirectHandler : DelegatingHandler
{
    /// <summary>
    /// The maximum number of redirects that are followed before the request is considered to be looping.
    /// </summary>
    private const int MAXIMUM_REDIRECT_COUNT = 5;

    /// <summary>
    /// Sends the request, validating that its host is public, and following its redirects only to public HTTPS hosts.
    /// </summary>
    /// <param name="request">The request to send.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The final response of the request.</returns>
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // The sink is provider agnostic, so the initial request is validated too, not only the redirects, so a provider that emits an internal URL
        // cannot make the host fetch a private, loopback or link local address before any redirect is even involved.
        Uri? initialUri = request.RequestUri;
        string initialHost = initialUri is { IsAbsoluteUri: true } ? initialUri.Host : string.Empty;
        if (initialUri is not { IsAbsoluteUri: true } || !PublicHostValidator.IsPublicHost(initialHost))
            throw new HttpRequestException($"The request was sent to the disallowed host '{initialHost}'.");

        HttpRequestMessage currentRequest = request;
        // The requests created here to follow a redirect are owned by this handler, so they are disposed once the sending is complete.
        List<HttpRequestMessage> redirectRequests = [];

        try
        {
            for (int redirectCount = 0; ; redirectCount++)
            {
                Uri requestUri = currentRequest.RequestUri ?? throw new HttpRequestException("The request has no request URI.");

                HttpResponseMessage response = await base.SendAsync(currentRequest, cancellationToken).ConfigureAwait(false);

                if (!IsRedirect(response.StatusCode) || response.Headers.Location is null)
                    return response;

                Uri? redirectUri = ResolveRedirect(requestUri, response.Headers.Location);
                // The response of a redirect is not returned, so it must be disposed here to release its connection.
                response.Dispose();

                if (redirectCount >= MAXIMUM_REDIRECT_COUNT)
                    throw new HttpRequestException("The request was redirected too many times.");

                // Only public HTTPS targets are followed, so a redirect can never reach a private, loopback, link local or other internal address,
                // nor downgrade the transfer back to an unencrypted one.
                if (redirectUri is null || redirectUri.Scheme != Uri.UriSchemeHttps || !PublicHostValidator.IsPublicHost(redirectUri.Host))
                    throw new HttpRequestException($"The request was redirected to the disallowed host '{redirectUri?.Host}'.");

                HttpRequestMessage redirectRequest = CloneRequest(currentRequest, redirectUri);
                redirectRequests.Add(redirectRequest);
                currentRequest = redirectRequest;
            }
        }
        finally
        {
            foreach (HttpRequestMessage redirectRequest in redirectRequests)
                redirectRequest.Dispose();
        }
    }

    /// <summary>
    /// Determines whether the provided status code is a redirect.
    /// </summary>
    /// <param name="statusCode">The status code to check.</param>
    /// <returns><see langword="true"/> when the status code is a redirect, otherwise <see langword="false"/>.</returns>
    private static bool IsRedirect(HttpStatusCode statusCode)
    {
        return statusCode is HttpStatusCode.MovedPermanently or HttpStatusCode.Found or HttpStatusCode.SeeOther
            or HttpStatusCode.TemporaryRedirect or HttpStatusCode.PermanentRedirect;
    }

    /// <summary>
    /// Resolves the location of a redirect against the URI of the request it was returned for.
    /// </summary>
    /// <param name="requestUri">The URI of the request the redirect was returned for.</param>
    /// <param name="location">The location of the redirect.</param>
    /// <returns>The absolute URI of the redirect, or <see langword="null"/> when it could not be resolved.</returns>
    private static Uri? ResolveRedirect(Uri requestUri, Uri location)
    {
        if (location.IsAbsoluteUri)
            return location;
        return Uri.TryCreate(requestUri, location, out Uri? resolvedUri) ? resolvedUri : null;
    }

    /// <summary>
    /// Creates the request used to follow a redirect to <paramref name="requestUri"/>, carrying over the method and the headers of
    /// <paramref name="request"/>. It is meant for the bodyless GET downloads this handler is registered for.
    /// </summary>
    /// <param name="request">The request that is being redirected.</param>
    /// <param name="requestUri">The URI of the redirect target.</param>
    /// <returns>The request used to follow the redirect.</returns>
    private static HttpRequestMessage CloneRequest(HttpRequestMessage request, Uri requestUri)
    {
        HttpRequestMessage redirectRequest = new(request.Method, requestUri)
        {
            Version = request.Version,
            VersionPolicy = request.VersionPolicy
        };
        foreach (KeyValuePair<string, IEnumerable<string>> header in request.Headers)
            redirectRequest.Headers.TryAddWithoutValidation(header.Key, header.Value);
        return redirectRequest;
    }
}
