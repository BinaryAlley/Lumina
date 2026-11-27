#region ========================================================================= USING =====================================================================================
using Nemera.Domain.SharedKernel.Common.Enums.PhotoLibrary;
using Nemera.Presentation.Api.Common.MimeTypes;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Nemera.Presentation.Api.UnitTests.Common.MimeTypes;

/// <summary>
/// Contains unit tests for the <see cref="ImageMimeTypes"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ImageMimeTypesTests
{
    [Theory]
    [InlineData(ImageType.BMP, "image/bmp")] // BMP image type.
    [InlineData(ImageType.JPEG, "image/jpeg")] // JPEG image type.
    [InlineData(ImageType.JPEG_CANON, "image/jpeg")] // Canon JPEG image type.
    [InlineData(ImageType.SVG, "image/svg+xml")] // SVG image type.
    [InlineData(ImageType.TGA, "image/x-tga")] // TGA image type.
    public void GetMimeType_WhenTypeIsKnown_ShouldReturnTheMappedMimeType(ImageType type, string expectedMimeType)
    {
        // Act
        string result = ImageMimeTypes.GetMimeType(type);

        // Assert
        Assert.Equal(expectedMimeType, result);
    }

    [Theory]
    [InlineData(ImageType.None)] // The none image type has no mime mapping.
    [InlineData((ImageType)999)] // An unknown image type has no mime mapping.
    public void GetMimeType_WhenTypeIsUnknown_ShouldReturnOpaqueBinaryMimeType(ImageType type)
    {
        // Act
        string result = ImageMimeTypes.GetMimeType(type);

        // Assert
        Assert.Equal("application/octet-stream", result);
    }
}
