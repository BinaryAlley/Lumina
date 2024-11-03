#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Contracts.Enums.FileSystem;
using Lumina.Contracts.Enums.PhotoLibrary;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Entities;
using System;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Services;

/// <summary>
/// Class defining methods for handling file-related operations.
/// </summary>
public class FileTypeService : IFileTypeService
{
    private readonly IFileSystem _fileSystem;
    private readonly IFileSystemPermissionsService _fileSystemPermissionsService;
    private const int BUFFER_SIZE = 18; // 18 bytes should be more than enough for common images header types

    /// <summary>
    /// Initializes a new instance of the <see cref="FileTypeService"/> class.
    /// </summary>
    /// <param name="fileSystem">Injected service used to interact with the local filesystem.</param>
    /// <param name="fileSystemPermissionsService">Injected service used to determine local filesystem permissions.</param>
    public FileTypeService(IFileSystem fileSystem, IFileSystemPermissionsService fileSystemPermissionsService)
    {
        _fileSystem = fileSystem;
        _fileSystemPermissionsService = fileSystemPermissionsService;
    }

    /// <summary>
    /// Determines if <paramref name="file"/> is of type image or not, and returns its type.
    /// </summary>
    /// <param name="file">The file to determine if it is an image or not.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing the type of image or an error.</returns>
    public async Task<ErrorOr<ImageType>> GetImageTypeAsync(File file, CancellationToken cancellationToken)
    {
        return await GetImageTypeAsync(file.Id, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Determines if a file identified by <paramref name="path"/> is of type image or not, and returns its type.
    /// </summary>
    /// <param name="path">The path of the file to determine if it is an image or not.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing the type of image or an error.</returns>
    public async Task<ErrorOr<ImageType>> GetImageTypeAsync(FileSystemPathId path, CancellationToken cancellationToken)
    {
        // check if the user has access permissions to the provided path
        if (!_fileSystemPermissionsService.CanAccessPath(path, FileAccessMode.ReadContents))
            return Errors.Permission.UnauthorizedAccess;
        Memory<byte> buffer = new byte[BUFFER_SIZE];
        using FileSystemStream stream = _fileSystem.FileStream.New(path.Path, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite);
        // check if the file's length is less than the buffer size
        if (stream.Length < BUFFER_SIZE)
            return ImageType.None;
        // read only the first bytes of the file, equal to the buffer size
        await stream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
        byte[] firstBytes = buffer[..BUFFER_SIZE].ToArray();
        // check if its a known image type, based on header bytes
        ImageType type = IdentifyHeader(firstBytes);
        if (type != ImageType.None)
            return type;
        // no known image header types were found, check other methods
        string content = Encoding.UTF8.GetString(buffer.ToArray());
        if (IsSvg(content, path.Path))
            return ImageType.SVG;
        else if (IsTga(buffer.ToArray()))
            return ImageType.TGA;
        else
            return ImageType.None;
    }

    /// <summary>
    /// Determines if the content of a file identified by <paramref name="path"/> belongs to a SVG.
    /// </summary>
    /// <param name="initialContent">The initial content of the file to check.</param>
    /// <param name="path">Path of the file to check.</param>
    /// <returns><see langword="true"/> if the file is a SVG image, <see langword="false"/> otherwise.</returns>
    private bool IsSvg(string initialContent, string path)
    {
        // check if it starts with <svg
        if (initialContent.StartsWith("<svg", StringComparison.OrdinalIgnoreCase))
            return true;
        // check if it starts with <?xml
        if (initialContent.StartsWith("<?xml", StringComparison.OrdinalIgnoreCase))
        {
            // Read more content from the file
            using FileSystemStream stream = _fileSystem.FileStream.New(path, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite);
            byte[] svgBuffer = new byte[1000];
            stream.Read(svgBuffer, 0, 1000);
            string extendedContent = Encoding.UTF8.GetString(svgBuffer);
            // check if the extended content contains <svg
            return extendedContent.Contains("<svg", StringComparison.OrdinalIgnoreCase);
        }
        return false;
    }

    /// <summary>
    /// Determines if the buffer represents a TGA file.
    /// </summary>
    /// <param name="buffer">Byte array to check.</param>
    /// <returns><see langword="true"/> if the file is a TGA image, <see langword="false"/> otherwise.</returns>
    private static bool IsTga(byte[] buffer)
    {
        if (buffer.Length < 18)
            return false;
        byte imageType = buffer[2];
        return imageType is 1 or 2 or 3 or 9 or 10 or 11;
    }

    /// <summary>
    /// Identifies the image type based on the provided header bytes.
    /// </summary>
    /// <param name="firstBytes">The first bytes of the file, typically used to identify the file type.</param>
    /// <returns>The type of the image, or <see cref="{ImageType.None}"/> if unrecognized.</returns>
    private static ImageType IdentifyHeader(byte[] firstBytes)
    {
        if (Encoding.ASCII.GetBytes("BM").SequenceEqual(firstBytes[..2]))
            return ImageType.BMP; // BMP
        if (Encoding.ASCII.GetBytes("GIF").SequenceEqual(firstBytes[..3]))
            return ImageType.GIF; // GIF
        if (new byte[] { 137, 80, 78, 71 }.SequenceEqual(firstBytes[..4]))
            return ImageType.PNG; // PNG
        if (new byte[] { 73, 73, 42 }.SequenceEqual(firstBytes[..3]) || new byte[] { 77, 77, 42 }.SequenceEqual(firstBytes[..3]))
            return ImageType.TIFF; // TIFF
        if (new byte[] { 255, 216, 255, 224 }.SequenceEqual(firstBytes[..4]))
            return ImageType.JPEG; // JPEG
        if (new byte[] { 255, 216, 255, 225 }.SequenceEqual(firstBytes[..4]))
            return ImageType.JPEG_CANON; // JPEG CANON
        if (new byte[] { 255, 216, 255, 226 }.SequenceEqual(firstBytes[..4]))
            return ImageType.JPEG_UNKNOWN; // JPEG UNKNOWN
        if (new byte[] { 0x00, 0x11, 0x02, 0xFF }.SequenceEqual(firstBytes[..4]))
            return ImageType.PICT; // PICT
        if (new byte[] { 0x00, 0x00, 0x01, 0x00 }.SequenceEqual(firstBytes[..4]))
            return ImageType.ICO; // ICO
        if (new byte[] { 0x38, 0x42, 0x50, 0x53 }.SequenceEqual(firstBytes[..4]))
            return ImageType.PSD; // PSD
        if (new byte[] { 0xFF, 0x4F, 0xFF, 0x51 }.SequenceEqual(firstBytes[..4]))
            return ImageType.JPEG2000; // JPEG 2000
        if (new byte[] { 0x00, 0x00, 0x00, 0x0C, 0x6A, 0x70, 0x20, 0x20 }.SequenceEqual(firstBytes[..8]))
            return ImageType.AVIF; // AVIF
        if (new byte[] { 0x00, 0x00, 0x00, 0x0C, 0x6A, 0x50, 0x20, 0x20 }.SequenceEqual(firstBytes[..8]))
            return ImageType.JPEG2000; // JPEG 2000 (variant)
        if (new byte[] { 0x52, 0x49, 0x46, 0x46 }.SequenceEqual(firstBytes[..4]) && new byte[] { 0x57, 0x45, 0x42, 0x50 }.SequenceEqual(firstBytes[8..12]))
            return ImageType.WEBP; // WEBP
        return ImageType.None;
    }
}
