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
    [InlineData(ImageType.BMP, "bmp")] // Bitmap image.
    [InlineData(ImageType.GIF, "gif")] // Graphics interchange format.
    [InlineData(ImageType.PNG, "png")] // Portable network graphics.
    [InlineData(ImageType.TIFF, "tiff")] // Tagged image file format.
    [InlineData(ImageType.PICT, "pct")] // Macintosh picture.
    [InlineData(ImageType.ICO, "ico")] // Icon format.
    [InlineData(ImageType.PSD, "psd")] // Photoshop document.
    [InlineData(ImageType.JPEG2000, "jp2")] // JPEG 2000.
    [InlineData(ImageType.AVIF, "avif")] // AV1 image file format.
    [InlineData(ImageType.WEBP, "webp")] // Web picture format.
    [InlineData(ImageType.TGA, "tga")] // Truevision TGA.
    [InlineData(ImageType.SVG, "svg")] // Scalable vector graphics.
    public void ToFileExtension_WhenTypeIsRecognized_ShouldReturnConventionalExtension(ImageType imageType, string expectedExtension)
    {
        // Act
        string result = imageType.ToFileExtension();

        // Assert
        Assert.Equal(expectedExtension, result);
    }

    [Theory]
    [InlineData(ImageType.JPEG)] // Joint photographic experts group.
    [InlineData(ImageType.JPEG_CANON)] // Canon-specific JPEG.
    [InlineData(ImageType.JPEG_UNKNOWN)] // Unknown JPEG.
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
