#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.Networking;
using Lumina.Plugins.Contracts.Common.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Infrastructure.Common.Networking;

/// <summary>
/// Message handler that follows the redirects of a bodyless request only to public HTTPS hosts, so that a server controlled redirect cannot make the host issue a
/// request to a private, loopback, link local or otherwise internal address. It is provider agnostic: it protects every request of the clients it is attached to.
/// </summary>
/// <remarks>
/// The host of the initial request is validated by <see cref="PublicHostValidationHandler"/>, which runs before this handler. The host of every redirect is
/// validated here, because the requests created to follow a redirect are sent below the validation handler and never reach it. Their connections are still
/// validated by the connect callback of the outbound client. A client with its own redirect policy disables the redirect handling of the host through
/// <see cref="OutboundRequestOptions.FollowRedirects"/> and receives the redirect response instead.
/// </remarks>
internal sealed class PublicHostRedirectHandler : DelegatingHandler
{
    /// <summary>
    /// The maximum number of redirects that are followed before the request is considered to be looping.
    /// </summary>
    private const int MAXIMUM_REDIRECT_COUNT = 5;

    private readonly PublicHostValidator _publicHostValidator;
    private readonly ILogger<PublicHostRedirectHandler> _logger;

    // Headers that describe a single connection rather than the request, so they must never be replayed on the connection made for a redirect.
    private static readonly HashSet<string> s_hopByHopHeaders = new(StringComparer.OrdinalIgnoreCase)
    {
        "Connection",
        "Keep-Alive",
        "Proxy-Authenticate",
        "Proxy-Authorization",
        "Proxy-Connection",
        "TE",
        "Trailer",
        "Transfer-Encoding",
        "Upgrade"
    };

    // Headers that belong to the origin of the request, so they must not be forwarded when a redirect points at another origin.
    private static readonly HashSet<string> s_crossOriginHeaders = new(StringComparer.OrdinalIgnoreCase)
    {
        "Authorization",
        "Cookie",
        "Cookie2",
        "Host"
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="PublicHostRedirectHandler"/> class.
    /// </summary>
    /// <param name="publicHostValidator">The validator that decides whether a host is public.</param>
    /// <param name="logger">The logger that records the rejected redirects.</param>
    public PublicHostRedirectHandler(PublicHostValidator publicHostValidator, ILogger<PublicHostRedirectHandler> logger)
    {
        _publicHostValidator = publicHostValidator;
        _logger = logger;
    }

    /// <summary>
    /// Sends the request, following its redirects only to public HTTPS hosts.
    /// </summary>
    /// <param name="request">The request to send.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The final response of the request.</returns>
    /// <exception cref="PublicHostResolutionException">A redirect target could not be resolved.</exception>
    /// <exception cref="PublicHostRejectedException">A redirect points to a host or a scheme that is not allowed, or too many redirects are followed.</exception>
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // A client with its own redirect policy opts out, so that this handler never consumes a redirect it cannot validate against that policy.
        if (!ShouldFollowRedirects(request))
            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

        HttpRequestMessage currentRequest = request;
        // The requests created here to follow a redirect are owned by this handler, so they are disposed once the sending is complete.
        List<HttpRequestMessage> redirectRequests = [];

        try
        {
            for (int redirectCount = 0; ; redirectCount++)
            {
                Uri requestUri = currentRequest.RequestUri ?? throw new HttpRequestException("The request has no request URI.");

                HttpResponseMessage response = await base.SendAsync(currentRequest, cancellationToken).ConfigureAwait(false);

                if (!IsRedirect(response.StatusCode) || response.Headers.Location is null || !IsFollowableMethod(currentRequest.Method))
                    return response;

                Uri? redirectUri = ResolveRedirect(requestUri, response.Headers.Location);
                // The response of a redirect is not returned, so it must be disposed here to release its connection.
                response.Dispose();

                if (redirectCount >= MAXIMUM_REDIRECT_COUNT)
                    throw Reject(requestUri.IdnHost, OutboundRequestRejectionReason.TooManyRedirects);

                if (redirectUri is null)
                    throw Reject(requestUri.IdnHost, OutboundRequestRejectionReason.RedirectTargetInvalid);

                // Only public HTTPS targets are followed, so a redirect can never reach a private, loopback, link local or other internal address,
                // nor downgrade the transfer back to an unencrypted one. A request that was explicitly allowed to reach private endpoints keeps that
                // permission only for the host it was granted for.
                if (redirectUri.Scheme != Uri.UriSchemeHttps)
                    throw Reject(redirectUri.IdnHost, OutboundRequestRejectionReason.RedirectSchemeNotHttps);

                PublicHostValidationResult result = await _publicHostValidator.ValidateHostAsync(redirectUri.IdnHost, GetAllowedPrivateHost(currentRequest), cancellationToken).ConfigureAwait(false);
                if (!result.IsAllowed)
                {
                    if (result.RejectionReason == OutboundRequestRejectionReason.HostCouldNotBeResolved)
                    {
                        if (_logger.IsEnabled(LogLevel.Warning))
                            _logger.LogWarning("Could not resolve the outbound redirect host '{Host}'. The request was not sent.", redirectUri.IdnHost);
                        throw new PublicHostResolutionException(redirectUri.IdnHost);
                    }
                    throw Reject(redirectUri.IdnHost, result.RejectionReason);
                }

                HttpRequestMessage redirectRequest = CloneRequest(currentRequest, requestUri, redirectUri);
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
    /// Logs and creates the exception that reports a redirect rejected by the outbound address policy.
    /// </summary>
    /// <param name="host">The host the redirect points at, or the host of the request when the redirect has no usable target.</param>
    /// <param name="rejectionReason">The reason the redirect was rejected.</param>
    /// <returns>The exception that reports the rejection.</returns>
    private PublicHostRejectedException Reject(string host, OutboundRequestRejectionReason rejectionReason)
    {
        if (_logger.IsEnabled(LogLevel.Warning))
            _logger.LogWarning("Rejected the outbound redirect to the host '{Host}' because {RejectionReason}. The request was not sent.", host, rejectionReason);
        return new PublicHostRejectedException(host, rejectionReason);
    }

    /// <summary>
    /// Determines whether the host should follow the redirects of the provided request, which is the default, unless the client opted out to apply its own policy.
    /// </summary>
    /// <param name="request">The request to check.</param>
    /// <returns><see langword="true"/> when the host should follow the redirects, otherwise <see langword="false"/>.</returns>
    private static bool ShouldFollowRedirects(HttpRequestMessage request)
    {
        return !request.Options.TryGetValue(OutboundRequestOptions.FollowRedirects, out bool shouldFollowRedirects) || shouldFollowRedirects;
    }

    /// <summary>
    /// Gets the single host the request was explicitly allowed to reach private or local addresses on, which is how a user opted into a provider that runs on the
    /// local network, like a self hosted MusicBrainz instance.
    /// </summary>
    /// <param name="request">The request to check.</param>
    /// <returns>The host that may be private, or <see langword="null"/> when no such permission was granted.</returns>
    private static string? GetAllowedPrivateHost(HttpRequestMessage request)
    {
        if (request.Options.TryGetValue(OutboundRequestOptions.AllowedPrivateHost, out string? allowedPrivateHost))
            return allowedPrivateHost;
        return null;
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
    /// Determines whether the redirect of a request with the provided method can be followed without replaying a body.
    /// </summary>
    /// <param name="method">The method of the request that was redirected.</param>
    /// <returns><see langword="true"/> when the method has no body and its redirect can be followed, otherwise <see langword="false"/>.</returns>
    private static bool IsFollowableMethod(HttpMethod method)
    {
        return HttpMethod.Get.Equals(method) || HttpMethod.Head.Equals(method);
    }

    /// <summary>
    /// Determines whether the provided URIs share a scheme, a host and a port, which makes them the same origin.
    /// </summary>
    /// <param name="first">The first URI to compare.</param>
    /// <param name="second">The second URI to compare.</param>
    /// <returns><see langword="true"/> when both URIs have the same origin, otherwise <see langword="false"/>.</returns>
    private static bool IsSameOrigin(Uri first, Uri second)
    {
        return string.Equals(first.Scheme, second.Scheme, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(first.IdnHost, second.IdnHost, StringComparison.OrdinalIgnoreCase) &&
            first.Port == second.Port;
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
    /// Creates the request used to follow a redirect to <paramref name="redirectUri"/>, carrying over the method, the safe headers, and the private host permission
    /// of <paramref name="request"/>. It is meant for the bodyless requests this handler follows.
    /// </summary>
    /// <param name="request">The request that is being redirected.</param>
    /// <param name="requestUri">The URI of the request the redirect was returned for.</param>
    /// <param name="redirectUri">The URI of the redirect target.</param>
    /// <returns>The request used to follow the redirect.</returns>
    /// <remarks>
    /// The credentials of the request, and the headers that describe its origin, are forwarded only when the redirect stays on the same origin, matching the
    /// behavior of the built in redirect handling. The hop by hop headers are never forwarded, because they only apply to the connection that is being replaced.
    /// </remarks>
    private static HttpRequestMessage CloneRequest(HttpRequestMessage request, Uri requestUri, Uri redirectUri)
    {
        HttpRequestMessage redirectRequest = new(request.Method, redirectUri)
        {
            Version = request.Version,
            VersionPolicy = request.VersionPolicy
        };
        bool isSameOrigin = IsSameOrigin(requestUri, redirectUri);
        foreach (KeyValuePair<string, IEnumerable<string>> header in request.Headers)
        {
            if (s_hopByHopHeaders.Contains(header.Key))
                continue;
            if (!isSameOrigin && s_crossOriginHeaders.Contains(header.Key))
                continue;
            redirectRequest.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }
        if (!isSameOrigin)
            redirectRequest.Headers.Host = null;
        if (request.Options.TryGetValue(OutboundRequestOptions.AllowedPrivateHost, out string? allowedPrivateHost))
            redirectRequest.Options.Set(OutboundRequestOptions.AllowedPrivateHost, allowedPrivateHost);
        return redirectRequest;
    }
}
