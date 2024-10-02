#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Contracts.Enums.PhotoLibrary;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.Strategies.Environment;
using Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.ValueObjects;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.Services;

/// <summary>
/// Service for handling thumbnails.
/// </summary>
public class ThumbnailService : IThumbnailService
{
    #region ================================================================== FIELD MEMBERS ================================================================================
    private readonly IEnvironmentContext _environmentContext;
    #endregion

    #region ====================================================================== CTOR =====================================================================================
    /// <summary>
    /// Initializes a new instance of the <see cref="ThumbnailService"/> class.
    /// </summary>
    /// <param name="environmentContext">Injected environment context.</param>
    public ThumbnailService(IEnvironmentContext environmentContext)
    {
        _environmentContext = environmentContext;
    }
    #endregion

    #region ===================================================================== METHODS ===================================================================================
    /// <summary>
    /// Gets the thumbnail of a file at the specified path.
    /// </summary>
    /// <param name="path">String representation of the file path.</param>
    /// <param name="quality">The quality of the thumbnail to get.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing either a collection of bytes representing the thumbnail of the file at the specified path or an error.</returns>
    public async Task<ErrorOr<Thumbnail>> GetThumbnailAsync(string path, int quality, CancellationToken cancellationToken)
    {
        ErrorOr<FileSystemPathId> fileSystemPathIdResult = FileSystemPathId.Create(path);
        if (fileSystemPathIdResult.IsError)
            return fileSystemPathIdResult.Errors;
        return await GetThumbnailAsync(fileSystemPathIdResult.Value, quality, cancellationToken);
    }

    /// <summary>
    /// Gets the thumbnail of a file at the specified path.
    /// </summary>
    /// <param name="path">The path of the file for which to get the thumbnail.</param>
    /// <param name="quality">The quality of the thumbnail to get.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing either a collection of bytes representing the thumbnail of the file at the specified path or an error.</returns>
    public async Task<ErrorOr<Thumbnail>> GetThumbnailAsync(FileSystemPathId path, int quality, CancellationToken cancellationToken)
    {
        // first, get the type of the image file
        ErrorOr<ImageType> imageTypeResult = await _environmentContext.FileTypeService.GetImageTypeAsync(path, cancellationToken);
        if (imageTypeResult.IsError)
            return imageTypeResult.Errors;
        if (imageTypeResult.Value != ImageType.None)
        {
            // then, get its bytes
            ErrorOr<byte[]> resultFileContents = _environmentContext.FileProviderService.GetFileAsync(path);
            if (resultFileContents.IsError)
                return resultFileContents.Errors;
            byte[] fileContents = resultFileContents.Value;
            // finally, resize or adjust quality based on image type
            byte[]? adjustedImage = null;
            if (imageTypeResult.Value is ImageType.JPEG or ImageType.JPEG_CANON or ImageType.JPEG2000
                or ImageType.JPEG_UNKNOWN or ImageType.PNG or ImageType.BMP
                or ImageType.WEBP or ImageType.GIF or ImageType.TIFF or ImageType.TGA)
                adjustedImage = await AdjustImageResolutionAsync(fileContents, imageTypeResult.Value, quality, cancellationToken);
            return new Thumbnail(imageTypeResult.Value, adjustedImage ?? fileContents);
        }
        else
            return Errors.Thumbnails.NoThumbnail;
    }

    /// <summary>
    /// Adjusts the quality or compression of an image, based on its format.
    /// </summary>
    /// <param name="imageBytes">The byte array of the source image.</param>
    /// <param name="imageType">The format of the image whose quality is adjusted.</param>
    /// <param name="quality">A value between 1 (lowest quality) to 100 (highest quality).</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>A byte array representing the processed image.</returns>
    private static async Task<byte[]> AdjustImageQualityAsync(byte[] imageBytes, ImageType imageType, int quality, CancellationToken cancellationToken)
    {
        using MemoryStream inputMemoryStream = new(imageBytes);
        using MemoryStream outputMemoryStream = new();
        switch (imageType)
        {
            case ImageType.JPEG:
            case ImageType.JPEG2000:
            case ImageType.JPEG_CANON:
            case ImageType.JPEG_UNKNOWN:
                {
                    using Image image = await Image.LoadAsync(inputMemoryStream, cancellationToken);
                    await image.SaveAsync(outputMemoryStream, new JpegEncoder() { Quality = quality }, cancellationToken);
                    break;
                }
            case ImageType.PNG:
                {
                    using Image image = await Image.LoadAsync(inputMemoryStream, cancellationToken);
                    // PNG is lossless; quality adjustments don't apply in the same way - we can adjust compression
                    int compressionLevel = MapRange(quality, 1, 100, 1, 9);
                    PngEncoder encoder = new()
                    { CompressionLevel = (PngCompressionLevel)compressionLevel };
                    await image.SaveAsync(outputMemoryStream, encoder, cancellationToken);
                    break;
                }
            case ImageType.BMP:
                {
                    using Image image = await Image.LoadAsync(inputMemoryStream, cancellationToken);
                    await image.SaveAsync(outputMemoryStream, new PngEncoder(), cancellationToken); // convert BMP to PNG for compression
                    break;
                }
            default:
                return imageBytes; // for unsupported formats, just return the original bytes
        }
        return outputMemoryStream.ToArray();
    }

    /// <summary>
    /// Adjusts the quality or size of an image, based on its format.
    /// </summary>
    /// <param name="imageBytes">The byte array of the source image.</param>
    /// <param name="imageType">The format of the image whose quality is adjusted.</param>
    /// <param name="quality">A value between 1 (lowest quality) to 100 (highest quality).</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The bytes of the adjusted image.</returns>
    private static async Task<byte[]> AdjustImageResolutionAsync(byte[] imageBytes, ImageType imageType, int quality, CancellationToken cancellationToken)
    {
        using MemoryStream inputMemoryStream = new(imageBytes);
        using MemoryStream outputMemoryStream = new();
        Image image = await Image.LoadAsync(inputMemoryStream, cancellationToken);
        double ratio = quality / 100.0;
        int newWidth = (int)(image.Width * ratio);
        int newHeight = (int)(image.Height * ratio);
        // ensure new dimensions are not zero
        newWidth = Math.Max(1, newWidth);
        newHeight = Math.Max(1, newHeight);
        ResizeOptions options = new()
        {
            Size = new Size(newWidth, newHeight),
            Mode = ResizeMode.Max
        };
        // resize the image to the specified size
        image.Mutate(ctx => ctx.Resize(options));
        // save the resized image and return its bytes
        switch (imageType)
        {
            case ImageType.BMP:
            case ImageType.PNG:
            case ImageType.GIF:
            case ImageType.TGA:
            case ImageType.WEBP:
            case ImageType.TIFF:
            case ImageType.JPEG:
            case ImageType.JPEG2000:
            case ImageType.JPEG_CANON:
            case ImageType.JPEG_UNKNOWN:
                await image.SaveAsync(outputMemoryStream, image.Metadata.DecodedImageFormat!, cancellationToken);
                break;
            default:
                return imageBytes; // for unsupported formats, just return the original bytes
        }
        return outputMemoryStream.ToArray();
    }

    /// <summary>
    /// Maps a given value from a source range to a target range.
    /// </summary>
    /// <param name="value">The value to map.</param>
    /// <param name="fromSource">The inclusive lower bound of the source range.</param>
    /// <param name="toSource">The inclusive upper bound of the source range.</param>
    /// <param name="fromTarget">The inclusive lower bound of the target range.</param>
    /// <param name="toTarget">The inclusive upper bound of the target range.</param>
    /// <returns>
    /// A value mapped to the target range.
    /// </returns>
    private static int MapRange(int value, int fromSource, int toSource, int fromTarget, int toTarget)
    {
        return ((value - fromSource) * (toTarget - fromTarget) / (toSource - fromSource)) + fromTarget;
    }
    #endregion
}
