#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.Common;
using Lumina.Plugins.CoverArtArchive.Common.Models.Contracts.Responses;
using Lumina.Plugins.CoverArtArchive.Core.Mapping;
using Lumina.Plugins.CoverArtArchive.Fixtures.Common.Models.Contracts.Responses;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Plugins.CoverArtArchive.SecurityTests.Core.Mapping;

/// <summary>
/// Contains security tests for the <see cref="CoverArtArchiveMapper"/> class, exercised through the artwork it emits.
/// </summary>
[ExcludeFromCodeCoverage]
public class CoverArtArchiveMapperSecurityTests
{
    private readonly CoverArtArchiveArtworkResponseFixture _coverArtArchiveArtworkResponseFixture = new();
    private readonly CoverArtArchiveImageResponseFixture _coverArtArchiveImageResponseFixture = new();

    [Theory]
    [InlineData("http://169.254.169.254/latest/meta-data/")] // the link local cloud metadata endpoint
    [InlineData("http://localhost/admin")] // the loopback host name
    [InlineData("http://127.0.0.1/admin")] // the IPv4 loopback address
    [InlineData("http://[::1]/admin")] // the IPv6 loopback address
    [InlineData("http://10.0.0.5/internal")] // a private class A address
    [InlineData("http://172.16.0.1/internal")] // a private class B address
    [InlineData("http://192.168.1.10/internal")] // a private class C address
    [InlineData("http://0.0.0.0/internal")] // the unspecified address
    [InlineData("http://[fc00::1]/internal")] // the IPv6 unique local range
    [InlineData("https://evil.example/tracker.jpg")] // an unrelated public host
    [InlineData("https://coverartarchive.org.evil.example/1.jpg")] // the trusted host used as a prefix of an attacker controlled host
    [InlineData("https://archive.org.evil.example/1.jpg")] // the Internet Archive host used as a prefix of an attacker controlled host
    public void MapArtwork_WhenAnImageUrlPointsAtAnInternalOrUntrustedHost_ShouldDropIt(string maliciousUrl)
    {
        // Arrange
        CoverArtArchiveImageResponse image = _coverArtArchiveImageResponseFixture.Create(image: maliciousUrl, types: ["Front"]);
        CoverArtArchiveArtworkResponse response = _coverArtArchiveArtworkResponseFixture.Create(images: [image]);

        // Act
        IReadOnlyList<ArtworkDto> result = CoverArtArchiveMapper.MapArtwork(response);

        // Assert
        Assert.Empty(result);
    }

    [Theory]
    [InlineData("file:///etc/passwd")] // a local file URI
    [InlineData("ftp://coverartarchive.org/1.jpg")] // a non web scheme
    [InlineData("javascript:alert(1)")] // a script scheme
    [InlineData("data:image/png;base64,SGVsbG8=")] // an inline data URI
    public void MapArtwork_WhenAnImageUrlUsesAnUnsupportedScheme_ShouldDropIt(string maliciousUrl)
    {
        // Arrange
        CoverArtArchiveImageResponse image = _coverArtArchiveImageResponseFixture.Create(image: maliciousUrl, types: ["Front"]);
        CoverArtArchiveArtworkResponse response = _coverArtArchiveArtworkResponseFixture.Create(images: [image]);

        // Act
        IReadOnlyList<ArtworkDto> result = CoverArtArchiveMapper.MapArtwork(response);

        // Assert
        Assert.Empty(result);
    }

    [Theory]
    [InlineData("https://coverartarchive.org/release/11111111-1111-1111-1111-111111111111/front.jpg")] // a trusted Cover Art Archive image
    [InlineData("https://archive.org/download/mbid-11111111-1111-1111-1111-111111111111/front.jpg")] // a trusted Internet Archive image
    public void MapArtwork_WhenAnImageUrlPointsAtATrustedHostOverHttps_ShouldKeepIt(string trustedUrl)
    {
        // Arrange
        CoverArtArchiveImageResponse image = _coverArtArchiveImageResponseFixture.Create(image: trustedUrl, types: ["Front"]);
        CoverArtArchiveArtworkResponse response = _coverArtArchiveArtworkResponseFixture.Create(images: [image]);

        // Act
        IReadOnlyList<ArtworkDto> result = CoverArtArchiveMapper.MapArtwork(response);

        // Assert
        ArtworkDto artwork = Assert.Single(result);
        Assert.Equal(trustedUrl, artwork.RemoteUrl);
    }

    [Fact]
    public void MapArtwork_WhenAnImageUrlPointsAtATrustedHostOverPlainHttp_ShouldUpgradeItToHttps()
    {
        // Arrange
        const string HTTP_URL = "http://coverartarchive.org/release/11111111-1111-1111-1111-111111111111/front.jpg";
        CoverArtArchiveImageResponse image = _coverArtArchiveImageResponseFixture.Create(image: HTTP_URL, types: ["Front"]);
        CoverArtArchiveArtworkResponse response = _coverArtArchiveArtworkResponseFixture.Create(images: [image]);

        // Act
        IReadOnlyList<ArtworkDto> result = CoverArtArchiveMapper.MapArtwork(response);

        // Assert
        ArtworkDto artwork = Assert.Single(result);
        Assert.Equal("https://coverartarchive.org/release/11111111-1111-1111-1111-111111111111/front.jpg", artwork.RemoteUrl);
    }
}
