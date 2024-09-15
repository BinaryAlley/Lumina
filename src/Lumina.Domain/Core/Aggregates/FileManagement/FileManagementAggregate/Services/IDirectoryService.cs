#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.Entities;
using Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.ValueObjects;
using System.Collections.Generic;
#endregion

namespace Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.Services;

/// <summary>
/// Interface for the service for handling directories.
/// </summary>
public interface IDirectoryService
{
    #region ===================================================================== METHODS ===================================================================================
    /// <summary>
    /// Retrieves subdirectories for the specified string path.
    /// </summary>
    /// <param name="path">String representation of the file path.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing either a collection of subdirectories or an error.</returns>
    ErrorOr<IEnumerable<Directory>> GetSubdirectories(string path);

    /// <summary>
    /// Retrieves subdirectories for the given directory.
    /// </summary>
    /// <param name="directory">Directory object to retrieve subdirectories for.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing either a collection of subdirectories or an error.</returns>
    ErrorOr<IEnumerable<Directory>> GetSubdirectories(Directory directory);

    /// <summary>
    /// Retrieves subdirectories for the specified file system path.
    /// </summary>
    /// <param name="path">Identifier for the file path.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing either a collection of subdirectories or an error.</returns>
    ErrorOr<IEnumerable<Directory>> GetSubdirectories(FileSystemPathId path);

    /// <summary>
    /// Creates a directory with the specified <paramref name="name"/>, at the specified <paramref name="path"/>.
    /// </summary>
    /// <param name="path">String representation of the path where the directory will be created.</param>
    /// <param name="name">The name of the directory that will be created.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing either a created directory, or an error.</returns>
    ErrorOr<Directory> CreateDirectory(string path, string name);

    /// <summary>
    /// Copies a directory located at <paramref name="sourcePath"/> to <paramref name="destinationPath"/>.
    /// </summary>
    /// <param name="sourcePath">String representation of the path where the directory to be copied is located.</param>
    /// <param name="destinationPath">String representation of the path where the directory will be copied.</param>
    /// <param name="overrideExisting">Whether to override existing directories, or not.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing either a copied directory, or an error.</returns>
    ErrorOr<Directory> CopyDirectory(string sourcePath, string destinationPath, bool? overrideExisting);

    /// <summary>
    /// Moves a directory located at <paramref name="sourcePath"/> to <paramref name="destinationPath"/>.
    /// </summary>
    /// <param name="sourcePath">String representation of the path where the directory to be moved is located.</param>
    /// <param name="destinationPath">String representation of the path where the directory will be moved.</param>
    /// <param name="overrideExisting">Whether to override existing directories, or not.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing either a moved directory, or an error.</returns>
    ErrorOr<Directory> MoveDirectory(string sourcePath, string destinationPath, bool? overrideExisting);

    /// <summary>
    /// Renames a directory with the specified <paramref name="name"/>, at the specified <paramref name="path"/>.
    /// </summary>
    /// <param name="path">String representation of the path of the directory that will be renamed.</param>
    /// <param name="name">The new name of the directory.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing either a renamed directory, or an error.</returns>
    ErrorOr<Directory> RenameDirectory(string path, string name);

    /// <summary>
    /// Delete a directory for the specified string path.
    /// </summary>
    /// <param name="path">String representation of the directory path.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing either the result of deleting a directory, or an error.</returns>
    ErrorOr<Deleted> DeleteDirectory(string path);
    #endregion
}