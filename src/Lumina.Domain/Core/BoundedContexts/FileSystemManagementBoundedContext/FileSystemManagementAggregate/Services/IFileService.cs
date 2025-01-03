#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Entities;
using System.Collections.Generic;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Services;

/// <summary>
/// Interface for the service for handling file system files.
/// </summary>
public interface IFileService
{
    /// <summary>
    /// Retrieves files for the specified string path.
    /// </summary>
    /// <param name="path">String representation of the file path.</param>
    /// <param name="includeHiddenElements">Whether to include hidden files or not.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing either a collection of files or an error.</returns>
    ErrorOr<IEnumerable<File>> GetFiles(string path, bool includeHiddenElements);

    /// <summary>
    /// Retrieves files associated with a given file.
    /// </summary>
    /// <param name="file">The file object.</param>
    /// <param name="includeHiddenElements">Whether to include hidden files or not.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing either a collection of files or an error.</returns>
    ErrorOr<IEnumerable<File>> GetFiles(File file, bool includeHiddenElements);

    /// <summary>
    /// Retrieves files for a specified file path ID.
    /// </summary>
    /// <param name="path">Identifier for the file path.</param>
    /// <param name="includeHiddenElements">Whether to include hidden files or not.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing either a collection of files or an error.</returns>
    ErrorOr<IEnumerable<File>> GetFiles(FileSystemPathId path, bool includeHiddenElements);

    /// <summary>
    /// Copies a file located at <paramref name="sourcePath"/> to <paramref name="destinationPath"/>.
    /// </summary>
    /// <param name="sourcePath">String representation of the path where the file to be copied is located.</param>
    /// <param name="destinationPath">String representation of the path where the file will be copied.</param>
    /// <param name="overrideExisting">Whether to override existing files, or not.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing either a copied file, or an error.</returns>
    ErrorOr<File> CopyFile(string sourcePath, string destinationPath, bool? overrideExisting);

    /// <summary>
    /// Moves a file located at <paramref name="sourcePath"/> to <paramref name="destinationPath"/>.
    /// </summary>
    /// <param name="sourcePath">String representation of the path where the file to be moved is located.</param>
    /// <param name="destinationPath">String representation of the path where the file will be moved.</param>
    /// <param name="overrideExisting">Whether to override existing files, or not.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing either a moved file, or an error.</returns>
    ErrorOr<File> MoveFile(string sourcePath, string destinationPath, bool? overrideExisting);

    /// <summary>
    /// Renames a file with the specified <paramref name="name"/>, at the specified <paramref name="path"/>.
    /// </summary>
    /// <param name="path">String representation of the path of the file that will be renamed.</param>
    /// <param name="name">The new name of the file.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing either a renamed file, or an error.</returns>
    ErrorOr<File> RenameFile(string path, string name);

    /// <summary>
    /// Delete a file for the specified string path.
    /// </summary>
    /// <param name="path">String representation of the file path.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing either the result of deleting a file, or an error.</returns>
    ErrorOr<Deleted> DeleteFile(string path);
}
