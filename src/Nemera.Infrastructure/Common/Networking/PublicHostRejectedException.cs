#region ========================================================================= USING =====================================================================================
using Nemera.Domain.SharedKernel.Common.Enums.Networking;
using System.Net.Http;
#endregion

namespace Nemera.Infrastructure.Common.Networking;

/// <summary>
/// The exception thrown when an outbound request is rejected by the host address policy.
/// </summary>
/// <remarks>
/// The <see cref="HttpRequestError.ConfigurationLimitExceeded"/> error is used because it is a deterministic configuration error, which tells the provider clients
/// that the request must not be retried, unlike a transient transport failure. When the exception is thrown by the connect callback, SocketsHttpHandler wraps it in
/// an <see cref="HttpRequestException"/> whose error is <see cref="HttpRequestError.ConnectionError"/> and whose inner exception is this one, so consumers inspect
/// the exception chain to recognize the policy rejection.
/// </remarks>
internal sealed class PublicHostRejectedException : HttpRequestException
{
    /// <summary>
    /// Gets the reason the host was rejected.
    /// </summary>
    public OutboundRequestRejectionReason RejectionReason { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PublicHostRejectedException"/> class.
    /// </summary>
    /// <param name="host">The host that was rejected.</param>
    /// <param name="rejectionReason">The reason the host was rejected.</param>
    public PublicHostRejectedException(string host, OutboundRequestRejectionReason rejectionReason)
        : base(HttpRequestError.ConfigurationLimitExceeded, BuildMessage(host, rejectionReason))
    {
        RejectionReason = rejectionReason;
    }

    /// <summary>
    /// Builds the message of the exception, which names the host but never the resolved address, so an internal address does not reach the logs.
    /// </summary>
    /// <param name="host">The host that was rejected.</param>
    /// <param name="rejectionReason">The reason the host was rejected.</param>
    /// <returns>The message of the exception.</returns>
    private static string BuildMessage(string host, OutboundRequestRejectionReason rejectionReason)
    {
        return rejectionReason switch
        {
            OutboundRequestRejectionReason.LinkLocalOrMetadataAddress => $"The host '{host}' resolved to a link local or cloud metadata address, which is not allowed.",
            OutboundRequestRejectionReason.RedirectSchemeNotHttps => $"The request to the host '{host}' was redirected to a target that is not an HTTPS URL.",
            OutboundRequestRejectionReason.RedirectTargetInvalid => $"The request to the host '{host}' was redirected to a target that could not be resolved.",
            OutboundRequestRejectionReason.TooManyRedirects => $"The request to the host '{host}' was redirected too many times.",
            OutboundRequestRejectionReason.RequestHasNoAbsoluteUri => "The outbound request has no absolute URI, so its host could not be validated.",
            _ => $"The host '{host}' points at an address that is not allowed."
        };
    }
}
