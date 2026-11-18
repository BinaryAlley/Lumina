#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.Networking;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Infrastructure.Common.Networking;

/// <summary>
/// Determines whether a host is public, so that a request can never be sent to a private, loopback, link local or otherwise internal address.
/// </summary>
internal sealed class PublicHostValidator
{
    // A resolution is reused briefly, so the validation of a request and the connection that follows it do not resolve the same host twice. A negative result
    // is kept for a much shorter time, so a temporary resolution failure does not keep a host unusable for long. The cache is bounded, so a provider that emits
    // many distinct host names cannot make it grow without limit.
    private const int CACHED_ADDRESS_LIFETIME_IN_SECONDS = 60;
    private const int CACHED_ADDRESS_FAILURE_LIFETIME_IN_SECONDS = 5;
    private const int MAXIMUM_CACHED_HOST_COUNT = 1024;

    private readonly Func<string, CancellationToken, Task<IPAddress[]>> _hostAddressResolver;
    private readonly ConcurrentDictionary<string, CachedAddresses> _addressesCache = new(StringComparer.Ordinal);

    /// <summary>
    /// Initializes a new instance of the <see cref="PublicHostValidator"/> class, resolving host names through the operating system resolver.
    /// </summary>
    public PublicHostValidator()
        : this(Dns.GetHostAddressesAsync)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PublicHostValidator"/> class, resolving host names through the provided resolver.
    /// </summary>
    /// <param name="hostAddressResolver">The resolver used to resolve host names, so the resolution can be replaced when the validator is tested.</param>
    internal PublicHostValidator(Func<string, CancellationToken, Task<IPAddress[]>> hostAddressResolver)
    {
        _hostAddressResolver = hostAddressResolver;
    }

    /// <summary>
    /// Validates whether the provided host may be reached, resolving it when it is a host name.
    /// </summary>
    /// <param name="host">The host to validate.</param>
    /// <param name="allowedPrivateHost">The single host that was explicitly allowed to reach private or local addresses, or <see langword="null"/> when no such permission was granted.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The outcome of the validation, with the reason of a rejection when the host may not be reached.</returns>
    /// <remarks>
    /// A host name is accepted only when every address it resolves to is public, so a name that points at an internal address, or that mixes public and internal
    /// addresses, is rejected. A name that cannot be resolved is rejected as well, because a request to it could not be checked before it is sent. When the
    /// host is the one explicitly allowed to be private, its private addresses are accepted too, except the link local and cloud metadata ranges.
    /// </remarks>
    public async Task<PublicHostValidationResult> ValidateHostAsync(string host, string? allowedPrivateHost, CancellationToken cancellationToken)
    {
        // A host that is missing or blank cannot be classified, so it is rejected instead of being treated as public.
        if (string.IsNullOrWhiteSpace(host))
            return PublicHostValidationResult.Rejected(OutboundRequestRejectionReason.AddressNotAllowed);

        string normalizedHost = NormalizeHost(host);
        // localhost always resolves to loopback, so it is allowed only when it is the host the user explicitly opted into.
        if (string.Equals(normalizedHost, "localhost", StringComparison.OrdinalIgnoreCase))
            return allowedPrivateHost is not null && IsSameHost(normalizedHost, allowedPrivateHost)
                ? PublicHostValidationResult.Allowed
                : PublicHostValidationResult.Rejected(OutboundRequestRejectionReason.AddressNotAllowed);

        // An IP literal needs no DNS resolution, so it can be classified directly.
        if (IPAddress.TryParse(normalizedHost, out IPAddress? literalAddress))
            return EvaluateAddress(host, literalAddress, allowedPrivateHost);

        IPAddress[] addresses = await ResolveAddressesAsync(normalizedHost, cancellationToken).ConfigureAwait(false);
        // A name that cannot be resolved is rejected, because the address it would connect to could not be checked first.
        if (addresses.Length == 0)
            return PublicHostValidationResult.Rejected(OutboundRequestRejectionReason.HostCouldNotBeResolved);

        // Every address must be allowed, so a name that resolves to any internal address cannot be used to reach it by retrying the request.
        foreach (IPAddress address in addresses)
        {
            PublicHostValidationResult result = EvaluateAddress(host, address, allowedPrivateHost);
            if (!result.IsAllowed)
                return result;
        }

        return PublicHostValidationResult.Allowed;
    }

    /// <summary>
    /// Resolves the endpoint the provided host and port identify, rejecting the host when any address it resolves to is not public and not explicitly allowed.
    /// </summary>
    /// <param name="host">The host to resolve.</param>
    /// <param name="port">The port the resolved addresses are paired with.</param>
    /// <param name="allowedPrivateHost">The single host that was explicitly allowed to reach private or local addresses, or <see langword="null"/> when no such permission was granted.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The endpoints the host resolves to, with the duplicate addresses removed.</returns>
    /// <exception cref="PublicHostResolutionException">The host cannot be resolved.</exception>
    /// <exception cref="PublicHostRejectedException">The host resolves to an address that is neither public nor allowed.</exception>
    public async Task<IReadOnlyList<IPEndPoint>> ResolveEndPointsAsync(string host, int port, string? allowedPrivateHost, CancellationToken cancellationToken)
    {
        string normalizedHost = NormalizeHost(host);
        List<IPEndPoint> endPoints = [];

        // An IP literal needs no DNS resolution; it is validated and paired with the port directly.
        if (IPAddress.TryParse(normalizedHost, out IPAddress? literalAddress))
        {
            AddEndPoint(endPoints, literalAddress, port, host, allowedPrivateHost);
            return endPoints;
        }

        IPAddress[] addresses = await ResolveAddressesAsync(normalizedHost, cancellationToken).ConfigureAwait(false);

        // An empty result means there is no address to connect to, which is reported as a resolution failure rather than as an empty list.
        if (addresses.Length == 0)
            throw new PublicHostResolutionException(host);

        // Every address is validated before it is added, so the caller can only connect to endpoints that passed the policy.
        foreach (IPAddress address in addresses)
            AddEndPoint(endPoints, address, port, host, allowedPrivateHost);

        return endPoints;
    }

    /// <summary>
    /// Determines whether the provided address is public.
    /// </summary>
    /// <param name="address">The address to check.</param>
    /// <returns><see langword="true"/> when the address is public, otherwise <see langword="false"/>.</returns>
    /// <remarks>
    /// The loopback, unspecified, private, link local, carrier grade NAT, multicast, reserved and documentation ranges are all rejected, for both address families.
    /// </remarks>
    public static bool IsPublicAddress(IPAddress address)
    {
        // An IPv4-mapped address is an IPv4 address in disguise. Classifying it as IPv4 keeps both spellings under the same policy.
        if (address.IsIPv4MappedToIPv6)
            address = address.MapToIPv4();

        // The loopback and unspecified addresses can never be a remote public peer.
        if (IPAddress.IsLoopback(address) || address.Equals(IPAddress.Any) || address.Equals(IPAddress.IPv6Any))
            return false;

        // The two families have different range tables, so each one has its own classifier.
        return address.AddressFamily == AddressFamily.InterNetwork
            ? IsPublicIPv4Address(address)
            : IsPublicIPv6Address(address);
    }

    /// <summary>
    /// Normalizes the provided host, removing the brackets of an IPv6 literal and the trailing dot of a fully qualified name.
    /// </summary>
    /// <param name="host">The host to normalize.</param>
    /// <returns>The normalized host.</returns>
    private static string NormalizeHost(string host)
    {
        // Uri.Host keeps the brackets around an IPv6 literal and may keep the trailing dot of a fully qualified name. Neither spelling changes the host.
        return host.Trim('[', ']').TrimEnd('.');
    }

    /// <summary>
    /// Resolves the addresses of the provided host, reusing a recent resolution when one is cached.
    /// </summary>
    /// <param name="host">The host to resolve.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The addresses the host resolves to, or an empty collection when it cannot be resolved.</returns>
    /// <remarks>
    /// A cached resolution cannot open a hole in the guard, because every cached address is validated again before it is used, and the connection is made to
    /// the validated address itself. A resolution failure is cached only briefly, so a temporary failure does not keep a host unusable for long.
    /// </remarks>
    private async Task<IPAddress[]> ResolveAddressesAsync(string host, CancellationToken cancellationToken)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        // The cache key ignores case, a trailing dot and the IDN spelling, so every spelling of the same name shares one entry.
        string cacheKey = NormalizeHostForComparison(host);
        // A cached positive result avoids a second DNS lookup between the validation handler and the connect callback.
        if (_addressesCache.TryGetValue(cacheKey, out CachedAddresses? cachedAddresses) && cachedAddresses.ExpiresOnUtc > now)
            return cachedAddresses.Addresses;

        IPAddress[] addresses;
        DateTimeOffset expiresOnUtc;
        try
        {
            addresses = await _hostAddressResolver(host, cancellationToken).ConfigureAwait(false);
            // A successful lookup is cached longer than a failure. An empty result is a failure too, because there is no address to connect to.
            expiresOnUtc = now.AddSeconds(addresses.Length > 0 ? CACHED_ADDRESS_LIFETIME_IN_SECONDS : CACHED_ADDRESS_FAILURE_LIFETIME_IN_SECONDS);
        }
        catch (Exception exception) when (exception is SocketException or ArgumentException)
        {
            // A temporary DNS failure is cached only briefly, so the host becomes usable again quickly. Cancellation is not caught on purpose.
            addresses = [];
            expiresOnUtc = now.AddSeconds(CACHED_ADDRESS_FAILURE_LIFETIME_IN_SECONDS);
        }

        // The cache is bounded. When it is full, expired entries are pruned first; if it is still full, this result is simply not cached.
        if (_addressesCache.Count >= MAXIMUM_CACHED_HOST_COUNT)
            RemoveExpiredCacheEntries();
        if (_addressesCache.Count < MAXIMUM_CACHED_HOST_COUNT)
            _addressesCache[cacheKey] = new CachedAddresses(addresses, expiresOnUtc);

        return addresses;
    }

    /// <summary>
    /// Removes the cached resolutions whose lifetime expired, so the cache stays within its size limit.
    /// </summary>
    private void RemoveExpiredCacheEntries()
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        // The entries are removed one by one, because a concurrent dictionary cannot be cleared safely while it is being enumerated.
        foreach (KeyValuePair<string, CachedAddresses> cachedAddress in _addressesCache)
            if (cachedAddress.Value.ExpiresOnUtc <= now)
                _addressesCache.TryRemove(cachedAddress.Key, out _);
    }

    /// <summary>
    /// Adds the endpoint of the provided address to <paramref name="endPoints"/>, unless the address is a duplicate or it is neither public nor allowed.
    /// </summary>
    /// <param name="endPoints">The endpoints collected so far.</param>
    /// <param name="address">The address to add.</param>
    /// <param name="port">The port the address is paired with.</param>
    /// <param name="host">The host the address was resolved from.</param>
    /// <param name="allowedPrivateHost">The single host that was explicitly allowed to reach private or local addresses, when one was allowed.</param>
    /// <exception cref="PublicHostRejectedException">The address is neither public nor allowed.</exception>
    private static void AddEndPoint(List<IPEndPoint> endPoints, IPAddress address, int port, string host, string? allowedPrivateHost)
    {
        // Mapped addresses are stored and compared in their IPv4 form, so the same endpoint is not added twice under two spellings.
        if (address.IsIPv4MappedToIPv6)
            address = address.MapToIPv4();

        // The exception names the host but never the address, so an internal address does not reach the logs or the caller.
        if (!IsAddressAllowed(host, address, allowedPrivateHost))
            throw new PublicHostRejectedException(host, GetRejectionReason(host, address, allowedPrivateHost));

        // A host that resolves to the same address twice is only tried once.
        if (!endPoints.Any(endPoint => endPoint.Address.Equals(address)))
            endPoints.Add(new IPEndPoint(address, port));
    }

    /// <summary>
    /// Evaluates the provided address against the outbound address policy.
    /// </summary>
    /// <param name="host">The host the address was resolved from.</param>
    /// <param name="address">The address to evaluate.</param>
    /// <param name="allowedPrivateHost">The single host that was explicitly allowed to reach private or local addresses, when one was allowed.</param>
    /// <returns>The outcome of the evaluation, with the reason of a rejection when the address may not be used.</returns>
    private static PublicHostValidationResult EvaluateAddress(string host, IPAddress address, string? allowedPrivateHost)
    {
        if (IsAddressAllowed(host, address, allowedPrivateHost))
            return PublicHostValidationResult.Allowed;
        return PublicHostValidationResult.Rejected(GetRejectionReason(host, address, allowedPrivateHost));
    }

    /// <summary>
    /// Gets the reason the provided address was rejected, without exposing the address itself.
    /// </summary>
    /// <param name="host">The host the address was resolved from.</param>
    /// <param name="address">The rejected address.</param>
    /// <param name="allowedPrivateHost">The single host that was explicitly allowed to reach private or local addresses, when one was allowed.</param>
    /// <returns>The reason the address was rejected.</returns>
    private static OutboundRequestRejectionReason GetRejectionReason(string host, IPAddress address, string? allowedPrivateHost)
    {
        // A link local or metadata address gets its own reason, so logs can distinguish it from a generic private address refusal.
        if (allowedPrivateHost is not null && IsSameHost(host, allowedPrivateHost) && IsLinkLocalOrMetadataAddress(address))
            return OutboundRequestRejectionReason.LinkLocalOrMetadataAddress;
        return OutboundRequestRejectionReason.AddressNotAllowed;
    }

    /// <summary>
    /// Determines whether the provided address is a link local or a cloud metadata address, which stay rejected even when the host is explicitly allowed to be private.
    /// </summary>
    /// <param name="address">The address to check.</param>
    /// <returns><see langword="true"/> when the address is a link local or cloud metadata address, otherwise <see langword="false"/>.</returns>
    private static bool IsLinkLocalOrMetadataAddress(IPAddress address)
    {
        if (address.IsIPv4MappedToIPv6)
            address = address.MapToIPv4();

        if (address.AddressFamily == AddressFamily.InterNetwork)
        {
            byte[] octets = address.GetAddressBytes();
            // 169.254.0.0/16 covers the common cloud metadata endpoints, and 100.100.100.200 is the Alibaba Cloud metadata endpoint.
            return (octets[0] == 169 && octets[1] == 254) || (octets[0] == 100 && octets[1] == 100 && octets[2] == 100 && octets[3] == 200);
        }

        // The IPv6 link local range covers the common metadata endpoints, but the AWS endpoint lives in a unique local range.
        return address.IsIPv6LinkLocal || IsAwsMetadataAddress(address);
    }

    /// <summary>
    /// Determines whether the provided address is the IPv6 endpoint of the AWS instance metadata service, which lives in a unique local range and is therefore
    /// not covered by the link local check.
    /// </summary>
    /// <param name="address">The address to check.</param>
    /// <returns><see langword="true"/> when the address is the AWS metadata endpoint, otherwise <see langword="false"/>.</returns>
    private static bool IsAwsMetadataAddress(IPAddress address)
    {
        if (address.AddressFamily != AddressFamily.InterNetworkV6)
            return false;

        byte[] addressBytes = address.GetAddressBytes();
        // The AWS IMDSv6 endpoint is fd00:ec2::254, inside the fd00:ec2::/64 prefix.
        return addressBytes[0] == 0xFD && addressBytes[1] == 0x00 && addressBytes[2] == 0x0E && addressBytes[3] == 0xC2 &&
            addressBytes[4] == 0 && addressBytes[5] == 0 && addressBytes[6] == 0 && addressBytes[7] == 0;
    }

    /// <summary>
    /// Determines whether the provided address may be used by the host, either because it is public, or because it is private and the host is the one explicitly allowed to be private.
    /// </summary>
    /// <param name="host">The host the address was resolved from.</param>
    /// <param name="address">The address to check.</param>
    /// <param name="allowedPrivateHost">The single host that was explicitly allowed to reach private or local addresses, when one was allowed.</param>
    /// <returns><see langword="true"/> when the address may be used, otherwise <see langword="false"/>.</returns>
    private static bool IsAddressAllowed(string host, IPAddress address, string? allowedPrivateHost)
    {
        // A public address is always allowed.
        if (IsPublicAddress(address))
            return true;

        // A private address is allowed only for the single host the user opted into, never for another host a redirect might point at.
        return allowedPrivateHost is not null &&
            IsSameHost(host, allowedPrivateHost) &&
            IsAllowedPrivateAddress(address);
    }

    /// <summary>
    /// Determines whether the provided host is the host that was explicitly allowed to reach private or local addresses.
    /// </summary>
    /// <param name="host">The host to check.</param>
    /// <param name="allowedPrivateHost">The host that was explicitly allowed to reach private or local addresses.</param>
    /// <returns><see langword="true"/> when the two hosts name the same host, otherwise <see langword="false"/>.</returns>
    /// <remarks>
    /// The comparison converts internationalized names to their punycode spelling, so the Unicode spelling a URI keeps is matched against the punycode spelling the connection authority uses.
    /// </remarks>
    private static bool IsSameHost(string host, string allowedPrivateHost)
    {
        // Both sides go through the same normalization, so case, a trailing dot and the IDN spelling do not affect the comparison.
        return string.Equals(NormalizeHostForComparison(host), NormalizeHostForComparison(allowedPrivateHost), StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Normalizes the provided host for comparison, using the punycode spelling of internationalized names.
    /// </summary>
    /// <param name="host">The host to normalize.</param>
    /// <returns>The normalized host.</returns>
    private static string NormalizeHostForComparison(string host)
    {
        string normalizedHost = NormalizeHost(host);
        // An IP literal is compared in its canonical text form, which is how both address families are represented.
        if (IPAddress.TryParse(normalizedHost, out IPAddress? address))
            return address.ToString();
        try
        {
            // An internationalized name is compared in punycode, because that is the spelling the connection authority uses.
            return new IdnMapping().GetAscii(normalizedHost).ToLowerInvariant();
        }
        catch (ArgumentException)
        {
            // A malformed name cannot be converted. It is compared as lowercased text and will fail resolution later anyway.
            return normalizedHost.ToLowerInvariant();
        }
    }

    /// <summary>
    /// Determines whether the provided non public address is a private or local address that a user may explicitly allow, which excludes the link local and cloud
    /// metadata ranges even when such a permission was granted.
    /// </summary>
    /// <param name="address">The address to check.</param>
    /// <returns><see langword="true"/> when the address is a private address a user may allow, otherwise <see langword="false"/>.</returns>
    private static bool IsAllowedPrivateAddress(IPAddress address)
    {
        // This is the only place where a non public address becomes reachable. The accepted ranges are deliberately narrow.
        if (address.IsIPv4MappedToIPv6)
            address = address.MapToIPv4();

        if (address.AddressFamily == AddressFamily.InterNetwork)
        {
            byte[] octets = address.GetAddressBytes();
            return octets[0] switch
            {
                10 => true, // The 10.0.0.0/8 private range.
                100 when octets[1] is >= 64 and <= 127 => !(octets[1] == 100 && octets[2] == 100 && octets[3] == 200), // Carrier grade NAT, except the Alibaba Cloud metadata endpoint.
                127 => true, // The loopback range, for a self hosted instance on the same machine.
                172 when octets[1] is >= 16 and <= 31 => true, // The 172.16.0.0/12 private range.
                192 when octets[1] == 168 => true, // The 192.168.0.0/16 private range.
                _ => false
            };
        }

        // Only the IPv6 loopback and unique local ranges can be allowed, except the AWS metadata endpoint, which lives in a unique local range but is never allowed.
        return IPAddress.IsLoopback(address) || (address.IsIPv6UniqueLocal && !IsAwsMetadataAddress(address));
    }

    /// <summary>
    /// Determines whether the provided IPv4 address is public.
    /// </summary>
    /// <param name="address">The IPv4 address to check.</param>
    /// <returns><see langword="true"/> when the address is public, otherwise <see langword="false"/>.</returns>
    private static bool IsPublicIPv4Address(IPAddress address)
    {
        // Each listed case is a range that must never be reached. Everything not listed is public.
        byte[] octets = address.GetAddressBytes();
        return octets[0] switch
        {
            0 => false, // The 0.0.0.0/8 "this network" range.
            10 => false, // The 10.0.0.0/8 private range.
            100 when octets[1] is >= 64 and <= 127 => false, // The 100.64.0.0/10 carrier grade NAT range.
            127 => false, // The 127.0.0.0/8 loopback range.
            169 when octets[1] == 254 => false, // The 169.254.0.0/16 link local range, including the cloud metadata endpoints.
            172 when octets[1] is >= 16 and <= 31 => false, // The 172.16.0.0/12 private range.
            192 when octets[1] == 168 => false, // The 192.168.0.0/16 private range.
            192 when octets[1] == 0 && (octets[2] is 0 or 2) => false, // The IETF protocol assignments and TEST-NET-1 documentation ranges.
            198 when octets[1] is >= 18 and <= 19 => false, // The 198.18.0.0/15 benchmarking range.
            198 when octets[1] == 51 && octets[2] == 100 => false, // The TEST-NET-2 documentation range.
            203 when octets[1] == 0 && octets[2] == 113 => false, // The TEST-NET-3 documentation range.
            >= 224 => false, // The multicast range, the reserved range, and the broadcast address.
            _ => true
        };
    }

    /// <summary>
    /// Determines whether the provided IPv6 address is public.
    /// </summary>
    /// <param name="address">The IPv6 address to check.</param>
    /// <returns><see langword="true"/> when the address is public, otherwise <see langword="false"/>.</returns>
    private static bool IsPublicIPv6Address(IPAddress address)
    {
        // Link local, site local, multicast, Teredo and unique local addresses are all internal, deprecated, multicast or transition ranges.
        if (address.IsIPv6LinkLocal || address.IsIPv6SiteLocal || address.IsIPv6Multicast || address.IsIPv6Teredo || address.IsIPv6UniqueLocal)
            return false;

        byte[] addressBytes = address.GetAddressBytes();
        // The ::/96 IPv4 compatible and ::ffff:0:0/96 IPv4 translated ranges embed an IPv4 address, and the mapped form was already converted to IPv4, so no
        // address with an all zero first eighty bits is public.
        if (addressBytes[0] == 0 && addressBytes[1] == 0 && addressBytes[2] == 0 && addressBytes[3] == 0 &&
            addressBytes[4] == 0 && addressBytes[5] == 0 && addressBytes[6] == 0 && addressBytes[7] == 0 &&
            addressBytes[8] == 0 && addressBytes[9] == 0)
            return false;

        // NAT64, the discard only range, the documentation range and 6to4 are not public destinations either.
        return addressBytes[0] switch
        {
            0x00 when addressBytes[1] == 0x64 && addressBytes[2] == 0xFF && addressBytes[3] == 0x9B => false, // The 64:ff9b::/96 NAT64 prefix, which can map to private IPv4 addresses.
            0x01 when addressBytes[1] == 0x00 && addressBytes[2] == 0x00 && addressBytes[3] == 0x00 => false, // The 100::/64 discard only range.
            0x20 when addressBytes[1] == 0x01 && addressBytes[2] == 0x0D && addressBytes[3] == 0xB8 => false, // The 2001:db8::/32 documentation range.
            0x20 when addressBytes[1] == 0x02 => false, // The 2002::/16 6to4 range.
            _ => true
        };
    }

    /// <summary>
    /// A resolved set of addresses, together with the time after which it must be resolved again.
    /// </summary>
    /// <param name="Addresses">The addresses the host resolved to.</param>
    /// <param name="ExpiresOnUtc">The time after which the resolution must be repeated.</param>
    private sealed record CachedAddresses(IPAddress[] Addresses, DateTimeOffset ExpiresOnUtc);
}
