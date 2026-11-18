#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.Networking;
using Lumina.Plugins.Contracts.Common.Http;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Infrastructure.Common.Networking;

/// <summary>
/// Message handler that validates the host of the outbound requests it sees, resolving it and rejecting it when it is not public, so that a URL chosen by a provider
/// can never make the host issue a request to a private, loopback, link local or otherwise internal address.
/// </summary>
/// <remarks>
/// The handler runs for the initial request of the clients it is attached to. A request that is created by a redirect handler is sent below this handler, so it
/// is validated by that redirect handler instead, and every connection is validated again by the connect callback installed alongside this handler.
/// </remarks>
internal sealed class PublicHostValidationHandler : DelegatingHandler
{
    private readonly PublicHostValidator _publicHostValidator;
    private readonly ILogger<PublicHostValidationHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PublicHostValidationHandler"/> class.
    /// </summary>
    /// <param name="publicHostValidator">The validator that decides whether a host is public.</param>
    /// <param name="logger">The logger that records the rejected requests.</param>
    public PublicHostValidationHandler(PublicHostValidator publicHostValidator, ILogger<PublicHostValidationHandler> logger)
    {
        _publicHostValidator = publicHostValidator;
        _logger = logger;
    }

    /// <summary>
    /// Sends the request, after validating that its host is public.
    /// </summary>
    /// <param name="request">The request to send.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The response of the request.</returns>
    /// <exception cref="PublicHostResolutionException">The host of the request could not be resolved.</exception>
    /// <exception cref="PublicHostRejectedException">The host of the request is not public, or the request has no absolute URI.</exception>
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.RequestUri is not { IsAbsoluteUri: true } requestUri)
        {
            _logger.LogWarning("Rejected an outbound request that has no absolute URI, because its host could not be validated.");
            throw new PublicHostRejectedException(string.Empty, OutboundRequestRejectionReason.RequestHasNoAbsoluteUri);
        }

        PublicHostValidationResult result = await _publicHostValidator.ValidateHostAsync(requestUri.IdnHost, GetAllowedPrivateHost(request), cancellationToken).ConfigureAwait(false);
        if (result.IsAllowed)
            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

        if (result.RejectionReason == OutboundRequestRejectionReason.HostCouldNotBeResolved)
        {
            _logger.LogWarning("Could not resolve the outbound host '{Host}'. The request was not sent.", requestUri.IdnHost);
            throw new PublicHostResolutionException(requestUri.IdnHost);
        }

        _logger.LogWarning("Rejected the outbound request to the host '{Host}' because {RejectionReason}. The request was not sent.", requestUri.IdnHost, result.RejectionReason);
        throw new PublicHostRejectedException(requestUri.IdnHost, result.RejectionReason);
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
}
