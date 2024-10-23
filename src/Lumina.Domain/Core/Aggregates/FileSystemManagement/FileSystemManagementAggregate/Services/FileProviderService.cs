#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Contracts.Enums.FileSystem;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.Aggregates.FileSystemManagement.FileSystemManagementAggregate.ValueObjects;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
#endregion

namespace Lumina.Domain.Core.Aggregates.FileSystemManagement.FileSystemManagementAggregate.Services;

/// <summary>
/// File provider for local file systems.
/// </summary>
internal class FileProviderService : IFileProviderService
{
    private readonly IFileSystem _fileSystem;
    private readonly IFileSystemPermissionsService _fileSystemPermissionsService;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileProviderService"/> class.
    /// </summary>
    /// <param name="fileSystem">Injected service used to interact with the local filesystem.</param>
    /// <param name="fileSystemPermissionsService">Injected service used to determine local filesystem permissions.</param>
    public FileProviderService(IFileSystem fileSystem, IFileSystemPermissionsService fileSystemPermissionsService)
    {
        _fileSystem = fileSystem;
        _fileSystemPermissionsService = fileSystemPermissionsService;
    }

    /// <summary>
    /// Retrieves a list of file paths at the specified path.
    /// </summary>
    /// <param name="path">The path for which to retrieve the list of files.</param>
    /// <param name="includeHiddenElements">Whether to include hidden files or not.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing either a collection of file paths or an error.</returns>
    public ErrorOr<IEnumerable<FileSystemPathId>> GetFilePaths(FileSystemPathId path, bool includeHiddenElements)
    {
        // check if the user has access permissions to the provided path
        if (!_fileSystemPermissionsService.CanAccessPath(path, FileAccessMode.ListDirectory))
            return Errors.Permission.UnauthorizedAccess;
        return ErrorOrFactory.From(_fileSystem.Directory.GetFiles(path.Path)
                                                        .Where(path => includeHiddenElements || (GetAttributes(path) & FileAttributes.Hidden) != FileAttributes.Hidden)
                                                        .OrderBy(path => path)
                                                        .Select(path => FileSystemPathId.Create(path))
                                                        .Where(errorOrPathId => !errorOrPathId.IsError)
                                                        .Select(errorOrPathId => errorOrPathId.Value)
                                                        .AsEnumerable());
    }

    /// <summary>
    /// Gets the attributes of a file system item identified by <paramref name="path"/>.
    /// </summary>
    /// <param name="path">The path for which to retrieve the file system attributes.</param>
    /// <returns>The retrieved file system attributes.</returns>
    private FileAttributes GetAttributes(string path)
    {
        try
        {
            // File.GetAttributes is used for both directories and files
            return _fileSystem.File.GetAttributes(path);
        }
        catch
        {
            return default;
        }
    }

    /// <summary>
    /// Checks if a file with the specified path exists.
    /// </summary>
    /// <param name="path">The path of the file whose existance is checked.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing either the result of checking the existance of a file, or an error.</returns>
    public ErrorOr<bool> FileExists(FileSystemPathId path)
    {
        return _fileSystem.File.Exists(path.Path);
    }

    /// <summary>
    /// Retrieves the file name from the specified path.
    /// </summary>
    /// <param name="path">The path to extract the file name from.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing either the name of the file without the path, or the last segment of the path if no file name is found, or an error.</returns>
    public ErrorOr<string> GetFileName(FileSystemPathId path)
    {
        return _fileSystem.Path.GetFileName(path.Path);
    }

    /// <summary>
    /// Retrieves the contents of a file at the specified path.
    /// </summary>
    /// <param name="path">The path for which to retrieve the file contents.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing either the contents of a file at the specified path, or an error.</returns>
    public ErrorOr<byte[]> GetFileAsync(FileSystemPathId path)
    {
        // check if the user has access permissions to the provided path
        if (!_fileSystemPermissionsService.CanAccessPath(path, FileAccessMode.ReadContents))
            return Errors.Permission.UnauthorizedAccess;
        return File.ReadAllBytes(path.Path);
    }

    /// <summary>
    /// Gets the last write time of a file at the specified path.
    /// </summary>
    /// <param name="path">The path of the file to retrieve the last write time for.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing either the optional last write time of <paramref name="path"/> if available, or an error.</returns>
    public ErrorOr<Optional<DateTime>> GetLastWriteTime(FileSystemPathId path)
    {
        // check if the user has access permissions to the provided path
        if (!_fileSystemPermissionsService.CanAccessPath(path, FileAccessMode.ReadProperties))
            return Errors.Permission.UnauthorizedAccess;
        return Optional<DateTime>.FromNullable(_fileSystem.File.GetLastWriteTime(path.Path));
    }

    /// <summary>
    /// Gets the creation time of a file at the specified path.
    /// </summary>
    /// <param name="path">The path of the file to retrieve the creation time for.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing either the optional creation time of <paramref name="path"/> if available, or an error.</returns>
    public ErrorOr<Optional<DateTime>> GetCreationTime(FileSystemPathId path)
    {
        // check if the user has access permissions to the provided path
        if (!_fileSystemPermissionsService.CanAccessPath(path, FileAccessMode.ReadProperties))
            return Errors.Permission.UnauthorizedAccess;
        return Optional<DateTime>.FromNullable(_fileSystem.File.GetCreationTime(path.Path));
    }

    /// <summary>
    /// Gets the size of a file at the specified path.
    /// </summary>
    /// <param name="path">The path of the file to retrieve the size for.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing either the size of <paramref name="path"/> or an error.</returns>
    public ErrorOr<long?> GetSize(FileSystemPathId path)
    {
        // check if the user has access permissions to the provided path
        if (!_fileSystemPermissionsService.CanAccessPath(path, FileAccessMode.ReadProperties))
            return Errors.Permission.UnauthorizedAccess;
        return _fileSystem.FileInfo.New(path.Path)?.Length ?? 0;
    }

    /// <summary>
    /// Copies a file located at <paramref name="sourceFilePath"/> to <paramref name="destinationDirectoryPath"/>.
    /// </summary>
    /// <param name="sourceFilePath">Identifier for the path where the file to be copied is located.</param>
    /// <param name="destinationDirectoryPath">Identifier for the path of the directory where the file will be copied.</param>
    /// <param name="overrideExisting">Whether to override existing files, or not.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing either the copied file, or an error.</returns>
    public ErrorOr<FileSystemPathId> CopyFile(FileSystemPathId sourceFilePath, FileSystemPathId destinationDirectoryPath, bool overrideExisting)
    {
        // check if the source file exists
        if (!_fileSystem.File.Exists(sourceFilePath.Path))
            return Errors.FileSystemManagement.FileNotFound;
        string fileName = _fileSystem.Path.GetFileName(sourceFilePath.Path);
        string destinationFilePath = _fileSystem.Path.Combine(destinationDirectoryPath.Path, fileName);
        // when copying a file to the same location, just copy it with a new name
        if (_fileSystem.Path.GetDirectoryName(sourceFilePath.Path) == destinationDirectoryPath.Path)
            destinationFilePath = CreateUniqueFilePath(destinationFilePath);
        else
            // check if there is already a file with the same name as the copied file, in the destination directory
            if (_fileSystem.File.Exists(destinationFilePath))
            if (!overrideExisting)
                return Errors.FileSystemManagement.FileAlreadyExists;
        try
        {
            _fileSystem.File.Copy(sourceFilePath.Path, destinationFilePath, overrideExisting); // copy the file
            _fileSystem.File.SetAttributes(destinationFilePath, _fileSystem.File.GetAttributes(sourceFilePath.Path)); // preserve file attributes
            return FileSystemPathId.Create(destinationFilePath);
        }
        catch
        {
            return Errors.FileSystemManagement.FileCopyError;
        }
    }

    /// <summary>
    /// Creates a file path that is unique
    /// </summary>
    /// <param name="destinationFilePath">The path from which to generate the unique file path.</param>
    /// <returns>A unique directory path.</returns>
    private string CreateUniqueFilePath(string destinationFilePath)
    {
        string? directory = _fileSystem.Path.GetDirectoryName(destinationFilePath);
        string? filename = _fileSystem.Path.GetFileNameWithoutExtension(destinationFilePath);
        string? extension = _fileSystem.Path.GetExtension(destinationFilePath);
        string destFilePath = destinationFilePath;
        int copyNumber = 1;
        // check if the destination file exists and create a unique file name
        if (!string.IsNullOrEmpty(directory) && !string.IsNullOrEmpty(filename) && !string.IsNullOrEmpty(extension))
            while (_fileSystem.File.Exists(destFilePath))
                destFilePath = _fileSystem.Path.Combine(directory, $"{filename} - Copy ({copyNumber++}){extension}");
        return destFilePath;
    }

    /// <summary>
    /// Moves a file located at <paramref name="sourceFilePath"/> to <paramref name="destinationDirectoryPath"/>.
    /// </summary>
    /// <param name="sourceFilePath">Identifier for the path where the file to be moved is located.</param>
    /// <param name="destinationDirectoryPath">Identifier for the path of the directory where the file will be moved.</param>
    /// <param name="overrideExisting">Whether to override existing files, or not.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing either a moved file, or an error.</returns>
    public ErrorOr<FileSystemPathId> MoveFile(FileSystemPathId sourceFilePath, FileSystemPathId destinationDirectoryPath, bool overrideExisting)
    {
        // check if the source file exists
        if (!_fileSystem.File.Exists(sourceFilePath.Path))
            return Errors.FileSystemManagement.FileNotFound;
        try
        {
            string fileName = _fileSystem.Path.GetFileName(sourceFilePath.Path);
            string destinationFilePath = _fileSystem.Path.Combine(destinationDirectoryPath.Path, fileName);
            // if the destination file does not exist, perform a simple move
            if (!_fileSystem.File.Exists(destinationFilePath))
                _fileSystem.File.Move(sourceFilePath.Path, destinationFilePath);
            else
                if (overrideExisting)
                _fileSystem.File.Move(sourceFilePath.Path, destinationFilePath, overrideExisting);
            else
                return Errors.FileSystemManagement.FileAlreadyExists;
            return FileSystemPathId.Create(destinationFilePath);
        }
        catch
        {
            return Errors.FileSystemManagement.FileMoveError;
        }
    }

    /// <summary>
    /// Renames a file at the specified path.
    /// </summary>
    /// <param name="path">The path of the file to be renamed.</param>
    /// <param name="name">The new name of the file.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing either the absolute path of the renamed file, or an error.</returns>
    public ErrorOr<FileSystemPathId> RenameFile(FileSystemPathId path, string name)
    {
        string? parentDirectory = _fileSystem.FileInfo.New(path.Path)?.DirectoryName;
        if (!string.IsNullOrEmpty(parentDirectory))
        {
            ErrorOr<FileSystemPathId> parendDirectoryResult = FileSystemPathId.Create(parentDirectory);
            if (parendDirectoryResult.IsError)
                return parendDirectoryResult.Errors;
            string? newFile = _fileSystem.Path.Combine(parendDirectoryResult.Value.Path, name);
            if (!string.IsNullOrEmpty(newFile))
            {
                ErrorOr<FileSystemPathId> newFilePathResult = FileSystemPathId.Create(newFile);
                if (newFilePathResult.IsError)
                    return newFilePathResult.Errors;
                // check if the user has access permissions to the provided path
                if (!_fileSystemPermissionsService.CanAccessPath(path, FileAccessMode.Execute))
                    return Errors.Permission.UnauthorizedAccess;
                if (!_fileSystemPermissionsService.CanAccessPath(parendDirectoryResult.Value, FileAccessMode.Write))
                    return Errors.Permission.UnauthorizedAccess;
                _fileSystem.File.Move(path.Path, newFilePathResult.Value.Path);
                return newFilePathResult;
            }
            else
                return Errors.FileSystemManagement.InvalidPath;
        }
        else
            return Errors.FileSystemManagement.CannotNavigateUp;
    }

    /// <summary>
    /// Deletes a file at the specified path.
    /// </summary>
    /// <param name="path">The path of the file to be deleted.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing either the result of deleting a file, or an error.</returns>
    public ErrorOr<Deleted> DeleteFile(FileSystemPathId path)
    {
        // check if the user has access permissions to the provided path
        if (!_fileSystemPermissionsService.CanAccessPath(path, FileAccessMode.Delete))
            return Errors.Permission.UnauthorizedAccess;
        _fileSystem.File.Delete(path.Path);
        return Result.Deleted;
    }
}
