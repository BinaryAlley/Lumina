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
    [InlineData("coverartarchive.org")] // the Cover Art Archive host
    [InlineData("archive.org")] // the Internet Archive host
    [InlineData("ia800100.us.archive.org")] // a subdomain of the Internet Archive that serves its downloads
    [InlineData("media.coverartarchive.org")] // a subdomain of the Cover Art Archive
    public void IsAllowed_WhenTheHostIsATrustedArchiveHost_ShouldReturnTrue(string host)
    {
        // Act
        bool result = CoverArtArchiveHosts.IsAllowed(host);

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData("COVERARTARCHIVE.ORG")] // the Cover Art Archive host, upper cased
    [InlineData("Archive.Org")] // the Internet Archive host, mixed cased
    public void IsAllowed_WhenTheHostDiffersOnlyByCase_ShouldReturnTrue(string host)
    {
        // Act
        bool result = CoverArtArchiveHosts.IsAllowed(host);

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData("")] // an empty host
    [InlineData("   ")] // only white space
    [InlineData("localhost")] // the loopback host name
    [InlineData("127.0.0.1")] // the IPv4 loopback address
    [InlineData("169.254.169.254")] // the link local cloud metadata endpoint
    [InlineData("10.0.0.5")] // a private class A address
    [InlineData("192.168.1.10")] // a private class C address
    [InlineData("evil.example")] // an unrelated public host
    [InlineData("coverartarchive.org.evil.example")] // the trusted host used as a prefix of an attacker controlled host
    [InlineData("archive.org.evil.example")] // the Internet Archive host used as a prefix of an attacker controlled host
    [InlineData("notcoverartarchive.org")] // a suffix collision that is not a subdomain
    [InlineData("notarchive.org")] // a suffix collision that is not a subdomain
    public void IsAllowed_WhenTheHostIsNotATrustedArchiveHost_ShouldReturnFalse(string host)
    {
        // Act
        bool result = CoverArtArchiveHosts.IsAllowed(host);

        // Assert
        Assert.False(result);
    }
}
