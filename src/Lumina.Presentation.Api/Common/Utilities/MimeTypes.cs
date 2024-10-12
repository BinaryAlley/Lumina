#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Enums.PhotoLibrary;
using System.Collections.Frozen;
using System.Collections.Generic;
#endregion

namespace Lumina.Presentation.Api.Common.Utilities;

/// <summary>
/// Utility class for converting from domain image types to known image mime types.
/// </summary>
public static class MimeTypes
{
    private static readonly FrozenDictionary<ImageType, string> s_imageTypeToMimeType = new Dictionary<ImageType, string>
    {
        { ImageType.BMP, "image/bmp" },
        { ImageType.GIF, "image/gif" },
        { ImageType.PNG, "image/png" },
        { ImageType.TIFF, "image/tiff" },
        { ImageType.JPEG, "image/jpeg" },
        { ImageType.JPEG_CANON, "image/jpeg" },
        { ImageType.JPEG_UNKNOWN, "image/jpeg" },
        { ImageType.PICT, "image/x-pict" },
        { ImageType.ICO, "image/x-icon" },
        { ImageType.PSD, "image/vnd.adobe.photoshop" },
        { ImageType.JPEG2000, "image/jp2" },
        { ImageType.AVIF, "image/avif" },
        { ImageType.WEBP, "image/webp" },
        { ImageType.TGA, "image/x-tga" },
        { ImageType.SVG, "image/svg+xml" }
    }.ToFrozenDictionary();

    /// <summary>
    /// Gets the mime type of <paramref name="type"/>.
    /// </summary>
    /// <param name="type">The domain image type for which to get the mime type.</param>
    /// <returns>The mime type for <paramref name="type"/>.</returns>
    public static string GetMimeType(ImageType type)
    {
        return s_imageTypeToMimeType.TryGetValue(type, out string? mimeType) ? mimeType : "application/octet-stream";
    }
}
