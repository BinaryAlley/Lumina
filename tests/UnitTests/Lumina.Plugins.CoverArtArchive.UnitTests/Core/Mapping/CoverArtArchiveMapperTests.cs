#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.Common;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Plugins.CoverArtArchive.Common.Models.Contracts.Responses;
using Lumina.Plugins.CoverArtArchive.Core.Mapping;
using Lumina.Plugins.CoverArtArchive.Fixtures.Common.Models.Contracts.Responses;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Plugins.CoverArtArchive.UnitTests.Core.Mapping;

/// <summary>
/// Contains unit tests for the <see cref="CoverArtArchiveMapper"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class CoverArtArchiveMapperTests
{
    private const string COVER_URL = "https://coverartarchive.org/release/11111111-1111-1111-1111-111111111111/1.jpg";

    private readonly CoverArtArchiveArtworkResponseFixture _coverArtArchiveArtworkResponseFixture = new();
    private readonly CoverArtArchiveImageResponseFixture _coverArtArchiveImageResponseFixture = new();

    [Fact]
    public void MapArtwork_WhenTheResponseIsNull_ShouldReturnEmpty()
    {
        // Act
        IReadOnlyList<ArtworkDto> result = CoverArtArchiveMapper.MapArtwork(null);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void MapArtwork_WhenTheImagesCollectionIsNull_ShouldReturnEmpty()
    {
        // Arrange
        CoverArtArchiveArtworkResponse response = _coverArtArchiveArtworkResponseFixture.Create(includeImages: false);

        // Act
        IReadOnlyList<ArtworkDto> result = CoverArtArchiveMapper.MapArtwork(response);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void MapArtwork_WhenThereAreNoImages_ShouldReturnEmpty()
    {
        // Arrange
        CoverArtArchiveArtworkResponse response = _coverArtArchiveArtworkResponseFixture.Create(images: []);

        // Act
        IReadOnlyList<ArtworkDto> result = CoverArtArchiveMapper.MapArtwork(response);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void MapArtwork_WhenAnImageElementIsNull_ShouldSkipIt()
    {
        // Arrange
        CoverArtArchiveImageResponse validImage = _coverArtArchiveImageResponseFixture.Create(image: COVER_URL, types: ["Front"]);
        CoverArtArchiveArtworkResponse response = _coverArtArchiveArtworkResponseFixture.Create(images: [null, validImage]);

        // Act
        IReadOnlyList<ArtworkDto> result = CoverArtArchiveMapper.MapArtwork(response);

        // Assert
        ArtworkDto artwork = Assert.Single(result);
        Assert.Equal(COVER_URL, artwork.RemoteUrl);
    }

    [Theory]
    [InlineData(null)] // a null URL
    [InlineData("")] // an empty URL
    [InlineData("   ")] // a white space URL
    public void MapArtwork_WhenTheImageUrlIsMissing_ShouldSkipIt(string? imageUrl)
    {
        // Arrange
        CoverArtArchiveImageResponse image = _coverArtArchiveImageResponseFixture.Create(image: imageUrl, includeImage: imageUrl is not null, types: ["Front"]);
        CoverArtArchiveArtworkResponse response = _coverArtArchiveArtworkResponseFixture.Create(images: [image]);

        // Act
        IReadOnlyList<ArtworkDto> result = CoverArtArchiveMapper.MapArtwork(response);

        // Assert
        Assert.Empty(result);
    }

    [Theory]
    [InlineData("not-a-url")] // a value that is not an URI at all
    [InlineData("/release/11111111-1111-1111-1111-111111111111/1.jpg")] // a relative URI
    [InlineData("coverartarchive.org/release/1.jpg")] // a host without a scheme
    public void MapArtwork_WhenTheImageUrlIsNotAnAbsoluteUri_ShouldSkipIt(string imageUrl)
    {
        // Arrange
        CoverArtArchiveImageResponse image = _coverArtArchiveImageResponseFixture.Create(image: imageUrl, types: ["Front"]);
        CoverArtArchiveArtworkResponse response = _coverArtArchiveArtworkResponseFixture.Create(images: [image]);

        // Act
        IReadOnlyList<ArtworkDto> result = CoverArtArchiveMapper.MapArtwork(response);

        // Assert
        Assert.Empty(result);
    }

    [Theory]
    [InlineData("http://localhost/release/1.jpg")] // the loopback host name
    [InlineData("http://127.0.0.1/release/1.jpg")] // the IPv4 loopback address
    [InlineData("http://169.254.169.254/latest/meta-data/")] // the link local cloud metadata endpoint
    [InlineData("http://10.0.0.5/release/1.jpg")] // a private class A address
    [InlineData("http://192.168.1.10/release/1.jpg")] // a private class C address
    [InlineData("https://evil.example/release/1.jpg")] // an unrelated public host
    [InlineData("https://coverartarchive.org.evil.example/1.jpg")] // the trusted host used as a prefix of an attacker controlled host
    [InlineData("https://archive.org.evil.example/1.jpg")] // the Internet Archive host used as a prefix of an attacker controlled host
    public void MapArtwork_WhenTheImageUrlPointsAtAnUntrustedHost_ShouldSkipIt(string imageUrl)
    {
        // Arrange
        CoverArtArchiveImageResponse image = _coverArtArchiveImageResponseFixture.Create(image: imageUrl, types: ["Front"]);
        CoverArtArchiveArtworkResponse response = _coverArtArchiveArtworkResponseFixture.Create(images: [image]);

        // Act
        IReadOnlyList<ArtworkDto> result = CoverArtArchiveMapper.MapArtwork(response);

        // Assert
        Assert.Empty(result);
    }

    [Theory]
    [InlineData("file:///etc/passwd")] // a local file URI
    [InlineData("ftp://coverartarchive.org/1.jpg")] // an unsupported scheme on a trusted host
    [InlineData("data:image/png;base64,SGVsbG8=")] // an inline data URI
    public void MapArtwork_WhenTheImageUrlUsesAnUnsupportedScheme_ShouldSkipIt(string imageUrl)
    {
        // Arrange
        CoverArtArchiveImageResponse image = _coverArtArchiveImageResponseFixture.Create(image: imageUrl, types: ["Front"]);
        CoverArtArchiveArtworkResponse response = _coverArtArchiveArtworkResponseFixture.Create(images: [image]);

        // Act
        IReadOnlyList<ArtworkDto> result = CoverArtArchiveMapper.MapArtwork(response);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void MapArtwork_WhenTheImageUrlUsesPlainHttpOnATrustedHost_ShouldUpgradeItToHttps()
    {
        // Arrange
        const string HTTP_URL = "http://coverartarchive.org/release/11111111-1111-1111-1111-111111111111/1.jpg";
        CoverArtArchiveImageResponse image = _coverArtArchiveImageResponseFixture.Create(image: HTTP_URL, types: ["Front"]);
        CoverArtArchiveArtworkResponse response = _coverArtArchiveArtworkResponseFixture.Create(images: [image]);

        // Act
        IReadOnlyList<ArtworkDto> result = CoverArtArchiveMapper.MapArtwork(response);

        // Assert
        ArtworkDto artwork = Assert.Single(result);
        Assert.Equal("https://coverartarchive.org/release/11111111-1111-1111-1111-111111111111/1.jpg", artwork.RemoteUrl);
    }

    [Fact]
    public void MapArtwork_WhenTheImageUrlUsesHttpsOnATrustedHost_ShouldKeepIt()
    {
        // Arrange
        const string ARCHIVE_URL = "https://archive.org/download/mbid-11111111-1111-1111-1111-111111111111/1.jpg";
        CoverArtArchiveImageResponse image = _coverArtArchiveImageResponseFixture.Create(image: ARCHIVE_URL, types: ["Front"]);
        CoverArtArchiveArtworkResponse response = _coverArtArchiveArtworkResponseFixture.Create(images: [image]);

        // Act
        IReadOnlyList<ArtworkDto> result = CoverArtArchiveMapper.MapArtwork(response);

        // Assert
        ArtworkDto artwork = Assert.Single(result);
        Assert.Equal(ARCHIVE_URL, artwork.RemoteUrl);
    }

    [Theory]
    [InlineData("front", ArtworkType.Cover)] // the front cover type, lower cased
    [InlineData("Front", ArtworkType.Cover)] // the front cover type
    [InlineData("BACK", ArtworkType.Back)] // the back cover type, upper cased
    [InlineData("Booklet", ArtworkType.Booklet)] // a booklet page
    [InlineData("Medium", ArtworkType.Medium)] // a medium label
    [InlineData("Tray", ArtworkType.Tray)] // the tray artwork
    [InlineData("Spine", ArtworkType.Spine)] // the spine artwork
    [InlineData("Obi", ArtworkType.Obi)] // the obi strip
    [InlineData("Sticker", ArtworkType.Sticker)] // the sticker
    [InlineData("Poster", ArtworkType.Poster)] // the poster
    [InlineData("Liner", ArtworkType.Liner)] // the liner notes
    [InlineData("Watermark", ArtworkType.Watermark)] // a watermark
    [InlineData("  Booklet  ", ArtworkType.Booklet)] // a type with surrounding white space
    [InlineData("SomeUnknownType", ArtworkType.Other)] // an unknown type is kept as generic artwork
    public void MapArtwork_WhenTheImageIsTaggedWithAType_ShouldMapItToTheExpectedArtworkType(string type, ArtworkType expectedArtworkType)
    {
        // Arrange
        CoverArtArchiveImageResponse image = _coverArtArchiveImageResponseFixture.Create(image: COVER_URL, types: [type]);
        CoverArtArchiveArtworkResponse response = _coverArtArchiveArtworkResponseFixture.Create(images: [image]);

        // Act
        IReadOnlyList<ArtworkDto> result = CoverArtArchiveMapper.MapArtwork(response);

        // Assert
        ArtworkDto artwork = Assert.Single(result);
        Assert.Equal(expectedArtworkType, artwork.Type);
        Assert.Equal(COVER_URL, artwork.RemoteUrl);
        Assert.Equal(0, artwork.Ordinal);
        Assert.Null(artwork.LocalPath);
    }

    [Fact]
    public void MapArtwork_WhenTheTypesAreNullAndTheImageIsFront_ShouldFallBackToCover()
    {
        // Arrange
        CoverArtArchiveImageResponse image = _coverArtArchiveImageResponseFixture.Create(image: COVER_URL, includeTypes: false, isFront: true, isBack: false);
        CoverArtArchiveArtworkResponse response = _coverArtArchiveArtworkResponseFixture.Create(images: [image]);

        // Act
        IReadOnlyList<ArtworkDto> result = CoverArtArchiveMapper.MapArtwork(response);

        // Assert
        ArtworkDto artwork = Assert.Single(result);
        Assert.Equal(ArtworkType.Cover, artwork.Type);
    }

    [Fact]
    public void MapArtwork_WhenTheTypesAreNullAndTheImageIsBack_ShouldFallBackToBack()
    {
        // Arrange
        CoverArtArchiveImageResponse image = _coverArtArchiveImageResponseFixture.Create(image: COVER_URL, includeTypes: false, isFront: false, isBack: true);
        CoverArtArchiveArtworkResponse response = _coverArtArchiveArtworkResponseFixture.Create(images: [image]);

        // Act
        IReadOnlyList<ArtworkDto> result = CoverArtArchiveMapper.MapArtwork(response);

        // Assert
        ArtworkDto artwork = Assert.Single(result);
        Assert.Equal(ArtworkType.Back, artwork.Type);
    }

    [Fact]
    public void MapArtwork_WhenTheTypesAreNullAndTheImageIsNeitherFrontNorBack_ShouldSkipIt()
    {
        // Arrange
        CoverArtArchiveImageResponse image = _coverArtArchiveImageResponseFixture.Create(image: COVER_URL, includeTypes: false, isFront: false, isBack: false);
        CoverArtArchiveArtworkResponse response = _coverArtArchiveArtworkResponseFixture.Create(images: [image]);

        // Act
        IReadOnlyList<ArtworkDto> result = CoverArtArchiveMapper.MapArtwork(response);

        // Assert
        Assert.Empty(result);
    }

    [Theory]
    [InlineData(null)] // a null element inside the types collection
    [InlineData("")] // an empty element inside the types collection
    [InlineData("   ")] // a white space element inside the types collection
    public void MapArtwork_WhenTheTypesCollectionContainsNullOrEmptyElements_ShouldSkipThem(string? type)
    {
        // Arrange
        CoverArtArchiveImageResponse image = _coverArtArchiveImageResponseFixture.Create(image: COVER_URL, types: [type], isFront: false, isBack: false);
        CoverArtArchiveArtworkResponse response = _coverArtArchiveArtworkResponseFixture.Create(images: [image]);

        // Act
        IReadOnlyList<ArtworkDto> result = CoverArtArchiveMapper.MapArtwork(response);

        // Assert
        // With no usable type and no front or back flag, the image describes no artwork at all.
        Assert.Empty(result);
    }

    [Fact]
    public void MapArtwork_WhenTheTypesCollectionContainsDuplicates_ShouldDeDuplicateAndKeepTheResponseOrder()
    {
        // Arrange
        CoverArtArchiveImageResponse image = _coverArtArchiveImageResponseFixture.Create(image: COVER_URL, types: ["Booklet", "Front", "Booklet", "Front"]);
        CoverArtArchiveArtworkResponse response = _coverArtArchiveArtworkResponseFixture.Create(images: [image]);

        // Act
        IReadOnlyList<ArtworkDto> result = CoverArtArchiveMapper.MapArtwork(response);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal(ArtworkType.Booklet, result[0].Type);
        Assert.Equal(ArtworkType.Cover, result[1].Type);
    }

    [Fact]
    public void MapArtwork_WhenAnImageIsTaggedWithSeveralTypes_ShouldEmitThemInTheOrderOfTheResponse()
    {
        // Arrange
        CoverArtArchiveImageResponse image = _coverArtArchiveImageResponseFixture.Create(image: COVER_URL, types: ["Front", "Back", "Booklet"]);
        CoverArtArchiveArtworkResponse response = _coverArtArchiveArtworkResponseFixture.Create(images: [image]);

        // Act
        IReadOnlyList<ArtworkDto> result = CoverArtArchiveMapper.MapArtwork(response);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal(ArtworkType.Cover, result[0].Type);
        Assert.Equal(ArtworkType.Back, result[1].Type);
        Assert.Equal(ArtworkType.Booklet, result[2].Type);
    }

    [Fact]
    public void MapArtwork_WhenSeveralImagesShareAType_ShouldAssignIncrementingOrdinals()
    {
        // Arrange
        CoverArtArchiveImageResponse firstPage = _coverArtArchiveImageResponseFixture.Create(image: $"{COVER_URL}?page=1", types: ["Booklet"]);
        CoverArtArchiveImageResponse secondPage = _coverArtArchiveImageResponseFixture.Create(image: $"{COVER_URL}?page=2", types: ["Booklet"]);
        CoverArtArchiveImageResponse thirdPage = _coverArtArchiveImageResponseFixture.Create(image: $"{COVER_URL}?page=3", types: ["Booklet"]);
        CoverArtArchiveArtworkResponse response = _coverArtArchiveArtworkResponseFixture.Create(images: [firstPage, secondPage, thirdPage]);

        // Act
        IReadOnlyList<ArtworkDto> result = CoverArtArchiveMapper.MapArtwork(response);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.All(result, artwork => Assert.Equal(ArtworkType.Booklet, artwork.Type));
        Assert.Equal(0, result[0].Ordinal);
        Assert.Equal(1, result[1].Ordinal);
        Assert.Equal(2, result[2].Ordinal);
    }

    [Fact]
    public void MapArtwork_WhenSeveralTypesArePresent_ShouldAssignIndependentOrdinalsPerType()
    {
        // Arrange
        CoverArtArchiveImageResponse cover = _coverArtArchiveImageResponseFixture.Create(image: $"{COVER_URL}?c=1", types: ["Front"]);
        CoverArtArchiveImageResponse booklet = _coverArtArchiveImageResponseFixture.Create(image: $"{COVER_URL}?b=1", types: ["Booklet"]);
        CoverArtArchiveImageResponse secondCover = _coverArtArchiveImageResponseFixture.Create(image: $"{COVER_URL}?c=2", types: ["Front"]);
        CoverArtArchiveArtworkResponse response = _coverArtArchiveArtworkResponseFixture.Create(images: [cover, booklet, secondCover]);

        // Act
        IReadOnlyList<ArtworkDto> result = CoverArtArchiveMapper.MapArtwork(response);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal(ArtworkType.Cover, result[0].Type);
        Assert.Equal(0, result[0].Ordinal);
        Assert.Equal(ArtworkType.Booklet, result[1].Type);
        Assert.Equal(0, result[1].Ordinal);
        Assert.Equal(ArtworkType.Cover, result[2].Type);
        Assert.Equal(1, result[2].Ordinal);
    }

    [Fact]
    public void MapArtwork_WhenEveryFieldIsPresent_ShouldMapEveryArtwork()
    {
        // Arrange
        CoverArtArchiveImageResponse front = _coverArtArchiveImageResponseFixture.Create(
            image: "https://coverartarchive.org/release/11111111-1111-1111-1111-111111111111/front.jpg",
            types: ["Front"],
            isFront: true,
            isBack: false);
        CoverArtArchiveImageResponse backAndMedium = _coverArtArchiveImageResponseFixture.Create(
            image: "http://coverartarchive.org/release/11111111-1111-1111-1111-111111111111/back.jpg",
            types: ["Back", "Medium"],
            isFront: false,
            isBack: true);
        CoverArtArchiveArtworkResponse response = _coverArtArchiveArtworkResponseFixture.Create(images: [front, backAndMedium]);

        // Act
        IReadOnlyList<ArtworkDto> result = CoverArtArchiveMapper.MapArtwork(response);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal(ArtworkType.Cover, result[0].Type);
        Assert.Equal(0, result[0].Ordinal);
        Assert.Null(result[0].LocalPath);
        Assert.Equal("https://coverartarchive.org/release/11111111-1111-1111-1111-111111111111/front.jpg", result[0].RemoteUrl);
        Assert.Equal(ArtworkType.Back, result[1].Type);
        Assert.Equal(0, result[1].Ordinal);
        Assert.Null(result[1].LocalPath);
        Assert.Equal("https://coverartarchive.org/release/11111111-1111-1111-1111-111111111111/back.jpg", result[1].RemoteUrl);
        Assert.Equal(ArtworkType.Medium, result[2].Type);
        Assert.Equal(0, result[2].Ordinal);
        Assert.Null(result[2].LocalPath);
        Assert.Equal("https://coverartarchive.org/release/11111111-1111-1111-1111-111111111111/back.jpg", result[2].RemoteUrl);
    }

    [Fact]
    public void MapArtwork_WhenCalled_ShouldNeverEmitALocalPath()
    {
        // Arrange
        CoverArtArchiveImageResponse front = _coverArtArchiveImageResponseFixture.Create(image: COVER_URL, types: ["Front"]);
        CoverArtArchiveArtworkResponse response = _coverArtArchiveArtworkResponseFixture.Create(images: [front]);

        // Act
        IReadOnlyList<ArtworkDto> result = CoverArtArchiveMapper.MapArtwork(response);

        // Assert
        Assert.All(result, artwork => Assert.Null(artwork.LocalPath));
    }
}
