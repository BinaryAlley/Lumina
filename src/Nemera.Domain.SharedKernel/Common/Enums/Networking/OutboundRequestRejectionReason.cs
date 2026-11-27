namespace Nemera.Domain.SharedKernel.Common.Enums.Networking;

/// <summary>
/// Enumeration for the reasons an outbound request was rejected by the host address policy.
/// </summary>
public enum OutboundRequestRejectionReason
{
    /// <summary>
    /// The request was not rejected.
    /// </summary>
    None,

    /// <summary>
    /// The host name could not be resolved.
    /// </summary>
    HostCouldNotBeResolved,

    /// <summary>
    /// The host resolved to an address that is not public and was not explicitly allowed.
    /// </summary>
    AddressNotAllowed,

    /// <summary>
    /// The host resolved to a link local or cloud metadata address, which is never allowed.
    /// </summary>
    LinkLocalOrMetadataAddress,

    /// <summary>
    /// The redirect target is not an HTTPS URL.
    /// </summary>
    RedirectSchemeNotHttps,

    /// <summary>
    /// The redirect target could not be resolved from the location of the response.
    /// </summary>
    RedirectTargetInvalid,

    /// <summary>
    /// The request was redirected too many times.
    /// </summary>
    TooManyRedirects,

    /// <summary>
    /// The outbound request has no absolute URI, so its host cannot be validated.
    /// </summary>
    RequestHasNoAbsoluteUri
}
