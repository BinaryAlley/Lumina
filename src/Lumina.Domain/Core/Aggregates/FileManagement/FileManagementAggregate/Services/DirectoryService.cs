#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Contracts.Enums.FileSystem;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.Entities;
using Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.Strategies.Environment;
using Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.Strategies.Platform;
using Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.ValueObjects;
using System;
using System.Collections.Generic;
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
    /// Initializes a new instance of the <see cref="DirectoryService"/> class.
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
    public ErrorOr<IEnumerable<Directory>> GetSubdirectories(string path)
    {
        ErrorOr<FileSystemPathId> fileSystemPathIdResult = FileSystemPathId.Create(path);
        if (fileSystemPathIdResult.IsError)
            return fileSystemPathIdResult.Errors;
        return GetSubdirectories(fileSystemPathIdResult.Value);
    }

    /// <summary>
    /// Retrieves subdirectories for the given directory.
    /// </summary>
    /// <param name="directory">Directory object to retrieve subdirectories for.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing either a collection of subdirectories or an error.</returns>
    public ErrorOr<IEnumerable<Directory>> GetSubdirectories(Directory directory)
    {
        return GetSubdirectories(directory.Id);
    }

    /// <summary>
    /// Retrieves subdirectories for the specified file system path.
    /// </summary>
    /// <param name="path">Identifier for the file path.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing either a collection of subdirectories or an error.</returns>
    public ErrorOr<IEnumerable<Directory>> GetSubdirectories(FileSystemPathId path)
    {
        // retrieve the list of subdirectories
        ErrorOr<IEnumerable<FileSystemPathId>> subdirectoryPathsResult = _environmentContext.DirectoryProviderService.GetSubdirectoryPaths(path);
        if (subdirectoryPathsResult.IsError)
            return subdirectoryPathsResult.Errors;
        List<Directory> result = [];
        IEnumerable<FileSystemPathId> subdirectoryPaths = subdirectoryPathsResult.Value;
        foreach (FileSystemPathId subPath in subdirectoryPaths)
        {
            // extract directory details
            ErrorOr<string> dirNameResult = _environmentContext.DirectoryProviderService.GetFileName(subPath);
            ErrorOr<Optional<DateTime>> dateModifiedResult = _environmentContext.DirectoryProviderService.GetLastWriteTime(subPath);
            ErrorOr<Optional<DateTime>> dateCreatedResult = _environmentContext.DirectoryProviderService.GetCreationTime(subPath);

            // if any error occurred, mark directory as Inaccessible
            if (dirNameResult.IsError || dateModifiedResult.IsError || dateCreatedResult.IsError)
            {
                ErrorOr<Directory> errorDirResult = Directory.Create(subPath, !dirNameResult.IsError ? dirNameResult.Value : null!,
                    !dateCreatedResult.IsError ? dateCreatedResult.Value : Optional<DateTime>.None(), 
                    !dateModifiedResult.IsError ? dateModifiedResult.Value : Optional<DateTime>.None());
                if(errorDirResult.IsError)
                    return errorDirResult.Errors;
                ErrorOr<Updated> setStatusResult = errorDirResult.Value.SetStatus(FileSystemItemStatus.Inaccessible);
                if (setStatusResult.IsError)
                    return setStatusResult.Errors;
                result.Add(errorDirResult.Value);
            }
            else
            {
                ErrorOr<Directory> subDirectoryResult = Directory.Create(subPath, dirNameResult.Value, dateCreatedResult.Value, dateModifiedResult.Value);
                if (subDirectoryResult.IsError)
                    return subDirectoryResult.Errors;
                result.Add(subDirectoryResult.Value);
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
        ErrorOr<FileSystemPathId> fileSystemPathIdResult = FileSystemPathId.Create(path);
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
        ErrorOr<FileSystemPathId> combinedPath = _platformContext.PathStrategy.CombinePath(path, name);
        if (combinedPath.IsError)
            return combinedPath.Errors;
        ErrorOr<bool> directoryExists = _environmentContext.DirectoryProviderService.DirectoryExists(combinedPath.Value);
        if (directoryExists.IsError)
            return directoryExists.Errors;
        else if (directoryExists.Value == true)
            return Errors.FileManagement.DirectoryAlreadyExists;
        else
        {
            // create the new directory
            ErrorOr<FileSystemPathId> newDirectoryPathResult = _environmentContext.DirectoryProviderService.CreateDirectory(path, name);
            if (newDirectoryPathResult.IsError)
                return newDirectoryPathResult.Errors;
            ErrorOr<string> dirNameResult = _environmentContext.DirectoryProviderService.GetFileName(path);
            ErrorOr<Optional<DateTime>> dateModifiedResult = _environmentContext.DirectoryProviderService.GetLastWriteTime(path);
            ErrorOr<Optional<DateTime>> dateCreatedResult = _environmentContext.DirectoryProviderService.GetCreationTime(path);
            // if any error occurred, mark directory as Inaccessible
            if (dirNameResult.IsError || dateModifiedResult.IsError || dateCreatedResult.IsError)
            {
                ErrorOr<Directory> errorDirResult = Directory.Create(newDirectoryPathResult.Value, !dirNameResult.IsError ? dirNameResult.Value : null!,
                    !dateCreatedResult.IsError ? dateCreatedResult.Value : Optional<DateTime>.None(), 
                    !dateModifiedResult.IsError ? dateModifiedResult.Value : Optional<DateTime>.None());
                if (errorDirResult.IsError)
                    return errorDirResult.Errors;
                ErrorOr<Updated> setStatusResult = errorDirResult.Value.SetStatus(FileSystemItemStatus.Inaccessible);
                if (setStatusResult.IsError)
                    return setStatusResult.Errors;
                return errorDirResult;
            }
            else
                return Directory.Create(newDirectoryPathResult.Value, dirNameResult.Value, dateCreatedResult.Value, dateModifiedResult.Value);
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
        ErrorOr<FileSystemPathId> fileSystemSourcePathIdResult = FileSystemPathId.Create(sourcePath);
        if (fileSystemSourcePathIdResult.IsError)
            return fileSystemSourcePathIdResult.Errors;
        ErrorOr<FileSystemPathId> fileSystemDestinationPathIdResult = FileSystemPathId.Create(destinationPath);
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
        ErrorOr<bool> directoryExists = _environmentContext.DirectoryProviderService.DirectoryExists(sourcePath);
        if (directoryExists.IsError)
            return directoryExists.Errors;
        else if (directoryExists.Value == false)
            return Errors.FileManagement.DirectoryNotFound;
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
        ErrorOr<FileSystemPathId> fileSystemSourcePathIdResult = FileSystemPathId.Create(sourcePath);
        if (fileSystemSourcePathIdResult.IsError)
            return fileSystemSourcePathIdResult.Errors;
        ErrorOr<FileSystemPathId> fileSystemDestinationPathIdResult = FileSystemPathId.Create(destinationPath);
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
        ErrorOr<bool> directoryExists = _environmentContext.DirectoryProviderService.DirectoryExists(sourcePath);
        if (directoryExists.IsError)
            return directoryExists.Errors;
        else if (directoryExists.Value == false)
            return Errors.FileManagement.DirectoryNotFound;
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
        ErrorOr<FileSystemPathId> fileSystemPathIdResult = FileSystemPathId.Create(path);
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
        ErrorOr<FileSystemPathId> combinedPath = _platformContext.PathStrategy.CombinePath(path, name);
        if (combinedPath.IsError)
            return combinedPath.Errors;
        ErrorOr<bool> directoryExists = _environmentContext.DirectoryProviderService.DirectoryExists(combinedPath.Value);
        if (directoryExists.IsError)
            return directoryExists.Errors;
        else if (directoryExists.Value == true)
            return Errors.FileManagement.DirectoryAlreadyExists;
        else
        {
            // rename the directory
            ErrorOr<FileSystemPathId> newDirectoryPathResult = _environmentContext.DirectoryProviderService.RenameDirectory(path, name);
            if (newDirectoryPathResult.IsError)
                return newDirectoryPathResult.Errors;
            ErrorOr<string> dirNameResult = _environmentContext.DirectoryProviderService.GetFileName(newDirectoryPathResult.Value);
            ErrorOr<Optional<DateTime>> dateModifiedResult = _environmentContext.DirectoryProviderService.GetLastWriteTime(newDirectoryPathResult.Value);
            ErrorOr<Optional<DateTime>> dateCreatedResult = _environmentContext.DirectoryProviderService.GetCreationTime(newDirectoryPathResult.Value);
            // if any error occurred, mark directory as Inaccessible
            if (dirNameResult.IsError || dateModifiedResult.IsError || dateCreatedResult.IsError)
            {
                ErrorOr<Directory> errorDirResult = Directory.Create(newDirectoryPathResult.Value, !dirNameResult.IsError ? dirNameResult.Value : null!,
                    !dateCreatedResult.IsError ? dateCreatedResult.Value : Optional<DateTime>.None(), 
                    !dateModifiedResult.IsError ? dateModifiedResult.Value : Optional<DateTime>.None());
                if (errorDirResult.IsError)
                    return errorDirResult.Errors;
                ErrorOr<Updated> setStatusResult = errorDirResult.Value.SetStatus(FileSystemItemStatus.Inaccessible);
                if (setStatusResult.IsError)
                    return setStatusResult.Errors;
                return errorDirResult;
            }
            else
                return Directory.Create(newDirectoryPathResult.Value, dirNameResult.Value, dateCreatedResult.Value, dateModifiedResult.Value);
        }
    }

    /// <summary>
    /// Delete a directory for the specified string path.
    /// </summary>
    /// <param name="path">String representation of the directory path.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing either the result of deleting a directory, or an error.</returns>
    public ErrorOr<Deleted> DeleteDirectory(string path)
    {
        ErrorOr<FileSystemPathId> fileSystemPathIdResult = FileSystemPathId.Create(path);
        if (fileSystemPathIdResult.IsError)
            return fileSystemPathIdResult.Errors;
        return DeleteDirectory(fileSystemPathIdResult.Value);
    }

    /// <summary>
    /// Delete a directory for the specified path.
    /// </summary>
    /// <param name="path">Identifier for the directory path.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing either the result of deleting a directory, or an error.</returns>
    public ErrorOr<Deleted> DeleteDirectory(FileSystemPathId path)
    {
        return _environmentContext.DirectoryProviderService.DeleteDirectory(path);
    }
    #endregion
}