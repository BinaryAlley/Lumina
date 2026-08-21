#region ========================================================================= USING =====================================================================================
using Lumina.Infrastructure.Core.Themes;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Infrastructure.UnitTests.Core.Themes;

/// <summary>
/// Contains unit tests for the <see cref="ThemeMimeTypes"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ThemeMimeTypesTests
{
    [Theory]
    [InlineData("index.html", "text/html")]
    [InlineData("index.htm", "text/html")]
    [InlineData("style.css", "text/css")]
    [InlineData("app.js", "application/javascript")]
    [InlineData("manifest.json", "application/json")]
    [InlineData("image.png", "image/png")]
    [InlineData("photo.jpg", "image/jpeg")]
    [InlineData("photo.jpeg", "image/jpeg")]
    [InlineData("animation.gif", "image/gif")]
    [InlineData("graphic.webp", "image/webp")]
    [InlineData("icon.svg", "image/svg+xml")]
    [InlineData("favicon.ico", "image/x-icon")]
    [InlineData("font.woff", "font/woff")]
    [InlineData("font.woff2", "font/woff2")]
    [InlineData("font.ttf", "font/ttf")]
    [InlineData("readme.txt", "text/plain")]
    public void GetMimeType_WhenExtensionIsKnown_ShouldReturnMappedContentType(string filePath, string expectedContentType)
    {
        // Arrange
        // filePath and expectedContentType are provided by the test data

        // Act
        string result = ThemeMimeTypes.GetMimeType(filePath);

        // Assert
        Assert.Equal(expectedContentType, result);
    }

    [Fact]
    public void GetMimeType_WhenExtensionIsUnknown_ShouldReturnApplicationOctetStream()
    {
        // Arrange
        string filePath = "archive.unknown";

        // Act
        string result = ThemeMimeTypes.GetMimeType(filePath);

        // Assert
        Assert.Equal("application/octet-stream", result);
    }

    [Fact]
    public void GetMimeType_WhenFileHasNoExtension_ShouldReturnApplicationOctetStream()
    {
        // Arrange
        string filePath = "README";

        // Act
        string result = ThemeMimeTypes.GetMimeType(filePath);

        // Assert
        Assert.Equal("application/octet-stream", result);
    }

    [Fact]
    public void GetMimeType_WhenExtensionIsMixedCase_ShouldResolveCaseInsensitively()
    {
        // Arrange
        string filePath = "Photo.PNG";

        // Act
        string result = ThemeMimeTypes.GetMimeType(filePath);

        // Assert
        Assert.Equal("image/png", result);
    }
}
