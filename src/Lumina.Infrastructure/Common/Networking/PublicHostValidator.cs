#region ========================================================================= USING =====================================================================================
using System;
using System.Net;
using System.Net.Sockets;
#endregion

namespace Lumina.Infrastructure.Common.Networking;

/// <summary>
/// Determines whether a host is public, so that a request can never be sent to a private, loopback, link local or otherwise internal address.
/// </summary>
internal static class PublicHostValidator
{
    /// <summary>
    /// Determines whether the provided host is a public host.
    /// </summary>
    /// <param name="host">The host to check.</param>
    /// <returns><see langword="true"/> when the host is public, otherwise <see langword="false"/>.</returns>
    /// <remarks>
    /// A host name is accepted as is, because it cannot be classified without resolving it, which is deliberately not done here. An IP literal is accepted
    /// only when it is a public address, so the unspecified, private, loopback, link local and carrier grade NAT ranges are all rejected.
    /// </remarks>
    public static bool IsPublicHost(string host)
    {
        if (string.IsNullOrWhiteSpace(host))
            return false;

        // Uri.Host surrounds an IPv6 literal with brackets, which IPAddress.TryParse does not accept.
        string normalizedHost = host.Trim('[', ']');
        if (string.Equals(normalizedHost, "localhost", StringComparison.OrdinalIgnoreCase))
            return false;

        if (!IPAddress.TryParse(normalizedHost, out IPAddress? address))
            return true;

        if (address.IsIPv4MappedToIPv6)
            address = address.MapToIPv4();

        if (IPAddress.IsLoopback(address) || address.Equals(IPAddress.Any) || address.Equals(IPAddress.IPv6Any))
            return false;

        if (address.AddressFamily == AddressFamily.InterNetwork)
        {
            byte[] octets = address.GetAddressBytes();
            return octets[0] switch
            {
                0 => false,
                10 => false,
                100 when octets[1] is >= 64 and <= 127 => false,
                127 => false,
                169 when octets[1] == 254 => false,
                172 when octets[1] is >= 16 and <= 31 => false,
                192 when octets[1] == 168 => false,
                _ => true
            };
        }

        if (address.IsIPv6LinkLocal || address.IsIPv6SiteLocal)
            return false;

        byte[] addressBytes = address.GetAddressBytes();
        // The fc00::/7 unique local range is rejected, matching the private IPv4 ranges.
        if ((addressBytes[0] & 0xFE) == 0xFC)
            return false;

        return true;
    }
}
