#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.FileSystem;
using Lumina.Domain.Common.Errors;

using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Strategies.Environment;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Strategies.Platform;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Entities;
using System;
using System.Collections.Generic;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Services;

/// <summary>
/// Service for handling directories.
/// </summary>
public class DirectoryService : IDirectoryService
{
    private readonly IPlatformContext _platformContext;
    private readonly IEnvironmentContext _environmentContext;

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

    /// <summary>
    /// Retrieves subdirectories for the specified string path.
    /// </summary>
    /// <param name="path">String representation of the file path.</param>
    /// <param name="includeHiddenElements">Whether to include hidden subdirectories or not.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either a collection of subdirectories or an error.</returns>
    public Result<IEnumerable<Directory>> GetSubdirectories(string path, bool includeHiddenElements)
    {
        Result<FileSystemPathId> fileSystemPathIdResult = FileSystemPathId.Create(path);
        if (fileSystemPathIdResult.IsFailure)
            return fileSystemPathIdResult.Errors;
        return GetSubdirectories(fileSystemPathIdResult.Value, includeHiddenElements);
    }

    /// <summary>
    /// Retrieves subdirectories for the given directory.
    /// </summary>
    /// <param name="directory">Directory object to retrieve subdirectories for.</param>
    /// <param name="includeHiddenElements">Whether to include hidden subdirectories or not.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either a collection of subdirectories or an error.</returns>
    public Result<IEnumerable<Directory>> GetSubdirectories(Directory directory, bool includeHiddenElements)
    {
        return GetSubdirectories(directory.Id, includeHiddenElements);
    }

    /// <summary>
    /// Retrieves subdirectories for the specified file system path.
    /// </summary>
    /// <param name="path">Identifier for the file path.</param>
    /// <param name="includeHiddenElements">Whether to include hidden subdirectories or not.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either a collection of subdirectories or an error.</returns>
    public Result<IEnumerable<Directory>> GetSubdirectories(FileSystemPathId path, bool includeHiddenElements)
    {
        // retrieve the list of subdirectories
        Result<IEnumerable<FileSystemPathId>> subdirectoryPathsResult = _environmentContext.DirectoryProviderService.GetSubdirectoryPaths(path, includeHiddenElements);
        if (subdirectoryPathsResult.IsFailure)
            return subdirectoryPathsResult.Errors;
        List<Directory> result = [];
        foreach (FileSystemPathId subPath in subdirectoryPathsResult.Value)
        {
            // extract directory details
            Result<string> dirNameResult = _environmentContext.DirectoryProviderService.GetFileName(subPath);
            Result<Optional<DateTime>> dateModifiedResult = _environmentContext.DirectoryProviderService.GetLastWriteTime(subPath);
            Result<Optional<DateTime>> dateCreatedResult = _environmentContext.DirectoryProviderService.GetCreationTime(subPath);

            // if any error occurred, mark directory as Inaccessible
            if (dirNameResult.IsFailure || dateModifiedResult.IsFailure || dateCreatedResult.IsFailure)
            {
                Result<Directory> errorDirResult = Directory.Create(subPath, !dirNameResult.IsFailure ? dirNameResult.Value : null!,
                    !dateCreatedResult.IsFailure ? dateCreatedResult.Value : Optional<DateTime>.None(),
                    !dateModifiedResult.IsFailure ? dateModifiedResult.Value : Optional<DateTime>.None());
                if (errorDirResult.IsFailure)
                    return errorDirResult.Errors;
                Result<Updated> setStatusResult = errorDirResult.Value.SetStatus(FileSystemItemStatus.Inaccessible);
                if (setStatusResult.IsFailure)
                    return setStatusResult.Errors;
                result.Add(errorDirResult.Value);
            }
            else
            {
                Result<Directory> subDirectoryResult = Directory.Create(subPath, dirNameResult.Value, dateCreatedResult.Value, dateModifiedResult.Value);
                if (subDirectoryResult.IsFailure)
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
    /// <returns>An <see cref="Result{TValue}"/> containing either the result of creating a directory, or an error.</returns>
    public Result<Directory> CreateDirectory(string path, string name)
    {
        Result<FileSystemPathId> fileSystemPathIdResult = FileSystemPathId.Create(path);
        if (fileSystemPathIdResult.IsFailure)
            return fileSystemPathIdResult.Errors;
        return CreateDirectory(fileSystemPathIdResult.Value, name);
    }

    /// <summary>
    /// Creates a directory with the specified <paramref name="name"/>, at the specified <paramref name="path"/>.
    /// </summary>
    /// <param name="path">Identifier for the path where the directory will be created.</param>
    /// <param name="name">The name of the directory that will be created.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the result of creating a directory, or an error.</returns>
    public Result<Directory> CreateDirectory(FileSystemPathId path, string name)
    {
        // first, check if the directory about to be created does not already exist
        Result<FileSystemPathId> combinedPath = _platformContext.PathStrategy.CombinePath(path, name);
        if (combinedPath.IsFailure)
            return combinedPath.Errors;
        Result<bool> directoryExists = _environmentContext.DirectoryProviderService.DirectoryExists(combinedPath.Value);
        if (directoryExists.IsFailure)
            return directoryExists.Errors;
        else if (directoryExists.Value == true)
            return Errors.FileSystemManagement.DirectoryAlreadyExists;
        else
        {
            // create the new directory
            Result<FileSystemPathId> newDirectoryPathResult = _environmentContext.DirectoryProviderService.CreateDirectory(path, name);
            if (newDirectoryPathResult.IsFailure)
                return newDirectoryPathResult.Errors;
            Result<string> dirNameResult = _environmentContext.DirectoryProviderService.GetFileName(newDirectoryPathResult.Value);
            Result<Optional<DateTime>> dateModifiedResult = _environmentContext.DirectoryProviderService.GetLastWriteTime(newDirectoryPathResult.Value);
            Result<Optional<DateTime>> dateCreatedResult = _environmentContext.DirectoryProviderService.GetCreationTime(newDirectoryPathResult.Value);
            // if any error occurred, mark directory as Inaccessible
            if (dirNameResult.IsFailure || dateModifiedResult.IsFailure || dateCreatedResult.IsFailure)
            {
                Result<Directory> errorDirResult = Directory.Create(newDirectoryPathResult.Value, !dirNameResult.IsFailure ? dirNameResult.Value : null!,
                    !dateCreatedResult.IsFailure ? dateCreatedResult.Value : Optional<DateTime>.None(),
                    !dateModifiedResult.IsFailure ? dateModifiedResult.Value : Optional<DateTime>.None());
                if (errorDirResult.IsFailure)
                    return errorDirResult.Errors;
                Result<Updated> setStatusResult = errorDirResult.Value.SetStatus(FileSystemItemStatus.Inaccessible);
                if (setStatusResult.IsFailure)
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
    /// <returns>An <see cref="Result{TValue}"/> containing either a copied directory, or an error.</returns>
    public Result<Directory> CopyDirectory(string sourcePath, string destinationPath, bool? overrideExisting)
    {
        // make sure the paths are in the expected format
        if (!sourcePath.EndsWith(_platformContext.PathStrategy.PathSeparator))
            sourcePath += _platformContext.PathStrategy.PathSeparator;
        if (!destinationPath.EndsWith(_platformContext.PathStrategy.PathSeparator))
            destinationPath += _platformContext.PathStrategy.PathSeparator;
        Result<FileSystemPathId> fileSystemSourcePathIdResult = FileSystemPathId.Create(sourcePath);
        if (fileSystemSourcePathIdResult.IsFailure)
            return fileSystemSourcePathIdResult.Errors;
        Result<FileSystemPathId> fileSystemDestinationPathIdResult = FileSystemPathId.Create(destinationPath);
        if (fileSystemDestinationPathIdResult.IsFailure)
            return fileSystemDestinationPathIdResult.Errors;
        return CopyDirectory(fileSystemSourcePathIdResult.Value, fileSystemDestinationPathIdResult.Value, overrideExisting ?? false);
    }

    /// <summary>
    /// Copies a directory located at <paramref name="sourcePath"/> to <paramref name="destinationPath"/>.
    /// </summary>
    /// <param name="sourcePath">Identifier for the path where the directory to be copied is located.</param>
    /// <param name="destinationPath">Identifier for the path where the directory will be copied.</param>
    /// <param name="overrideExisting">Whether to override existing directories, or not.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the copied directory, or an error.</returns>
    public Result<Directory> CopyDirectory(FileSystemPathId sourcePath, FileSystemPathId destinationPath, bool overrideExisting)
    {
        Result<bool> directoryExists = _environmentContext.DirectoryProviderService.DirectoryExists(sourcePath);
        if (directoryExists.IsFailure)
            return directoryExists.Errors;
        else if (directoryExists.Value == false)
            return Errors.FileSystemManagement.DirectoryNotFound;
        else
        {
            // copy the directory
            Result<FileSystemPathId> newDirectory = _environmentContext.DirectoryProviderService.CopyDirectory(sourcePath, destinationPath, overrideExisting);

            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Moves a directory located at <paramref name="sourcePath"/> to <paramref name="destinationPath"/>.
    /// </summary>
    /// <param name="sourcePath">String representation of the path where the directory to be moved is located.</param>
    /// <param name="destinationPath">String representation of the path where the directory will be moved.</param>
    /// <param name="overrideExisting">Whether to override existing directories, or not.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either a moved directory, or an error.</returns>
    public Result<Directory> MoveDirectory(string sourcePath, string destinationPath, bool? overrideExisting)
    {
        // make sure the paths are in the expected format
        if (!sourcePath.EndsWith(_platformContext.PathStrategy.PathSeparator))
            sourcePath += _platformContext.PathStrategy.PathSeparator;
        if (!destinationPath.EndsWith(_platformContext.PathStrategy.PathSeparator))
            destinationPath += _platformContext.PathStrategy.PathSeparator;
        Result<FileSystemPathId> fileSystemSourcePathIdResult = FileSystemPathId.Create(sourcePath);
        if (fileSystemSourcePathIdResult.IsFailure)
            return fileSystemSourcePathIdResult.Errors;
        Result<FileSystemPathId> fileSystemDestinationPathIdResult = FileSystemPathId.Create(destinationPath);
        if (fileSystemDestinationPathIdResult.IsFailure)
            return fileSystemDestinationPathIdResult.Errors;
        return MoveDirectory(fileSystemSourcePathIdResult.Value, fileSystemDestinationPathIdResult.Value, overrideExisting ?? false);
    }

    /// <summary>
    /// Moves a directory for the specified path.
    /// </summary>
    /// <param name="sourcePath">Identifier for the path where the directory to be moved is located.</param>
    /// <param name="destinationPath">Identifier for the path where the directory will be moved.</param>
    /// <param name="overrideExisting">Whether to override existing directories, or not.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the moved directory, or an error.</returns>
    public Result<Directory> MoveDirectory(FileSystemPathId sourcePath, FileSystemPathId destinationPath, bool overrideExisting)
    {
        Result<bool> directoryExists = _environmentContext.DirectoryProviderService.DirectoryExists(sourcePath);
        if (directoryExists.IsFailure)
            return directoryExists.Errors;
        else if (directoryExists.Value == false)
            return Errors.FileSystemManagement.DirectoryNotFound;
        else
        {
            // move the directory
            Result<FileSystemPathId> newDirectory = _environmentContext.DirectoryProviderService.MoveDirectory(sourcePath, destinationPath, overrideExisting);

            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Renames a directory.
    /// </summary>
    /// <param name="path">String representation of the directory path.</param>
    /// <param name="name">The new name of the directory.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the renamed directory, or an error.</returns>
    public Result<Directory> RenameDirectory(string path, string name)
    {
        Result<FileSystemPathId> fileSystemPathIdResult = FileSystemPathId.Create(path);
        if (fileSystemPathIdResult.IsFailure)
            return fileSystemPathIdResult.Errors;
        return RenameDirectory(fileSystemPathIdResult.Value, name);
    }

    /// <summary>
    /// Renames a directory for the specified path.
    /// </summary>
    /// <param name="path">Identifier for the directory path.</param>
    /// <param name="name">The new name of the directory.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the renamed directory, or an error.</returns>
    public Result<Directory> RenameDirectory(FileSystemPathId path, string name)
    {
        // first, check if the directory about to be created does not already exist
        Result<FileSystemPathId> combinedPath = _platformContext.PathStrategy.CombinePath(path, name);
        if (combinedPath.IsFailure)
            return combinedPath.Errors;
        Result<bool> directoryExists = _environmentContext.DirectoryProviderService.DirectoryExists(combinedPath.Value);
        if (directoryExists.IsFailure)
            return directoryExists.Errors;
        else if (directoryExists.Value == true)
            return Errors.FileSystemManagement.DirectoryAlreadyExists;
        else
        {
            // rename the directory
            Result<FileSystemPathId> newDirectoryPathResult = _environmentContext.DirectoryProviderService.RenameDirectory(path, name);
            if (newDirectoryPathResult.IsFailure)
                return newDirectoryPathResult.Errors;
            Result<string> dirNameResult = _environmentContext.DirectoryProviderService.GetFileName(newDirectoryPathResult.Value);
            Result<Optional<DateTime>> dateModifiedResult = _environmentContext.DirectoryProviderService.GetLastWriteTime(newDirectoryPathResult.Value);
            Result<Optional<DateTime>> dateCreatedResult = _environmentContext.DirectoryProviderService.GetCreationTime(newDirectoryPathResult.Value);
            // if any error occurred, mark directory as Inaccessible
            if (dirNameResult.IsFailure || dateModifiedResult.IsFailure || dateCreatedResult.IsFailure)
            {
                Result<Directory> errorDirResult = Directory.Create(newDirectoryPathResult.Value, !dirNameResult.IsFailure ? dirNameResult.Value : null!,
                    !dateCreatedResult.IsFailure ? dateCreatedResult.Value : Optional<DateTime>.None(),
                    !dateModifiedResult.IsFailure ? dateModifiedResult.Value : Optional<DateTime>.None());
                if (errorDirResult.IsFailure)
                    return errorDirResult.Errors;
                Result<Updated> setStatusResult = errorDirResult.Value.SetStatus(FileSystemItemStatus.Inaccessible);
                if (setStatusResult.IsFailure)
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
    /// <returns>An <see cref="Result{TValue}"/> containing either the result of deleting a directory, or an error.</returns>
    public Result<Deleted> DeleteDirectory(string path)
    {
        Result<FileSystemPathId> fileSystemPathIdResult = FileSystemPathId.Create(path);
        if (fileSystemPathIdResult.IsFailure)
            return fileSystemPathIdResult.Errors;
        return DeleteDirectory(fileSystemPathIdResult.Value);
    }

    /// <summary>
    /// Delete a directory for the specified path.
    /// </summary>
    /// <param name="path">Identifier for the directory path.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the result of deleting a directory, or an error.</returns>
    public Result<Deleted> DeleteDirectory(FileSystemPathId path)
    {
        return _environmentContext.DirectoryProviderService.DeleteDirectory(path);
    }
}
