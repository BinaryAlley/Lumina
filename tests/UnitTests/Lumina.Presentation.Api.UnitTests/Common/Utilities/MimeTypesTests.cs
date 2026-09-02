#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.PhotoLibrary;
using Lumina.Presentation.Api.Common.Utilities;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.UnitTests.Common.Utilities;

/// <summary>
/// Contains unit tests for the <see cref="MimeTypes"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MimeTypesTests
{
    [Theory]
    [InlineData(ImageType.BMP, "image/bmp")] // bmp image type
    [InlineData(ImageType.JPEG, "image/jpeg")] // jpeg image type
    [InlineData(ImageType.JPEG_CANON, "image/jpeg")] // canon jpeg image type
    [InlineData(ImageType.SVG, "image/svg+xml")] // svg image type
    [InlineData(ImageType.TGA, "image/x-tga")] // tga image type
    public void GetMimeType_WhenTypeIsKnown_ShouldReturnTheMappedMimeType(ImageType type, string expectedMimeType)
    {
        // Act
        string result = MimeTypes.GetMimeType(type);

        // Assert
        Assert.Equal(expectedMimeType, result);
    }

    [Theory]
    [InlineData(ImageType.None)] // the none image type has no mime mapping
    [InlineData((ImageType)999)] // an unknown image type has no mime mapping
    public void GetMimeType_WhenTypeIsUnknown_ShouldReturnOpaqueBinaryMimeType(ImageType type)
    {
        // Act
        string result = MimeTypes.GetMimeType(type);

        // Assert
        Assert.Equal("application/octet-stream", result);
    }
}
