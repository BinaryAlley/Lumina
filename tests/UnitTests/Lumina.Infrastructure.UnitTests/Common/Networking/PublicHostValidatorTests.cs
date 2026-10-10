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
    [InlineData("coverartarchive.org")] // a public host name
    [InlineData("example.com")] // a public host name
    [InlineData("8.8.8.8")] // a public IPv4 address
    [InlineData("1.1.1.1")] // a public IPv4 address
    [InlineData("172.15.0.1")] // the address just below the private class B range
    [InlineData("172.32.0.1")] // the address just above the private class B range
    [InlineData("2606:4700:4700::1111")] // a public IPv6 address
    public void IsPublicHost_WhenTheHostIsPublic_ShouldReturnTrue(string host)
    {
        // Act
        bool result = PublicHostValidator.IsPublicHost(host);

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData("")] // an empty host
    [InlineData("   ")] // only white space
    [InlineData("localhost")] // the loopback host name
    [InlineData("LOCALHOST")] // the loopback host name, upper cased
    [InlineData("127.0.0.1")] // the IPv4 loopback address
    [InlineData("::1")] // the IPv6 loopback address
    [InlineData("[::1]")] // the IPv6 loopback address, bracketed like a URI host
    [InlineData("0.0.0.0")] // the unspecified IPv4 address
    [InlineData("[::]")] // the unspecified IPv6 address
    [InlineData("169.254.169.254")] // the link local cloud metadata endpoint
    [InlineData("10.0.0.5")] // a private class A address
    [InlineData("172.16.0.1")] // the lower bound of the private class B range
    [InlineData("172.31.255.255")] // the upper bound of the private class B range
    [InlineData("192.168.1.10")] // a private class C address
    [InlineData("100.64.0.1")] // the lower bound of the carrier grade NAT range
    [InlineData("100.127.255.255")] // the upper bound of the carrier grade NAT range
    [InlineData("fc00::1")] // the IPv6 unique local range
    [InlineData("fe80::1")] // the IPv6 link local range
    public void IsPublicHost_WhenTheHostIsPrivateOrLocal_ShouldReturnFalse(string host)
    {
        // Act
        bool result = PublicHostValidator.IsPublicHost(host);

        // Assert
        Assert.False(result);
    }
}
