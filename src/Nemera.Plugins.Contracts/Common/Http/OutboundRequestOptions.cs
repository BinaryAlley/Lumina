#region ========================================================================= USING =====================================================================================
using System.Net.Http;
#endregion

namespace Lumina.Plugins.Contracts.Common.Http;

/// <summary>
/// Options that a request may set to control how the host establishes the connection it is sent through.
/// </summary>
public static class OutboundRequestOptions
{
    /// <summary>
    /// Gets the option that names the single host a request is allowed to reach private or local addresses on. A provider sets it only when the user
    /// explicitly enabled a private base address for it, so the permission applies to that host alone and not to any other host a redirect could point at.
    /// </summary>
    /// <remarks>
    /// Even on the allowed host, the link local and cloud metadata ranges are never reachable, and only the private, loopback and carrier grade NAT ranges are
    /// accepted, so enabling a private base address cannot be turned into a request to an unrelated internal service.
    /// </remarks>
    public static HttpRequestOptionsKey<string> AllowedPrivateHost { get; } = new(nameof(AllowedPrivateHost));

    /// <summary>
    /// Gets the option that disables the redirect handling of the host for a request whose client follows redirects itself. The host follows redirects by
    /// default, and a client that sets this option receives the redirect response and applies its own policy instead.
    /// </summary>
    public static HttpRequestOptionsKey<bool> FollowRedirects { get; } = new(nameof(FollowRedirects));
}
