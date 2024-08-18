#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Domain.Common.Models.Core;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Common.Errors;
#endregion

namespace Lumina.Domain.FileManagementAggregate.ValueObjects;

/// <summary>
/// Value Object for the information of a file.
/// </summary>
public class FileInfo : ValueObject
{
    #region ==================================================================== PROPERTIES =================================================================================
    /// <summary>
    /// Gets the path of the file.
    /// </summary>
    public string Path { get; }
    
    /// <summary>
    /// Gets the extension of the file.
    /// </summary>    
    public string FileExtension { get; }

    /// <summary>
    /// Gets the size of the file, in bytes.
    /// </summary>
    public long SizeInBytes { get; }

    /// <summary>
    /// Gets the last modification date of the file.
    /// </summary>    
    public DateTime LastModified { get; }

    /// <summary>
    /// Gets the optional MIME type of the file.
    /// </summary>    
    public Optional<string> MimeType { get; }
    #endregion

    #region ====================================================================== CTOR =====================================================================================
    /// <summary>
    /// Overload C-tor.
    /// </summary>
    /// <param name="path">The full path of the file.</param>
    /// <param name="fileExtension">The file extension.</param>
    /// <param name="sizeInBytes">The size of the file in bytes.</param>
    /// <param name="lastModified">The date and time when the file was last modified.</param>
    /// <param name="mimeType">The MIME type of the file, if available.</param>
    private FileInfo(string path, string fileExtension, long sizeInBytes, DateTime lastModified, Optional<string> mimeType)
    {
        Path = path;
        FileExtension = fileExtension;
        SizeInBytes = sizeInBytes;
        LastModified = lastModified;
        MimeType = mimeType;
    }
    #endregion

    #region ===================================================================== METHODS ===================================================================================
    /// <summary>
    /// Creates a new instance of <see cref="FileInfo"/>.
    /// </summary>
    /// <param name="path">The full path of the file.</param>
    /// <param name="fileExtension">The file extension.</param>
    /// <param name="sizeInBytes">The size of the file in bytes.</param>
    /// <param name="lastModified">The date and time when the file was last modified.</param>
    /// <param name="mimeType">The MIME type of the file, if available.</param>
    /// <returns>
    /// An <see cref="ErrorOr{T}"/> containing either a successfully created <see cref="FileInfo"/> or an error message.
    /// </returns>
    public static ErrorOr<FileInfo> Create(string path, string fileExtension, long sizeInBytes, DateTime lastModified, Optional<string> mimeType)
    {
        if (path is null)
            return Errors.FileManagement.PathCannotBeEmpty;
        if (fileExtension is null)
            return Errors.FileManagement.ExtensionCannotBeEmpty;
        return new FileInfo(path, fileExtension, sizeInBytes, lastModified, mimeType);
    }

    /// <inheritdoc/>
    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Path;
        yield return SizeInBytes;
        yield return FileExtension;
        yield return LastModified;
        yield return MimeType;
    }
    #endregion
}