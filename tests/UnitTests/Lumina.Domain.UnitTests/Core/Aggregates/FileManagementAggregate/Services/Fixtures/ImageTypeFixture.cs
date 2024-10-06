#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Enums.PhotoLibrary;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using System.Diagnostics.CodeAnalysis;
using System.IO;
#endregion

namespace Lumina.Domain.UnitTests.Core.Aggregates.FileManagementAggregate.Services.Fixtures;

/// <summary>
/// Fixture class for the <see cref="ImageType"/> enum.
/// </summary>
[ExcludeFromCodeCoverage]
public class ImageTypeFixture : TheoryData<ImageType, byte[]>
{

    #region ====================================================================== CTOR =====================================================================================
    /// <summary>
    /// Initializes a new instance of the <see cref="ImageTypeFixture"/> class.
    /// </summary>
    public ImageTypeFixture()
    {
        Add(ImageType.JPEG, CreateImage(new JpegEncoder()));
        Add(ImageType.PNG, CreateImage(new PngEncoder()));
        Add(ImageType.BMP, CreateImage(new BmpEncoder()));
        Add(ImageType.GIF, CreateImage(new GifEncoder()));
    }
    #endregion

    #region ===================================================================== METHODS ===================================================================================
    /// <summary>
    /// Creates a byte array representing a thumbnail fixture.
    /// </summary>
    /// <param name="encoder">The encoder used for the image.</param>
    /// <returns>The byte array of the created image.</returns>
    private static byte[] CreateImage(IImageEncoder encoder)
    {
        using Image<Rgba32> image = new(10, 10);
        using MemoryStream ms = new();
        image.Save(ms, encoder);
        return ms.ToArray();
    }
    #endregion
}
