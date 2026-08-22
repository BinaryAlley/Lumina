#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Entities;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Strategies.Environment;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Strategies.Platform;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.FileSystem;
using System;
using System.Collections.Generic;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Services;

/// <summary>
/// Service for handling file system files.
/// </summary>
public class FileService : IFileService
{
    private readonly IPlatformContext _platformContext;
    private readonly IEnvironmentContext _environmentContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileService"/> class.
    /// </summary>
    /// <param name="environmentContext">Injected facade service for environment contextual services.</param>
    /// <param name="platformContextManager">Injected facade service for platform contextual services.</param>
    public FileService(IEnvironmentContext environmentContext, IPlatformContextManager platformContextManager)
    {
        _platformContext = platformContextManager.GetCurrentContext();
        _environmentContext = environmentContext;
    }

    /// <summary>
    /// Retrieves files for the specified string path.
    /// </summary>
    /// <param name="path">String representation of the file path.</param>
    /// <param name="includeHiddenElements">Whether to include hidden files or not.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either a collection of files or an error.</returns>
    public Result<IEnumerable<File>> GetFiles(string path, bool includeHiddenElements)
    {
        Result<FileSystemPathId> fileSystemPathIdResult = FileSystemPathId.Create(path);
        if (fileSystemPathIdResult.IsFailure)
            return fileSystemPathIdResult.Errors;
        return GetFiles(fileSystemPathIdResult.Value, includeHiddenElements);
    }

    /// <summary>
    /// Retrieves files associated with a given file.
    /// </summary>
    /// <param name="file">The file object.</param>
    /// <param name="includeHiddenElements">Whether to include hidden files or not.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either a collection of files or an error.</returns>
    public Result<IEnumerable<File>> GetFiles(File file, bool includeHiddenElements)
    {
        return GetFiles(file.Id, includeHiddenElements);
    }

    /// <summary>
    /// Retrieves files for a specified file path Id.
    /// </summary>
    /// <param name="path">Identifier for the file path.</param>
    /// <param name="includeHiddenElements">Whether to include hidden files or not.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either a collection of files or an error.</returns>
    public Result<IEnumerable<File>> GetFiles(FileSystemPathId path, bool includeHiddenElements)
    {
        // retrieve the list of files
        Result<IEnumerable<FileSystemPathId>> filePathsResult = _environmentContext.FileProviderService.GetFilePaths(path, includeHiddenElements);
        if (filePathsResult.IsFailure)
            return filePathsResult.Errors;
        List<File> result = [];
        IEnumerable<FileSystemPathId> filePaths = filePathsResult.Value;
        foreach (FileSystemPathId filePath in filePaths)
        {
            // extract file details and add to the result list
            Result<string> fileNameResult = _environmentContext.FileProviderService.GetFileName(filePath);
            Result<Optional<DateTime>> dateModifiedResult = _environmentContext.FileProviderService.GetLastWriteTime(filePath);
            Result<Optional<DateTime>> dateCreatedResult = _environmentContext.FileProviderService.GetCreationTime(filePath);
            Result<long?> sizeResult = _environmentContext.FileProviderService.GetSize(filePath);
            long size = !sizeResult.IsFailure ? sizeResult.Value ?? 0 : 0;
            // if any of the details returned an error, set inaccessible status
            if (fileNameResult.IsFailure || dateModifiedResult.IsFailure || dateCreatedResult.IsFailure || sizeResult.IsFailure)
            {
                Result<File> errorFileResult = File.Create(filePath, !fileNameResult.IsFailure ? fileNameResult.Value : null!,
                    !dateCreatedResult.IsFailure ? dateCreatedResult.Value : Optional<DateTime>.None(),
                    !dateModifiedResult.IsFailure ? dateModifiedResult.Value : Optional<DateTime>.None(), size);
                if (errorFileResult.IsFailure)
                    return errorFileResult.Errors;
                Result<Updated> setStatusResult = errorFileResult.Value.SetStatus(FileSystemItemStatus.Inaccessible);
                if (setStatusResult.IsFailure)
                    return setStatusResult.Errors;
                result.Add(errorFileResult.Value);
            }
            else
            {
                Result<File> fileResult = File.Create(filePath, fileNameResult.Value, dateCreatedResult.Value, dateModifiedResult.Value, size);
                if (fileResult.IsFailure)
                    return fileResult.Errors;
                result.Add(fileResult.Value);
            }
        }
        return result;
    }

    /// <summary>
    /// Copies a file located at <paramref name="sourceFilePath"/> to <paramref name="destinationDirectoryPath"/>.
    /// </summary>
    /// <param name="sourceFilePath">String representation of the path where the file to be copied is located.</param>
    /// <param name="destinationDirectoryPath">String representation of the path of the directory where the file will be copied.</param>
    /// <param name="overrideExisting">Whether to override existing files, or not.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either a copied file, or an error.</returns>
    public Result<File> CopyFile(string sourceFilePath, string destinationDirectoryPath, bool? overrideExisting)
    {
        if (string.IsNullOrWhiteSpace(destinationDirectoryPath))
            return Errors.FileSystemManagement.InvalidPath;
        // make sure the paths are in the expected format
        if (!destinationDirectoryPath.EndsWith(_platformContext.PathStrategy.PathSeparator))
            destinationDirectoryPath += _platformContext.PathStrategy.PathSeparator;
        Result<FileSystemPathId> fileSystemSourcePathIdResult = FileSystemPathId.Create(sourceFilePath);
        if (fileSystemSourcePathIdResult.IsFailure)
            return fileSystemSourcePathIdResult.Errors;
        Result<FileSystemPathId> fileSystemDestinationPathIdResult = FileSystemPathId.Create(destinationDirectoryPath);
        if (fileSystemDestinationPathIdResult.IsFailure)
            return fileSystemDestinationPathIdResult.Errors;
        return CopyFile(fileSystemSourcePathIdResult.Value, fileSystemDestinationPathIdResult.Value, overrideExisting ?? false);
    }

    /// <summary>
    /// Copies a file for the specified path.
    /// </summary>
    /// <param name="sourceFilePath">Identifier for the path where the file to be copied is located.</param>
    /// <param name="destinationDirectoryPath">Identifier for the path of the directory where the file will be copied.</param>
    /// <param name="overrideExisting">Whether to override existing files, or not.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the copied file, or an error.</returns>
    public Result<File> CopyFile(FileSystemPathId sourceFilePath, FileSystemPathId destinationDirectoryPath, bool overrideExisting)
    {
        Result<bool> fileExists = _environmentContext.FileProviderService.FileExists(sourceFilePath);
        if (fileExists.IsFailure)
            return fileExists.Errors;
        else if (fileExists.Value == false)
            return Errors.FileSystemManagement.FileNotFound;
        else
        {
            // copy the file
            Result<FileSystemPathId> copyFileResult = _environmentContext.FileProviderService.CopyFile(sourceFilePath, destinationDirectoryPath, overrideExisting);
            if (copyFileResult.IsFailure)
                return copyFileResult.Errors;
            Result<string> fileNameResult = _environmentContext.FileProviderService.GetFileName(copyFileResult.Value);
            Result<Optional<DateTime>> dateModifiedResult = _environmentContext.FileProviderService.GetLastWriteTime(copyFileResult.Value);
            Result<Optional<DateTime>> dateCreatedResult = _environmentContext.FileProviderService.GetCreationTime(copyFileResult.Value);
            Result<long?> sizeResult = _environmentContext.FileProviderService.GetSize(copyFileResult.Value);
            long size = !sizeResult.IsFailure ? sizeResult.Value ?? 0 : 0;
            // if any of the details returned an error, set inaccessible status
            if (fileNameResult.IsFailure || dateModifiedResult.IsFailure || dateCreatedResult.IsFailure || sizeResult.IsFailure)
            {
                Result<File> errorFileResult = File.Create(copyFileResult.Value, !fileNameResult.IsFailure ? fileNameResult.Value : null!,
                    !dateCreatedResult.IsFailure ? dateCreatedResult.Value : Optional<DateTime>.None(),
                    !dateModifiedResult.IsFailure ? dateModifiedResult.Value : Optional<DateTime>.None(), size);
                if (errorFileResult.IsFailure)
                    return errorFileResult.Errors;
                Result<Updated> setStatusResult = errorFileResult.Value.SetStatus(FileSystemItemStatus.Inaccessible);
                if (setStatusResult.IsFailure)
                    return setStatusResult.Errors;
                return errorFileResult;
            }
            else
                return File.Create(copyFileResult.Value, fileNameResult.Value, dateCreatedResult.Value, dateModifiedResult.Value, size);
        }
    }

    /// <summary>
    /// Moves a file located at <paramref name="sourceFilePath"/> to <paramref name="destinationDirectoryPath"/>.
    /// </summary>
    /// <param name="sourceFilePath">String representation of the path where the file to be moved is located.</param>
    /// <param name="destinationDirectoryPath">String representation of the path of the directory where the file will be moved.</param>
    /// <param name="overrideExisting">Whether to override existing files, or not.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either a moved file, or an error.</returns>
    public Result<File> MoveFile(string sourceFilePath, string destinationDirectoryPath, bool? overrideExisting)
    {
        if (string.IsNullOrWhiteSpace(destinationDirectoryPath))
            return Errors.FileSystemManagement.InvalidPath;
        // make sure the paths are in the expected format
        if (!destinationDirectoryPath.EndsWith(_platformContext.PathStrategy.PathSeparator))
            destinationDirectoryPath += _platformContext.PathStrategy.PathSeparator;
        Result<FileSystemPathId> fileSystemSourcePathIdResult = FileSystemPathId.Create(sourceFilePath);
        if (fileSystemSourcePathIdResult.IsFailure)
            return fileSystemSourcePathIdResult.Errors;
        Result<FileSystemPathId> fileSystemDestinationPathIdResult = FileSystemPathId.Create(destinationDirectoryPath);
        if (fileSystemDestinationPathIdResult.IsFailure)
            return fileSystemDestinationPathIdResult.Errors;
        return MoveFile(fileSystemSourcePathIdResult.Value, fileSystemDestinationPathIdResult.Value, overrideExisting ?? false);
    }

    /// <summary>
    /// Moves a file for the specified path.
    /// </summary>
    /// <param name="sourceFilePath">Identifier for the path where the file to be moved is located.</param>
    /// <param name="destinationDirectoryPath">Identifier for the path of the directory where the file will be moved.</param>
    /// <param name="overrideExisting">Whether to override existing files, or not.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the moved file, or an error.</returns>
    public Result<File> MoveFile(FileSystemPathId sourceFilePath, FileSystemPathId destinationDirectoryPath, bool overrideExisting)
    {
        Result<bool> fileExists = _environmentContext.FileProviderService.FileExists(sourceFilePath);
        if (fileExists.IsFailure)
            return fileExists.Errors;
        else if (fileExists.Value == false)
            return Errors.FileSystemManagement.FileNotFound;
        else
        {
            // move the file
            Result<FileSystemPathId> moveFileResult = _environmentContext.FileProviderService.MoveFile(sourceFilePath, destinationDirectoryPath, overrideExisting);
            if (moveFileResult.IsFailure)
                return moveFileResult.Errors;
            Result<string> fileNameResult = _environmentContext.FileProviderService.GetFileName(moveFileResult.Value);
            Result<Optional<DateTime>> dateModifiedResult = _environmentContext.FileProviderService.GetLastWriteTime(moveFileResult.Value);
            Result<Optional<DateTime>> dateCreatedResult = _environmentContext.FileProviderService.GetCreationTime(moveFileResult.Value);
            Result<long?> sizeResult = _environmentContext.FileProviderService.GetSize(moveFileResult.Value);
            long size = !sizeResult.IsFailure ? sizeResult.Value ?? 0 : 0;
            // if any of the details returned an error, set inaccessible status
            if (fileNameResult.IsFailure || dateModifiedResult.IsFailure || dateCreatedResult.IsFailure || sizeResult.IsFailure)
            {
                Result<File> errorFileResult = File.Create(moveFileResult.Value, !fileNameResult.IsFailure ? fileNameResult.Value : null!,
                    !dateCreatedResult.IsFailure ? dateCreatedResult.Value : Optional<DateTime>.None(),
                    !dateModifiedResult.IsFailure ? dateModifiedResult.Value : Optional<DateTime>.None(), size);
                if (errorFileResult.IsFailure)
                    return errorFileResult.Errors;
                Result<Updated> setStatusResult = errorFileResult.Value.SetStatus(FileSystemItemStatus.Inaccessible);
                if (setStatusResult.IsFailure)
                    return setStatusResult.Errors;
                return errorFileResult;
            }
            else
                return File.Create(moveFileResult.Value, fileNameResult.Value, dateCreatedResult.Value, dateModifiedResult.Value, size);
        }
    }

    /// <summary>
    /// Renames a file.
    /// </summary>
    /// <param name="path">String representation of the file path.</param>
    /// <param name="name">The new name of the file.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the renamed file, or an error.</returns>
    public Result<File> RenameFile(string path, string name)
    {
        Result<FileSystemPathId> fileSystemPathIdResult = FileSystemPathId.Create(path);
        if (fileSystemPathIdResult.IsFailure)
            return fileSystemPathIdResult.Errors;
        return RenameFile(fileSystemPathIdResult.Value, name);
    }

    /// <summary>
    /// Renames a file for the specified path.
    /// </summary>
    /// <param name="path">Identifier for the file path.</param>
    /// <param name="name">The new name of the file.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the renamed file, or an error.</returns>
    public Result<File> RenameFile(FileSystemPathId path, string name)
    {
        // first, check if the directory about to be created does not already exist
        Result<FileSystemPathId> combinedPath = _platformContext.PathStrategy.CombinePath(path, name);
        if (combinedPath.IsFailure)
            return combinedPath.Errors;
        Result<bool> fileExists = _environmentContext.FileProviderService.FileExists(combinedPath.Value);
        if (fileExists.IsFailure)
            return fileExists.Errors;
        else if (fileExists.Value == true)
            return Errors.FileSystemManagement.FileAlreadyExists;
        else
        {
            // rename the file
            Result<FileSystemPathId> newFilePathResult = _environmentContext.FileProviderService.RenameFile(path, name);
            if (newFilePathResult.IsFailure)
                return newFilePathResult.Errors;
            Result<string> fileNameResult = _environmentContext.FileProviderService.GetFileName(newFilePathResult.Value);
            Result<Optional<DateTime>> dateModifiedResult = _environmentContext.FileProviderService.GetLastWriteTime(newFilePathResult.Value);
            Result<Optional<DateTime>> dateCreatedResult = _environmentContext.FileProviderService.GetCreationTime(newFilePathResult.Value);
            Result<long?> sizeResult = _environmentContext.FileProviderService.GetSize(newFilePathResult.Value);
            long size = !sizeResult.IsFailure ? sizeResult.Value ?? 0 : 0;
            // if any of the details returned an error, set inaccessible status
            if (fileNameResult.IsFailure || dateModifiedResult.IsFailure || dateCreatedResult.IsFailure || sizeResult.IsFailure)
            {
                Result<File> errorFileResult = File.Create(newFilePathResult.Value, !fileNameResult.IsFailure ? fileNameResult.Value : null!,
                    !dateCreatedResult.IsFailure ? dateCreatedResult.Value : Optional<DateTime>.None(),
                    !dateModifiedResult.IsFailure ? dateModifiedResult.Value : Optional<DateTime>.None(), size);
                if (errorFileResult.IsFailure)
                    return errorFileResult.Errors;
                Result<Updated> setStatusResult = errorFileResult.Value.SetStatus(FileSystemItemStatus.Inaccessible);
                if (setStatusResult.IsFailure)
                    return setStatusResult.Errors;
                return errorFileResult;
            }
            else
                return File.Create(newFilePathResult.Value, fileNameResult.Value, dateCreatedResult.Value, dateModifiedResult.Value, size);
        }
    }

    /// <summary>
    /// Delete a file for the specified string path.
    /// </summary>
    /// <param name="path">String representation of the file path.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the result of deleting a file, or an error.</returns>
    public Result<Deleted> DeleteFile(string path)
    {
        Result<FileSystemPathId> fileSystemPathIdResult = FileSystemPathId.Create(path);
        if (fileSystemPathIdResult.IsFailure)
            return fileSystemPathIdResult.Errors;
        return DeleteFile(fileSystemPathIdResult.Value);
    }

    /// <summary>
    /// Delete a file for the specified string path.
    /// </summary>
    /// <param name="path">Identifier for the file path.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the result of deleting a file, or an error.</returns>
    public Result<Deleted> DeleteFile(FileSystemPathId path)
    {
        return _environmentContext.FileProviderService.DeleteFile(path);
    }

    public Result<bool> ReadFile(FileSystemPathId path)
    {
        return true;
    }
}
