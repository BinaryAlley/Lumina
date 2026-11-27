#region ========================================================================= USING =====================================================================================
using Lumina.Plugins.CoverArtArchive.Core;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Plugins.CoverArtArchive.UnitTests.Core;

/// <summary>
/// Contains unit tests for the <see cref="CoverArtArchiveHosts"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class CoverArtArchiveHostsTests
{
    [Theory]
    [InlineData("coverartarchive.org")] // The Cover Art Archive host.
    [InlineData("archive.org")] // The Internet Archive host.
    [InlineData("ia800100.us.archive.org")] // A subdomain of the Internet Archive that serves its downloads.
    [InlineData("media.coverartarchive.org")] // A subdomain of the Cover Art Archive.
    public void IsAllowed_WhenTheHostIsATrustedArchiveHost_ShouldReturnTrue(string host)
    {
        // Act
        bool result = CoverArtArchiveHosts.IsAllowed(host);

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData("COVERARTARCHIVE.ORG")] // The Cover Art Archive host, upper cased.
    [InlineData("Archive.Org")] // The Internet Archive host, mixed cased.
    public void IsAllowed_WhenTheHostDiffersOnlyByCase_ShouldReturnTrue(string host)
    {
        // Act
        bool result = CoverArtArchiveHosts.IsAllowed(host);

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData("")] // An empty host.
    [InlineData("   ")] // Only white space.
    [InlineData("localhost")] // The loopback host name.
    [InlineData("127.0.0.1")] // The IPv4 loopback address.
    [InlineData("169.254.169.254")] // The link local cloud metadata endpoint.
    [InlineData("10.0.0.5")] // A private class A address.
    [InlineData("192.168.1.10")] // A private class C address.
    [InlineData("evil.example")] // An unrelated public host.
    [InlineData("coverartarchive.org.evil.example")] // The trusted host used as a prefix of an attacker controlled host.
    [InlineData("archive.org.evil.example")] // The Internet Archive host used as a prefix of an attacker controlled host.
    [InlineData("notcoverartarchive.org")] // A suffix collision that is not a subdomain.
    [InlineData("notarchive.org")] // A suffix collision that is not a subdomain.
    public void IsAllowed_WhenTheHostIsNotATrustedArchiveHost_ShouldReturnFalse(string host)
    {
        // Act
        bool result = CoverArtArchiveHosts.IsAllowed(host);

        // Assert
        Assert.False(result);
    }
}
