#region ========================================================================= USING =====================================================================================
using Lumina.Infrastructure.Common.Networking;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Infrastructure.UnitTests.Common.Networking;

/// <summary>
/// Contains unit tests for the <see cref="PublicHostValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class PublicHostValidatorTests
{
    [Theory]
    [InlineData("coverartarchive.org")] // A public host name.
    [InlineData("example.com")] // A public host name.
    [InlineData("8.8.8.8")] // A public IPv4 address.
    [InlineData("1.1.1.1")] // A public IPv4 address.
    [InlineData("172.15.0.1")] // The address just below the private class B range.
    [InlineData("172.32.0.1")] // The address just above the private class B range.
    [InlineData("2606:4700:4700::1111")] // A public IPv6 address.
    public void IsPublicHost_WhenTheHostIsPublic_ShouldReturnTrue(string host)
    {
        // Act
        bool result = PublicHostValidator.IsPublicHost(host);

        // Assert
        Assert.True(result);
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
    [InlineData("172.16.0.1")] // The lower bound of the private class B range.
    [InlineData("172.31.255.255")] // The upper bound of the private class B range.
    [InlineData("192.168.1.10")] // A private class C address.
    [InlineData("100.64.0.1")] // The lower bound of the carrier grade NAT range.
    [InlineData("100.127.255.255")] // The upper bound of the carrier grade NAT range.
    [InlineData("fc00::1")] // The IPv6 unique local range.
    [InlineData("fe80::1")] // The IPv6 link local range.
    public void IsPublicHost_WhenTheHostIsPrivateOrLocal_ShouldReturnFalse(string host)
    {
        // Act
        bool result = PublicHostValidator.IsPublicHost(host);

        // Assert
        Assert.False(result);
    }
}
