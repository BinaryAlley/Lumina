#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.ValueObjects;
using System;
using System.Collections.Generic;
#endregion

namespace Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.Services;

/// <summary>
/// Interface defining the file provider service.
/// </summary>
public interface IFileProviderService
{
    #region ===================================================================== METHODS ===================================================================================
    /// <summary>
    /// Retrieves a list of files at the specified path.
    /// </summary>
    /// <param name="path">The path for which to retrieve the list of files.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing either a collection of file paths, or an error.</returns>
    ErrorOr<IEnumerable<FileSystemPathId>> GetFilePaths(FileSystemPathId path);

    /// <summary>
    /// Retrieves the contents of a file at the specified path.
    /// </summary>
    /// <param name="path">The path for which to retrieve the file contents.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing either the contents of a file at the specified path, or an error.</returns>
    ErrorOr<byte[]> GetFileAsync(FileSystemPathId path);

    /// <summary>
    /// Checks if a file with the specified path exists.
    /// </summary>
    /// <param name="path">The path of the file whose existance is checked.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing either the result of checking the existance of a file, or an error.</returns>
    ErrorOr<bool> FileExists(FileSystemPathId path);

    /// <summary>
    /// Retrieves the file name from the specified path.
    /// </summary>
    /// <param name="path">The path to extract the file name from.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing either the name of the file without the path, or the last segment of the path if no file name is found, or an error.</returns>
    ErrorOr<string> GetFileName(FileSystemPathId path);

    /// <summary>
    /// Gets the last write time of a file at the specified path.
    /// </summary>
    /// <param name="path">The path of the file to retrieve the last write time for.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing either the optional last write time of <paramref name="path"/> if available, or an error.</returns>
    ErrorOr<Optional<DateTime>> GetLastWriteTime(FileSystemPathId path);

    /// <summary>
    /// Gets the creation time of a file at the specified path.
    /// </summary>
    /// <param name="path">The path of the file to retrieve the creation time for.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing either the otional creation time of <paramref name="path"/> if available, or an error.</returns>
    ErrorOr<Optional<DateTime>> GetCreationTime(FileSystemPathId path);

    /// <summary>
    /// Gets the size of a file at the specified path.
    /// </summary>
    /// <param name="path">The path of the file to retrieve the size for.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing either the size of <paramref name="path"/> or an error.</returns>
    ErrorOr<long?> GetSize(FileSystemPathId path);

    /// <summary>
    /// Copies a file located at <paramref name="sourceFilePath"/> to <paramref name="destinationDirectoryPath"/>.
    /// </summary>
    /// <param name="sourceFilePath">Identifier for the path where the file to be copied is located.</param>
    /// <param name="destinationDirectoryPath">Identifier for the path of the directory where the file will be copied.</param>
    /// <param name="overrideExisting">Whether to override existing files, or not.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing either the copied file, or an error.</returns>
    ErrorOr<FileSystemPathId> CopyFile(FileSystemPathId sourceFilePath, FileSystemPathId destinationDirectoryPath, bool overrideExisting);

    /// <summary>
    /// Moves a file located at <paramref name="sourceFilePath"/> to <paramref name="destinationDirectoryPath"/>.
    /// </summary>
    /// <param name="sourceFilePath">Identifier for the path where the file to be moved is located.</param>
    /// <param name="destinationDirectoryPath">Identifier for the path of the directory where the file will be moved.</param>
    /// <param name="overrideExisting">Whether to override existing files, or not.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing either a moved file, or an error.</returns>
    ErrorOr<FileSystemPathId> MoveFile(FileSystemPathId sourceFilePath, FileSystemPathId destinationDirectoryPath, bool overrideExisting);

    /// <summary>
    /// Renames a file at the specified path.
    /// </summary>
    /// <param name="path">The path of the file to be renamed.</param>
    /// <param name="name">The new name of the file.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing either the absolute path of the renamed file, or an error.</returns>
    ErrorOr<FileSystemPathId> RenameFile(FileSystemPathId path, string name);

    /// <summary>
    /// Deletes a file at the specified path.
    /// </summary>
    /// <param name="path">The path of the file to be deleted.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing either the result of deleting a file, or an error.</returns>
    ErrorOr<Deleted> DeleteFile(FileSystemPathId path);
    #endregion
}