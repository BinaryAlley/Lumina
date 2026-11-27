#region ========================================================================= USING =====================================================================================
using Nemera.Domain.SharedKernel.Common.Enums.Networking;
using Nemera.Infrastructure.Common.Networking;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Nemera.Infrastructure.UnitTests.Common.Networking;

/// <summary>
/// Contains unit tests for the <see cref="PublicHostValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class PublicHostValidatorTests
{
    private static readonly IPAddress s_publicAddress = IPAddress.Parse("93.184.216.34");

    [Theory]
    [InlineData("8.8.8.8")] // A public IPv4 address.
    [InlineData("1.1.1.1")] // A public IPv4 address.
    [InlineData("172.15.0.1")] // The address just below the private class B range.
    [InlineData("172.32.0.1")] // The address just above the private class B range.
    [InlineData("100.63.255.255")] // The address just below the carrier grade NAT range.
    [InlineData("100.128.0.1")] // The address just above the carrier grade NAT range.
    [InlineData("2606:4700:4700::1111")] // A public IPv6 address.
    [InlineData("[2606:4700:4700::1111]")] // A public IPv6 address, bracketed like a URI host.
    [InlineData("::ffff:93.184.216.34")] // An IPv4-mapped public IPv4 address.
    public async Task ValidateHostAsync_WhenTheHostIsAPublicAddress_ShouldReturnTrue(string host)
    {
        // Arrange
        PublicHostValidator sut = CreateValidator(_ => throw new InvalidOperationException("An IP literal must not be resolved."));

        // Act
        PublicHostValidationResult result = await sut.ValidateHostAsync(host, allowedPrivateHost: null, CancellationToken.None);

        // Assert
        Assert.True(result.IsAllowed);
    }

    [Theory]
    [InlineData("")] // An empty host.
    [InlineData("   ")] // Only white space.
    [InlineData("localhost")] // The loopback host name.
    [InlineData("LOCALHOST")] // The loopback host name, upper cased.
    [InlineData("127.0.0.1")] // The IPv4 loopback address.
    [InlineData("::1")] // The IPv6 loopback address.
    [InlineData("[::1]")] // The IPv6 loopback address, bracketed like a URI host.
    [InlineData("0.0.0.0")] // The unspecified IPv4 address.
    [InlineData("[::]")] // The unspecified IPv6 address.
    [InlineData("169.254.169.254")] // The link local cloud metadata endpoint.
    [InlineData("10.0.0.5")] // A private class A address.
    [InlineData("::ffff:10.0.0.5")] // A private class A address, in its IPv4-mapped IPv6 form.
    [InlineData("172.16.0.1")] // The lower bound of the private class B range.
    [InlineData("172.31.255.255")] // The upper bound of the private class B range.
    [InlineData("192.168.1.10")] // A private class C address.
    [InlineData("100.64.0.1")] // The lower bound of the carrier grade NAT range.
    [InlineData("100.127.255.255")] // The upper bound of the carrier grade NAT range.
    [InlineData("192.0.2.10")] // The TEST-NET-1 documentation range.
    [InlineData("198.18.0.1")] // The benchmarking range.
    [InlineData("198.51.100.10")] // The TEST-NET-2 documentation range.
    [InlineData("203.0.113.10")] // The TEST-NET-3 documentation range.
    [InlineData("224.0.0.1")] // The multicast range.
    [InlineData("240.0.0.1")] // The reserved range.
    [InlineData("255.255.255.255")] // The broadcast address.
    [InlineData("fc00::1")] // The IPv6 unique local range.
    [InlineData("fd00:ec2::254")] // The AWS IMDSv6 endpoint, which lives in a unique local range.
    [InlineData("fe80::1")] // The IPv6 link local range.
    [InlineData("2001:db8::1")] // The IPv6 documentation range.
    [InlineData("::127.0.0.1")] // An IPv4 compatible IPv6 address.
    [InlineData("64:ff9b:1::1")] // The local use NAT64 prefix.
    [InlineData("2002::1")] // The IPv6 6to4 range.
    [InlineData("64:ff9b::1")] // The IPv6 NAT64 prefix.
    [InlineData("100::1")] // The IPv6 discard only range.
    [InlineData("ff02::1")] // The IPv6 multicast range.
    public async Task ValidateHostAsync_WhenTheHostIsPrivateOrLocal_ShouldReturnFalse(string host)
    {
        // Arrange
        PublicHostValidator sut = CreateValidator(_ => throw new InvalidOperationException("An IP literal must not be resolved."));

        // Act
        PublicHostValidationResult result = await sut.ValidateHostAsync(host, allowedPrivateHost: null, CancellationToken.None);

        // Assert
        Assert.False(result.IsAllowed);
    }

    [Fact]
    public async Task ValidateHostAsync_WhenTheAllowedPrivateHostIsAnIPv4MappedPrivateAddress_ShouldReturnTrue()
    {
        // Arrange
        PublicHostValidator sut = CreateValidator(_ => throw new InvalidOperationException("An IP literal must not be resolved."));

        // Act
        PublicHostValidationResult result = await sut.ValidateHostAsync("::ffff:10.0.0.5", allowedPrivateHost: "::ffff:10.0.0.5", CancellationToken.None);

        // Assert
        Assert.True(result.IsAllowed);
    }

    [Fact]
    public async Task ValidateHostAsync_WhenTheHostNameResolvesToPublicAddresses_ShouldReturnTrue()
    {
        // Arrange
        PublicHostValidator sut = CreateValidator(host => [s_publicAddress, IPAddress.Parse("93.184.216.35")]);

        // Act
        PublicHostValidationResult result = await sut.ValidateHostAsync("example.com", allowedPrivateHost: null, CancellationToken.None);

        // Assert
        Assert.True(result.IsAllowed);
    }

    [Fact]
    public async Task ValidateHostAsync_WhenTheHostNameResolvesToOnePrivateAddress_ShouldReturnFalse()
    {
        // Arrange
        PublicHostValidator sut = CreateValidator(host => [IPAddress.Parse("10.0.0.5")]);

        // Act
        PublicHostValidationResult result = await sut.ValidateHostAsync("internal.example.com", allowedPrivateHost: null, CancellationToken.None);

        // Assert
        Assert.False(result.IsAllowed);
    }

    [Fact]
    public async Task ValidateHostAsync_WhenTheHostNameResolvesToBothPublicAndPrivateAddresses_ShouldReturnFalse()
    {
        // Arrange
        PublicHostValidator sut = CreateValidator(host => [s_publicAddress, IPAddress.Parse("192.168.1.10")]);

        // Act
        PublicHostValidationResult result = await sut.ValidateHostAsync("mixed.example.com", allowedPrivateHost: null, CancellationToken.None);

        // Assert
        Assert.False(result.IsAllowed);
    }

    [Fact]
    public async Task ValidateHostAsync_WhenTheHostNameCannotBeResolved_ShouldReturnFalse()
    {
        // Arrange
        PublicHostValidator sut = CreateValidator(_ => throw new SocketException((int)SocketError.HostNotFound));

        // Act
        PublicHostValidationResult result = await sut.ValidateHostAsync("missing.example.com", allowedPrivateHost: null, CancellationToken.None);

        // Assert
        Assert.False(result.IsAllowed);
    }

    [Fact]
    public async Task ValidateHostAsync_WhenTheHostNameIsMalformed_ShouldReturnFalse()
    {
        // Arrange
        PublicHostValidator sut = CreateValidator(_ => throw new ArgumentException("The host is malformed."));

        // Act
        PublicHostValidationResult result = await sut.ValidateHostAsync("malformed host", allowedPrivateHost: null, CancellationToken.None);

        // Assert
        Assert.False(result.IsAllowed);
    }

    [Fact]
    public async Task ValidateHostAsync_WhenTheHostNameResolvesToNoAddress_ShouldReturnFalse()
    {
        // Arrange
        PublicHostValidator sut = CreateValidator(_ => []);

        // Act
        PublicHostValidationResult result = await sut.ValidateHostAsync("empty.example.com", allowedPrivateHost: null, CancellationToken.None);

        // Assert
        Assert.False(result.IsAllowed);
    }

    [Fact]
    public async Task ValidateHostAsync_WhenCancellationIsRequested_ShouldThrowOperationCanceledException()
    {
        // Arrange
        PublicHostValidator sut = CreateValidator(_ => throw new OperationCanceledException());
        using CancellationTokenSource cancellationTokenSource = new();
        cancellationTokenSource.Cancel();

        // Act
        Task Act()
        {
            return sut.ValidateHostAsync("example.com", allowedPrivateHost: null, cancellationTokenSource.Token);
        }

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(Act);
    }

    [Fact]
    public async Task ResolveEndPointsAsync_WhenThePublicHostNameResolvesToAddresses_ShouldReturnTheirEndPoints()
    {
        // Arrange
        PublicHostValidator sut = CreateValidator(host => [s_publicAddress, s_publicAddress]);
        IPAddress expectedAddress = s_publicAddress;

        // Act
        IReadOnlyList<IPEndPoint> result = await sut.ResolveEndPointsAsync("example.com", 443, allowedPrivateHost: null, CancellationToken.None);

        // Assert
        IPEndPoint endPoint = Assert.Single(result);
        Assert.Equal(expectedAddress, endPoint.Address);
        Assert.Equal(443, endPoint.Port);
    }

    [Fact]
    public async Task ResolveEndPointsAsync_WhenTheHostNameResolvesToOnePrivateAddress_ShouldThrow()
    {
        // Arrange
        PublicHostValidator sut = CreateValidator(host => [s_publicAddress, IPAddress.Parse("10.0.0.5")]);

        // Act
        Task Act()
        {
            return sut.ResolveEndPointsAsync("mixed.example.com", 80, allowedPrivateHost: null, CancellationToken.None);
        }

        // Assert
        await Assert.ThrowsAsync<PublicHostRejectedException>(Act);
    }

    [Fact]
    public async Task ResolveEndPointsAsync_WhenTheHostIsAnIPv4MappedIPv6Address_ShouldReturnItsIPv4EndPoint()
    {
        // Arrange
        PublicHostValidator sut = CreateValidator(_ => throw new InvalidOperationException("An IP literal must not be resolved."));
        IPAddress expectedAddress = IPAddress.Parse("93.184.216.34");

        // Act
        IReadOnlyList<IPEndPoint> result = await sut.ResolveEndPointsAsync("::ffff:93.184.216.34", 443, allowedPrivateHost: null, CancellationToken.None);

        // Assert
        IPEndPoint endPoint = Assert.Single(result);
        Assert.Equal(expectedAddress, endPoint.Address);
        Assert.Equal(443, endPoint.Port);
    }

    [Fact]
    public async Task ResolveEndPointsAsync_WhenTheHostCannotBeResolved_ShouldThrow()
    {
        // Arrange
        PublicHostValidator sut = CreateValidator(_ => throw new SocketException((int)SocketError.HostNotFound));

        // Act
        Task Act()
        {
            return sut.ResolveEndPointsAsync("missing.example.com", 443, allowedPrivateHost: null, CancellationToken.None);
        }

        // Assert
        await Assert.ThrowsAsync<PublicHostResolutionException>(Act);
    }

    [Fact]
    public async Task ResolveEndPointsAsync_WhenTheHostNameResolvesToNoAddress_ShouldThrow()
    {
        // Arrange
        PublicHostValidator sut = CreateValidator(_ => []);

        // Act
        Task Act()
        {
            return sut.ResolveEndPointsAsync("empty.example.com", 443, allowedPrivateHost: null, CancellationToken.None);
        }

        // Assert
        await Assert.ThrowsAsync<PublicHostResolutionException>(Act);
    }

    [Fact]
    public async Task ValidateHostAsync_WhenTheAllowedPrivateHostResolvesToAPrivateAddress_ShouldReturnTrue()
    {
        // Arrange
        PublicHostValidator sut = CreateValidator(host => [IPAddress.Parse("10.0.0.5")]);

        // Act
        PublicHostValidationResult result = await sut.ValidateHostAsync("internal.example.com", allowedPrivateHost: "internal.example.com", CancellationToken.None);

        // Assert
        Assert.True(result.IsAllowed);
    }

    [Fact]
    public async Task ValidateHostAsync_WhenAnotherHostResolvesToOnePrivateAddress_ShouldReturnFalse()
    {
        // Arrange
        PublicHostValidator sut = CreateValidator(host => [IPAddress.Parse("10.0.0.5")]);

        // Act
        PublicHostValidationResult result = await sut.ValidateHostAsync("other.example.com", allowedPrivateHost: "internal.example.com", CancellationToken.None);

        // Assert
        Assert.False(result.IsAllowed);
    }

    [Fact]
    public async Task ValidateHostAsync_WhenTheAllowedPrivateHostIsLocalhost_ShouldReturnTrue()
    {
        // Arrange
        PublicHostValidator sut = CreateValidator(_ => throw new InvalidOperationException("The loopback host name must not be resolved."));

        // Act
        PublicHostValidationResult result = await sut.ValidateHostAsync("localhost", allowedPrivateHost: "localhost", CancellationToken.None);

        // Assert
        Assert.True(result.IsAllowed);
    }

    [Fact]
    public async Task ValidateHostAsync_WhenTheAllowedPrivateHostIsAnIdnNameAndTheConnectionUsesItsPunycodeSpelling_ShouldReturnTrue()
    {
        // Arrange
        PublicHostValidator sut = CreateValidator(host => [IPAddress.Parse("192.168.1.10")]);

        // Act
        PublicHostValidationResult result = await sut.ValidateHostAsync("xn--bcher-kva.example", allowedPrivateHost: "bücher.example", CancellationToken.None);

        // Assert
        Assert.True(result.IsAllowed);
    }

    [Fact]
    public async Task ValidateHostAsync_WhenTheAllowedPrivateHostResolvesToTheLinkLocalMetadataAddress_ShouldReturnFalse()
    {
        // Arrange
        PublicHostValidator sut = CreateValidator(host => [IPAddress.Parse("169.254.169.254")]);

        // Act
        PublicHostValidationResult result = await sut.ValidateHostAsync("internal.example.com", allowedPrivateHost: "internal.example.com", CancellationToken.None);

        // Assert
        Assert.False(result.IsAllowed);
    }

    [Fact]
    public async Task ValidateHostAsync_WhenTheAllowedPrivateHostResolvesToAnIPv6UniqueLocalAddress_ShouldReturnTrue()
    {
        // Arrange
        PublicHostValidator sut = CreateValidator(host => [IPAddress.Parse("fc00::1")]);

        // Act
        PublicHostValidationResult result = await sut.ValidateHostAsync("internal.example.com", allowedPrivateHost: "internal.example.com", CancellationToken.None);

        // Assert
        Assert.True(result.IsAllowed);
    }

    [Fact]
    public async Task ValidateHostAsync_WhenTheAllowedPrivateHostResolvesToTheAwsMetadataAddress_ShouldReturnFalse()
    {
        // Arrange
        PublicHostValidator sut = CreateValidator(host => [IPAddress.Parse("fd00:ec2::254")]);

        // Act
        PublicHostValidationResult result = await sut.ValidateHostAsync("internal.example.com", allowedPrivateHost: "internal.example.com", CancellationToken.None);

        // Assert
        Assert.False(result.IsAllowed);
        Assert.Equal(OutboundRequestRejectionReason.LinkLocalOrMetadataAddress, result.RejectionReason);
    }

    [Fact]
    public async Task ValidateHostAsync_WhenTheAllowedPrivateHostResolvesToTheAlibabaMetadataAddress_ShouldReturnFalse()
    {
        // Arrange
        PublicHostValidator sut = CreateValidator(host => [IPAddress.Parse("100.100.100.200")]);

        // Act
        PublicHostValidationResult result = await sut.ValidateHostAsync("internal.example.com", allowedPrivateHost: "internal.example.com", CancellationToken.None);

        // Assert
        Assert.False(result.IsAllowed);
    }

    [Fact]
    public async Task ResolveEndPointsAsync_WhenTheAllowedPrivateHostResolvesToAPrivateAddress_ShouldReturnItsEndPoint()
    {
        // Arrange
        PublicHostValidator sut = CreateValidator(host => [IPAddress.Parse("10.0.0.5")]);
        IPAddress expectedAddress = IPAddress.Parse("10.0.0.5");

        // Act
        IReadOnlyList<IPEndPoint> result = await sut.ResolveEndPointsAsync("internal.example.com", 8080, allowedPrivateHost: "internal.example.com", CancellationToken.None);

        // Assert
        IPEndPoint endPoint = Assert.Single(result);
        Assert.Equal(expectedAddress, endPoint.Address);
        Assert.Equal(8080, endPoint.Port);
    }

    [Fact]
    public async Task ResolveEndPointsAsync_WhenAnotherHostResolvesToAPrivateAddress_ShouldThrow()
    {
        // Arrange
        PublicHostValidator sut = CreateValidator(host => [IPAddress.Parse("10.0.0.5")]);

        // Act
        Task Act()
        {
            return sut.ResolveEndPointsAsync("other.example.com", 443, allowedPrivateHost: "internal.example.com", CancellationToken.None);
        }

        // Assert
        await Assert.ThrowsAsync<PublicHostRejectedException>(Act);
    }

    [Fact]
    public async Task ResolveEndPointsAsync_WhenTheAllowedPrivateHostResolvesToTheLinkLocalMetadataAddress_ShouldThrow()
    {
        // Arrange
        PublicHostValidator sut = CreateValidator(host => [IPAddress.Parse("169.254.169.254")]);

        // Act
        Task Act()
        {
            return sut.ResolveEndPointsAsync("internal.example.com", 443, allowedPrivateHost: "internal.example.com", CancellationToken.None);
        }

        // Assert
        PublicHostRejectedException exception = await Assert.ThrowsAsync<PublicHostRejectedException>(Act);
        Assert.Equal(OutboundRequestRejectionReason.LinkLocalOrMetadataAddress, exception.RejectionReason);
    }

    [Fact]
    public async Task ValidateHostAsync_WhenTheSameHostIsCheckedTwice_ShouldResolveItOnlyOnce()
    {
        // Arrange
        int resolutionCount = 0;
        PublicHostValidator sut = new((host, cancellationToken) =>
        {
            resolutionCount++;
            return Task.FromResult<IPAddress[]>([s_publicAddress]);
        });

        // Act
        await sut.ValidateHostAsync("example.com", allowedPrivateHost: null, CancellationToken.None);
        await sut.ValidateHostAsync("example.com", allowedPrivateHost: null, CancellationToken.None);

        // Assert
        Assert.Equal(1, resolutionCount);
    }

    [Fact]
    public async Task ResolveEndPointsAsync_WhenTheHostWasAlreadyChecked_ShouldReuseTheResolution()
    {
        // Arrange
        int resolutionCount = 0;
        PublicHostValidator sut = new((host, cancellationToken) =>
        {
            resolutionCount++;
            return Task.FromResult<IPAddress[]>([s_publicAddress]);
        });

        // Act
        await sut.ValidateHostAsync("example.com", allowedPrivateHost: null, CancellationToken.None);
        await sut.ResolveEndPointsAsync("example.com", 443, allowedPrivateHost: null, CancellationToken.None);

        // Assert
        Assert.Equal(1, resolutionCount);
    }

    [Fact]
    public async Task ValidateHostAsync_WhenTheHostCannotBeResolvedTwice_ShouldCacheTheFailureBriefly()
    {
        // Arrange
        int resolutionCount = 0;
        PublicHostValidator sut = new((host, cancellationToken) =>
        {
            resolutionCount++;
            throw new SocketException((int)SocketError.HostNotFound);
        });

        // Act
        await sut.ValidateHostAsync("missing.example.com", allowedPrivateHost: null, CancellationToken.None);
        await sut.ValidateHostAsync("missing.example.com", allowedPrivateHost: null, CancellationToken.None);

        // Assert
        Assert.Equal(1, resolutionCount);
    }

    /// <summary>
    /// Creates a <see cref="PublicHostValidator"/> whose host name resolution is replaced by the provided resolver.
    /// </summary>
    /// <param name="addressResolver">The resolver that produces the addresses of a host name.</param>
    /// <returns>The created validator.</returns>
    private static PublicHostValidator CreateValidator(Func<string, IPAddress[]> addressResolver)
    {
        return new PublicHostValidator((host, cancellationToken) => Task.FromResult(addressResolver(host)));
    }
}
