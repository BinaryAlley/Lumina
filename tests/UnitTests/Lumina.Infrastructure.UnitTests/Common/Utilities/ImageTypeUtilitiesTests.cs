#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.PhotoLibrary;
using Lumina.Infrastructure.Common.Utilities;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Infrastructure.UnitTests.Common.Utilities;

/// <summary>
/// Contains unit tests for the <see cref="ImageTypeUtilities"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ImageTypeUtilitiesTests
{
    [Theory]
    [InlineData(ImageType.BMP, "bmp")] // bitmap image
    [InlineData(ImageType.GIF, "gif")] // graphics interchange format
    [InlineData(ImageType.PNG, "png")] // portable network graphics
    [InlineData(ImageType.TIFF, "tiff")] // tagged image file format
    [InlineData(ImageType.PICT, "pct")] // macintosh picture
    [InlineData(ImageType.ICO, "ico")] // icon format
    [InlineData(ImageType.PSD, "psd")] // photoshop document
    [InlineData(ImageType.JPEG2000, "jp2")] // jpeg 2000
    [InlineData(ImageType.AVIF, "avif")] // av1 image file format
    [InlineData(ImageType.WEBP, "webp")] // web picture format
    [InlineData(ImageType.TGA, "tga")] // truevision tga
    [InlineData(ImageType.SVG, "svg")] // scalable vector graphics
    public void ToFileExtension_WhenTypeIsRecognized_ShouldReturnConventionalExtension(ImageType imageType, string expectedExtension)
    {
        // Act
        string result = imageType.ToFileExtension();

        // Assert
        Assert.Equal(expectedExtension, result);
    }

    [Theory]
    [InlineData(ImageType.JPEG)] // joint photographic experts group
    [InlineData(ImageType.JPEG_CANON)] // canon-specific jpeg
    [InlineData(ImageType.JPEG_UNKNOWN)] // unknown jpeg
    public void ToFileExtension_WhenTypeIsAnyJpeg_ShouldReturnJpg(ImageType imageType)
    {
        // Act
        string result = imageType.ToFileExtension();

        // Assert
        Assert.Equal("jpg", result);
    }

    [Fact]
    public void ToFileExtension_WhenTypeIsNotARecognizedImage_ShouldReturnBin()
    {
        // Act
        string result = ImageType.None.ToFileExtension();

        // Assert
        Assert.Equal("bin", result);
    }
}
