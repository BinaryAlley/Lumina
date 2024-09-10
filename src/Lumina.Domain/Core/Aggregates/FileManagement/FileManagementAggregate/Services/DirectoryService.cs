#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Domain.Common.Enums;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.Entities;
using Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.Strategies.Environment;
using Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.Strategies.Platform;
using Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.ValueObjects;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
#endregion

namespace Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.Services;

/// <summary>
/// Service for handling directories.
/// </summary>
public class DirectoryService : IDirectoryService
{
    #region ================================================================== FIELD MEMBERS ================================================================================
    private readonly IPlatformContext _platformContext;
    private readonly IEnvironmentContext _environmentContext;
    #endregion

    #region ====================================================================== CTOR =====================================================================================
    /// <summary>
    /// Initializes a new instance of the <see cref="DirectoryProviderService"/> class.
    /// </summary>
    /// <param name="environmentContext">Injected facade service for environment contextual services.</param>
    /// <param name="platformContextManager">Injected facade service for platform contextual services.</param>
    public DirectoryService(IEnvironmentContext environmentContext, IPlatformContextManager platformContextManager)
    {
        _platformContext = platformContextManager.GetCurrentContext();
        _environmentContext = environmentContext;
    }
    #endregion

    #region ===================================================================== METHODS ===================================================================================
    /// <summary>
    /// Retrieves subdirectories for the specified string path.
    /// </summary>
    /// <param name="path">String representation of the file path.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing either a collection of subdirectories or an error.</returns>
    public async Task<ErrorOr<IEnumerable<Directory>>> GetSubdirectoriesAsync(string path)
    {
        var fileSystemPathIdResult = FileSystemPathId.Create(path);
        if (fileSystemPathIdResult.IsError)
            return fileSystemPathIdResult.Errors;
        return await GetSubdirectoriesAsync(fileSystemPathIdResult.Value).ConfigureAwait(false);
    }

    /// <summary>
    /// Retrieves subdirectories for the given directory.
    /// </summary>
    /// <param name="directory">Directory object to retrieve subdirectories for.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing either a collection of subdirectories or an error.</returns>
    public async Task<ErrorOr<IEnumerable<Directory>>> GetSubdirectoriesAsync(Directory directory)
    {
        return await GetSubdirectoriesAsync(directory.Id).ConfigureAwait(false);
    }

    /// <summary>
    /// Retrieves subdirectories for the specified file system path.
    /// </summary>
    /// <param name="path">Identifier for the file path.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing either a collection of subdirectories or an error.</returns>
    public async Task<ErrorOr<IEnumerable<Directory>>> GetSubdirectoriesAsync(FileSystemPathId path)
    {
        // retrieve the list of subdirectories
        ErrorOr<Task<IEnumerable<FileSystemPathId>>> subdirectoryPathsResult = _environmentContext.DirectoryProviderService.GetSubdirectoryPathsAsync(path);
        if (subdirectoryPathsResult.IsError)
            return subdirectoryPathsResult.Errors;
        List<Directory> result = new();
        IEnumerable<FileSystemPathId> subdirectoryPaths = await subdirectoryPathsResult.Value.ConfigureAwait(false);
        foreach (FileSystemPathId subPath in subdirectoryPaths)
        {
            // extract directory details
            ErrorOr<string> dirNameResult = _environmentContext.DirectoryProviderService.GetFileName(subPath);
            ErrorOr<DateTime?> dateModifiedResult = _environmentContext.DirectoryProviderService.GetLastWriteTime(subPath);
            ErrorOr<DateTime?> dateCreatedResult = _environmentContext.DirectoryProviderService.GetCreationTime(subPath);

            // if any error occurred, mark directory as Inaccessible
            if (dirNameResult.IsError || dateModifiedResult.IsError || dateCreatedResult.IsError)
            {
                Directory errorDir = new(subPath, !dirNameResult.IsError ? dirNameResult.Value : null!,
                    !dateCreatedResult.IsError ? dateCreatedResult.Value : null, !dateModifiedResult.IsError ? dateModifiedResult.Value : null);
                errorDir.SetStatus(FileSystemItemStatus.Inaccessible);
                result.Add(errorDir);
            }
            else
            {
                Directory subDirectory = new(subPath, dirNameResult.Value, dateCreatedResult.Value, dateModifiedResult.Value);
                result.Add(subDirectory);
            }
        }
        return result;
    }

    /// <summary>
    /// Creates a directory with the specified <paramref name="name"/>, at the specified <paramref name="path"/>.
    /// </summary>
    /// <param name="path">String representation of the path where the directory will be created.</param>
    /// <param name="name">The name of the directory that will be created.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing either the result of creating a directory, or an error.</returns>
    public ErrorOr<Directory> CreateDirectory(string path, string name)
    {
        var fileSystemPathIdResult = FileSystemPathId.Create(path);
        if (fileSystemPathIdResult.IsError)
            return fileSystemPathIdResult.Errors;
        return CreateDirectory(fileSystemPathIdResult.Value, name);
    }

    /// <summary>
    /// Creates a directory with the specified <paramref name="name"/>, at the specified <paramref name="path"/>.
    /// </summary>
    /// <param name="path">Identifier for the path where the directory will be created.</param>
    /// <param name="name">The name of the directory that will be created.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing either the result of creating a directory, or an error.</returns>
    public ErrorOr<Directory> CreateDirectory(FileSystemPathId path, string name)
    {
        // first, check if the directory about to be created does not already exist
        var combinedPath = _platformContext.PathStrategy.CombinePath(path, name);
        if (combinedPath.IsError)
            return combinedPath.Errors;
        var directoryExists = _environmentContext.DirectoryProviderService.DirectoryExists(combinedPath.Value);
        if (directoryExists.IsError)
            return directoryExists.Errors;
        else if (directoryExists.Value == true)
            return Errors.FileManagement.DirectoryAlreadyExistsError;
        else
        {
            // create the new directory
            ErrorOr<FileSystemPathId> newDirectoryPathResult = _environmentContext.DirectoryProviderService.CreateDirectory(path, name);
            if (newDirectoryPathResult.IsError)
                return newDirectoryPathResult.Errors;
            ErrorOr<string> dirNameResult = _environmentContext.DirectoryProviderService.GetFileName(path);
            ErrorOr<DateTime?> dateModifiedResult = _environmentContext.DirectoryProviderService.GetLastWriteTime(path);
            ErrorOr<DateTime?> dateCreatedResult = _environmentContext.DirectoryProviderService.GetCreationTime(path);
            // if any error occurred, mark directory as Inaccessible
            if (dirNameResult.IsError || dateModifiedResult.IsError || dateCreatedResult.IsError)
            {
                Directory errorDir = new(newDirectoryPathResult.Value, !dirNameResult.IsError ? dirNameResult.Value : null!,
                    !dateCreatedResult.IsError ? dateCreatedResult.Value : null, !dateModifiedResult.IsError ? dateModifiedResult.Value : null);
                errorDir.SetStatus(FileSystemItemStatus.Inaccessible);
                return errorDir;
            }
            else
                return new Directory(newDirectoryPathResult.Value, dirNameResult.Value, dateCreatedResult.Value, dateModifiedResult.Value);
        }
    }

    /// <summary>
    /// Copies a directory located at <paramref name="sourcePath"/> to <paramref name="destinationPath"/>.
    /// </summary>
    /// <param name="sourcePath">String representation of the path where the directory to be copied is located.</param>
    /// <param name="destinationPath">String representation of the path where the directory will be copied.</param>
    /// <param name="overrideExisting">Whether to override existing directories, or not.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing either a copied directory, or an error.</returns>
    public ErrorOr<Directory> CopyDirectory(string sourcePath, string destinationPath, bool? overrideExisting)
    {
        // make sure the paths are in the expected format
        if (!sourcePath.EndsWith(_platformContext.PathStrategy.PathSeparator))
            sourcePath += _platformContext.PathStrategy.PathSeparator;
        if (!destinationPath.EndsWith(_platformContext.PathStrategy.PathSeparator))
            destinationPath += _platformContext.PathStrategy.PathSeparator;
        var fileSystemSourcePathIdResult = FileSystemPathId.Create(sourcePath);
        if (fileSystemSourcePathIdResult.IsError)
            return fileSystemSourcePathIdResult.Errors;
        var fileSystemDestinationPathIdResult = FileSystemPathId.Create(destinationPath);
        if (fileSystemDestinationPathIdResult.IsError)
            return fileSystemDestinationPathIdResult.Errors;
        return CopyDirectory(fileSystemSourcePathIdResult.Value, fileSystemDestinationPathIdResult.Value, overrideExisting ?? false);
    }

    /// <summary>
    /// Copies a directory located at <paramref name="sourcePath"/> to <paramref name="destinationPath"/>.
    /// </summary>
    /// <param name="sourcePath">Identifier for the path where the directory to be copied is located.</param>
    /// <param name="destinationPath">Identifier for the path where the directory will be copied.</param>
    /// <param name="overrideExisting">Whether to override existing directories, or not.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing either the copied directory, or an error.</returns>
    public ErrorOr<Directory> CopyDirectory(FileSystemPathId sourcePath, FileSystemPathId destinationPath, bool overrideExisting)
    {
        var directoryExists = _environmentContext.DirectoryProviderService.DirectoryExists(sourcePath);
        if (directoryExists.IsError)
            return directoryExists.Errors;
        else if (directoryExists.Value == false)
            return Errors.FileManagement.DirectoryNotFoundError;
        else
        {
            // copy the directory
            ErrorOr<FileSystemPathId> newDirectory = _environmentContext.DirectoryProviderService.CopyDirectory(sourcePath, destinationPath, overrideExisting);

            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Moves a directory located at <paramref name="sourcePath"/> to <paramref name="destinationPath"/>.
    /// </summary>
    /// <param name="sourcePath">String representation of the path where the directory to be moved is located.</param>
    /// <param name="destinationPath">String representation of the path where the directory will be moved.</param>
    /// <param name="overrideExisting">Whether to override existing directories, or not.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing either a moved directory, or an error.</returns>
    public ErrorOr<Directory> MoveDirectory(string sourcePath, string destinationPath, bool? overrideExisting)
    {
        // make sure the paths are in the expected format
        if (!sourcePath.EndsWith(_platformContext.PathStrategy.PathSeparator))
            sourcePath += _platformContext.PathStrategy.PathSeparator;
        if (!destinationPath.EndsWith(_platformContext.PathStrategy.PathSeparator))
            destinationPath += _platformContext.PathStrategy.PathSeparator;
        var fileSystemSourcePathIdResult = FileSystemPathId.Create(sourcePath);
        if (fileSystemSourcePathIdResult.IsError)
            return fileSystemSourcePathIdResult.Errors;
        var fileSystemDestinationPathIdResult = FileSystemPathId.Create(destinationPath);
        if (fileSystemDestinationPathIdResult.IsError)
            return fileSystemDestinationPathIdResult.Errors;
        return MoveDirectory(fileSystemSourcePathIdResult.Value, fileSystemDestinationPathIdResult.Value, overrideExisting ?? false);
    }

    /// <summary>
    /// Moves a directory for the specified path.
    /// </summary>
    /// <param name="sourcePath">Identifier for the path where the directory to be moved is located.</param>
    /// <param name="destinationPath">Identifier for the path where the directory will be moved.</param>
    /// <param name="overrideExisting">Whether to override existing directories, or not.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing either the moved directory, or an error.</returns>
    public ErrorOr<Directory> MoveDirectory(FileSystemPathId sourcePath, FileSystemPathId destinationPath, bool overrideExisting)
    {
        var directoryExists = _environmentContext.DirectoryProviderService.DirectoryExists(sourcePath);
        if (directoryExists.IsError)
            return directoryExists.Errors;
        else if (directoryExists.Value == false)
            return Errors.FileManagement.DirectoryNotFoundError;
        else
        {
            // move the directory
            ErrorOr<FileSystemPathId> newDirectory = _environmentContext.DirectoryProviderService.MoveDirectory(sourcePath, destinationPath, overrideExisting);

            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Renames a directory.
    /// </summary>
    /// <param name="path">String representation of the directory path.</param>
    /// <param name="name">The new name of the directory.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing either the renamed directory, or an error.</returns>
    public ErrorOr<Directory> RenameDirectory(string path, string name)
    {
        var fileSystemPathIdResult = FileSystemPathId.Create(path);
        if (fileSystemPathIdResult.IsError)
            return fileSystemPathIdResult.Errors;
        return RenameDirectory(fileSystemPathIdResult.Value, name);
    }

    /// <summary>
    /// Renames a directory for the specified path.
    /// </summary>
    /// <param name="path">Identifier for the directory path.</param>
    /// <param name="name">The new name of the directory.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing either the renamed directory, or an error.</returns>
    public ErrorOr<Directory> RenameDirectory(FileSystemPathId path, string name)
    {
        // first, check if the directory about to be created does not already exist
        var combinedPath = _platformContext.PathStrategy.CombinePath(path, name);
        if (combinedPath.IsError)
            return combinedPath.Errors;
        var directoryExists = _environmentContext.DirectoryProviderService.DirectoryExists(combinedPath.Value);
        if (directoryExists.IsError)
            return directoryExists.Errors;
        else if (directoryExists.Value == true)
            return Errors.FileManagement.DirectoryAlreadyExistsError;
        else
        {
            // rename the directory
            ErrorOr<FileSystemPathId> newDirectoryPathResult = _environmentContext.DirectoryProviderService.RenameDirectory(path, name);
            if (newDirectoryPathResult.IsError)
                return newDirectoryPathResult.Errors;
            ErrorOr<string> dirNameResult = _environmentContext.DirectoryProviderService.GetFileName(newDirectoryPathResult.Value);
            ErrorOr<DateTime?> dateModifiedResult = _environmentContext.DirectoryProviderService.GetLastWriteTime(newDirectoryPathResult.Value);
            ErrorOr<DateTime?> dateCreatedResult = _environmentContext.DirectoryProviderService.GetCreationTime(newDirectoryPathResult.Value);
            // if any error occurred, mark directory as Inaccessible
            if (dirNameResult.IsError || dateModifiedResult.IsError || dateCreatedResult.IsError)
            {
                Directory errorDir = new(newDirectoryPathResult.Value, !dirNameResult.IsError ? dirNameResult.Value : null!,
                    !dateCreatedResult.IsError ? dateCreatedResult.Value : null, !dateModifiedResult.IsError ? dateModifiedResult.Value : null);
                errorDir.SetStatus(FileSystemItemStatus.Inaccessible);
                return errorDir;
            }
            else
                return new Directory(newDirectoryPathResult.Value, dirNameResult.Value, dateCreatedResult.Value, dateModifiedResult.Value);
        }
    }

    /// <summary>
    /// Delete a directory for the specified string path.
    /// </summary>
    /// <param name="path">String representation of the directory path.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing either the result of deleting a directory, or an error.</returns>
    public ErrorOr<bool> DeleteDirectory(string path)
    {
        var fileSystemPathIdResult = FileSystemPathId.Create(path);
        if (fileSystemPathIdResult.IsError)
            return fileSystemPathIdResult.Errors;
        return DeleteDirectory(fileSystemPathIdResult.Value);
    }

    /// <summary>
    /// Delete a directory for the specified path.
    /// </summary>
    /// <param name="path">Identifier for the directory path.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing either the result of deleting a directory, or an error.</returns>
    public ErrorOr<bool> DeleteDirectory(FileSystemPathId path)
    {
        return _environmentContext.DirectoryProviderService.DeleteDirectory(path);
    }
    #endregion
}