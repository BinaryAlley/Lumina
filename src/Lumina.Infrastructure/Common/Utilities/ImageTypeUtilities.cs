#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.PhotoLibrary;
#endregion

namespace Lumina.Infrastructure.Common.Utilities;

/// <summary>
/// Provides helpers for the <see cref="ImageType"/> enumeration.
/// </summary>
public static class ImageTypeUtilities
{
    /// <summary>
    /// Gets the conventional file extension of the provided <paramref name="imageType"/>, without the leading dot.
    /// </summary>
    /// <param name="imageType">The image type whose file extension is resolved.</param>
    /// <returns>The file extension of the image type, or <c>bin</c> when the type is not a recognized image.</returns>
    public static string ToFileExtension(this ImageType imageType)
    {
        return imageType switch
        {
            ImageType.BMP => "bmp",
            ImageType.GIF => "gif",
            ImageType.PNG => "png",
            ImageType.TIFF => "tiff",
            ImageType.JPEG or ImageType.JPEG_CANON or ImageType.JPEG_UNKNOWN => "jpg",
            ImageType.PICT => "pct",
            ImageType.ICO => "ico",
            ImageType.PSD => "psd",
            ImageType.JPEG2000 => "jp2",
            ImageType.AVIF => "avif",
            ImageType.WEBP => "webp",
            ImageType.TGA => "tga",
            ImageType.SVG => "svg",
            _ => "bin"
        };
    }
}
