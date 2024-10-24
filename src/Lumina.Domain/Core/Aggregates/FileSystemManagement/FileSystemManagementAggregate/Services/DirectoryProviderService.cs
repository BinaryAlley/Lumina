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
/// Directory provider for local file systems.
/// </summary>
internal class DirectoryProviderService : IDirectoryProviderService
{
    private readonly IFileSystem _fileSystem;
    private readonly IFileSystemPermissionsService _fileSystemPermissionsService;

    /// <summary>
    /// Initializes a new instance of the <see cref="DirectoryProviderService"/> class.
    /// </summary>
    /// <param name="fileSystem">Injected service used to interact with the local filesystem.</param>
    /// <param name="fileSystemPermissionsService">Injected service used to determine local filesystem permissions.</param>
    public DirectoryProviderService(IFileSystem fileSystem, IFileSystemPermissionsService fileSystemPermissionsService)
    {
        _fileSystem = fileSystem;
        _fileSystemPermissionsService = fileSystemPermissionsService;
    }

    /// <summary>
    /// Retrieves a list of subdirectory paths from the specified path.
    /// </summary>
    /// <param name="path">The path from which to retrieve the subdirectory paths.</param>
    /// <param name="includeHiddenElements">Whether to include hidden subdirectories or not.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing either a collection of directory paths or an error.</returns>
    public ErrorOr<IEnumerable<FileSystemPathId>> GetSubdirectoryPaths(FileSystemPathId path, bool includeHiddenElements)
    {
        // check if the user has access permissions to the provided path
        if (!_fileSystemPermissionsService.CanAccessPath(path, FileAccessMode.ListDirectory, false))
            return Errors.Permission.UnauthorizedAccess;
        return ErrorOrFactory.From(_fileSystem.Directory.GetDirectories(path.Path)
                                                        .Where(path => includeHiddenElements || (GetAttributes(path) & FileAttributes.Hidden) != FileAttributes.Hidden)
                                                        .OrderBy(path => path)
                                                        .Select(path => path.EndsWith(_fileSystem.Path.DirectorySeparatorChar) ? path : path + _fileSystem.Path.DirectorySeparatorChar)
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
    /// Checks if a directory with the specified path exists.
    /// </summary>
    /// <param name="path">The path of the directory whose existance is checked.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing either the result of checking the existance of a directory, or an error.</returns>
    public ErrorOr<bool> DirectoryExists(FileSystemPathId path)
    {
        return _fileSystem.Directory.Exists(path.Path);
    }

    /// <summary>
    /// Retrieves the file name from the specified path.
    /// </summary>
    /// <param name="path">The path to extract the file name from.</param>
    /// <returns>The name of the file without the path, or the last segment of the path if no file name is found.</returns>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing either a file name or an error.</returns>
    public ErrorOr<string> GetFileName(FileSystemPathId path)
    {
        string inputPath = path.Path.EndsWith(_fileSystem.Path.DirectorySeparatorChar) ? path.Path[..^1] : path.Path;
        return _fileSystem.Path.GetFileName(inputPath);
    }

    /// <summary>
    /// Gets the last write time of a specific path.
    /// </summary>
    /// <param name="path">The path to retrieve the last write time for.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing either the optional last write time of <paramref name="path"/> if available, or an error.</returns>
    public ErrorOr<Optional<DateTime>> GetLastWriteTime(FileSystemPathId path)
    {
        // check if the user has access permissions to the provided path
        if (!_fileSystemPermissionsService.CanAccessPath(path, FileAccessMode.ReadProperties, false))
            return Errors.Permission.UnauthorizedAccess;
        return Optional<DateTime>.FromNullable(_fileSystem.Directory.GetLastWriteTime(path.Path));
    }

    /// <summary>
    /// Gets the creation time of a specific path.
    /// </summary>
    /// <param name="path">The path to retrieve the creation time for.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing the optional creation time of <paramref name="path"/> if available, or an error.</returns>
    public ErrorOr<Optional<DateTime>> GetCreationTime(FileSystemPathId path)
    {
        // check if the user has access permissions to the provided path
        if (!_fileSystemPermissionsService.CanAccessPath(path, FileAccessMode.ReadProperties, false))
            return Errors.Permission.UnauthorizedAccess;
        return Optional<DateTime>.FromNullable(_fileSystem.Directory.GetCreationTime(path.Path));
    }

    /// <summary>
    /// Creates a new directory with the specified name, at the specified path.
    /// </summary>
    /// <param name="path">The path where the directory will be created..</param>
    /// <param name="name">The name of the directory that will be created.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing either the path of a created directory, or an error.</returns>
    public ErrorOr<FileSystemPathId> CreateDirectory(FileSystemPathId path, string name)
    {
        // to create a directory, its parent directory must be writable
        if (!_fileSystemPermissionsService.CanAccessPath(path, FileAccessMode.Write, false))
            return Errors.Permission.UnauthorizedAccess;
        // create the directory and return its absolute path
        string directoryPath = _fileSystem.Directory.CreateDirectory(_fileSystem.Path.Combine(path.Path, name)).FullName;
        return FileSystemPathId.Create(directoryPath);
    }

    /// <summary>
    /// Copies a directory for the specified path.
    /// </summary>
    /// <param name="sourcePath">Identifier for the path where the directory to be copied is located.</param>
    /// <param name="destinationPath">Identifier for the path where the directory will be copied.</param>
    /// <param name="overrideExisting">Whether to override existing directories, or not.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing either the copied directory, or an error.</returns>
    public ErrorOr<FileSystemPathId> CopyDirectory(FileSystemPathId sourcePath, FileSystemPathId destinationPath, bool overrideExisting)
    {
        // check if the source directory exists
        if (!_fileSystem.Directory.Exists(sourcePath.Path))
            return Errors.FileSystemManagement.DirectoryNotFound;
        string destPath = CreateUniqueDirectoryPath(destinationPath.Path);
        try
        {
            // create the new directory
            IDirectoryInfo destDirInfo = _fileSystem.DirectoryInfo.New(_fileSystem.Directory.CreateDirectory(destPath).FullName);
            // get the information of the source directory
            IDirectoryInfo sourceDirInfo = _fileSystem.DirectoryInfo.New(sourcePath.Path);
            // copy all files
            foreach (IFileInfo fileInfo in sourceDirInfo.GetFiles())
            {
                string targetFilePath = _fileSystem.Path.Combine(destPath, fileInfo.Name);
                fileInfo.CopyTo(targetFilePath, overrideExisting);
                _fileSystem.File.SetAttributes(targetFilePath, fileInfo.Attributes); // preserve file attributes
            }
            // copy all subdirectories
            foreach (IDirectoryInfo subDirInfo in sourceDirInfo.GetDirectories())
            {
                ErrorOr<FileSystemPathId> newSourcePathResult = FileSystemPathId.Create(subDirInfo.FullName);
                if (newSourcePathResult.IsError)
                    return newSourcePathResult.Errors;
                ErrorOr<FileSystemPathId> newDestinationPathResult = FileSystemPathId.Create(_fileSystem.Path.Combine(destPath, subDirInfo.Name));
                if (newDestinationPathResult.IsError)
                    return newDestinationPathResult.Errors;
                // recursive call
                ErrorOr<FileSystemPathId> copySubDirResult = CopyDirectory(newSourcePathResult.Value, newDestinationPathResult.Value, overrideExisting);
                if (copySubDirResult.IsError)
                    return copySubDirResult.Errors;
            }
            return FileSystemPathId.Create(destPath);
        }
        catch
        {
            return Errors.FileSystemManagement.DirectoryCopyError;
        }
    }

    /// <summary>
    /// Creates a directory path that is unique.
    /// </summary>
    /// <param name="destinationPath">The path from which to generate the unique directory path.</param>
    /// <returns>A unique directory path.</returns>
    private string CreateUniqueDirectoryPath(string destinationPath)
    {
        string destPath = destinationPath;
        int copyNumber = 1;
        // check if the destination directory exists and create a unique directory name
        while (_fileSystem.Directory.Exists(destPath) || _fileSystem.File.Exists(destPath))
            destPath = $"{destinationPath} - Copy ({copyNumber++})";
        return destPath;
    }

    /// <summary>
    /// Moves a directory located at <paramref name="sourcePath"/> to <paramref name="destinationPath"/>.
    /// </summary>
    /// <param name="sourcePath">Identifier for the path where the directory to be moved is located.</param>
    /// <param name="destinationPath">Identifier for the path where the directory will be moved.</param>
    /// <param name="overrideExisting">Whether to override existing directories, or not.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing either a moved directory, or an error.</returns>
    public ErrorOr<FileSystemPathId> MoveDirectory(FileSystemPathId sourcePath, FileSystemPathId destinationPath, bool overrideExisting)
    {
        // check if the source directory exists
        if (!_fileSystem.Directory.Exists(sourcePath.Path))
            return Errors.FileSystemManagement.DirectoryNotFound;
        try
        {
            // if the destination directory does not exist, perform a simple move
            if (!_fileSystem.Directory.Exists(destinationPath.Path))
                _fileSystem.Directory.Move(sourcePath.Path, destinationPath.Path);
            else
                MergeDirectories(sourcePath.Path, destinationPath.Path, overrideExisting); // if the destination directory exists, perform a merge based on the overwrite policy
            return FileSystemPathId.Create(destinationPath.Path);
        }
        catch
        {
            return Errors.FileSystemManagement.DirectoryMoveError;
        }
    }

    /// <summary>
    /// Merges two directories.
    /// </summary>
    /// <param name="sourceDir">The source directory to be merged.</param>
    /// <param name="destDir">The destination directory to be merged.</param>
    /// <param name="overwrite">Whether to override existing files.</param>
    private void MergeDirectories(string sourceDir, string destDir, bool overwrite)
    {
        // merge files from source directory to destination directory
        foreach (string sourceFilePath in _fileSystem.Directory.GetFiles(sourceDir))
        {
            string fileName = _fileSystem.Path.GetFileName(sourceFilePath);
            string destFilePath = _fileSystem.Path.Combine(destDir, fileName);
            // if a file with the same name exists in the destination
            if (_fileSystem.File.Exists(destFilePath))
            {
                if (overwrite)
                {
                    // overwrite the file in the destination directory
                    _fileSystem.File.Delete(destFilePath);
                    _fileSystem.File.Move(sourceFilePath, destFilePath);
                }
            }
            else
                _fileSystem.File.Move(sourceFilePath, destFilePath); // move the file if it does not exist in the destination directory
        }
        // recursively merge subdirectories
        foreach (string sourceSubDirPath in _fileSystem.Directory.GetDirectories(sourceDir))
        {
            string subDirName = _fileSystem.Path.GetFileName(sourceSubDirPath);
            string destSubDirPath = _fileSystem.Path.Combine(destDir, subDirName);
            // if the subdirectory does not exist in the destination, move it
            if (!_fileSystem.Directory.Exists(destSubDirPath))
                _fileSystem.Directory.Move(sourceSubDirPath, destSubDirPath);
            else
                MergeDirectories(sourceSubDirPath, destSubDirPath, overwrite); // if the subdirectory exists, recursively merge its contents
        }
        // after merging the contents, delete the source directory if it's now empty
        if (!_fileSystem.Directory.EnumerateFileSystemEntries(sourceDir).Any())
            _fileSystem.Directory.Delete(sourceDir);
    }

    /// <summary>
    /// Renames a directory at the specified path.
    /// </summary>
    /// <param name="path">The path of the directory to be renamed.</param>
    /// <param name="name">The new name of the directory.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing either the absolute path of the renamed directory, or an error.</returns>
    public ErrorOr<FileSystemPathId> RenameDirectory(FileSystemPathId path, string name)
    {
        // to rename a directory, its parent directory must be writable
        string? parentDirectory = _fileSystem.Directory.GetParent(path.Path)?.FullName;
        if (!string.IsNullOrEmpty(parentDirectory))
        {
            ErrorOr<FileSystemPathId> parendDirectoryResult = FileSystemPathId.Create(parentDirectory);
            if (parendDirectoryResult.IsError)
                return parendDirectoryResult.Errors;
            string? newDirectory = _fileSystem.Path.Combine(parentDirectory, name);
            if (!string.IsNullOrEmpty(newDirectory))
            {
                ErrorOr<FileSystemPathId> newDirectoryPathResult = FileSystemPathId.Create(newDirectory);
                if (newDirectoryPathResult.IsError)
                    return newDirectoryPathResult.Errors;
                if (!_fileSystemPermissionsService.CanAccessPath(parendDirectoryResult.Value, FileAccessMode.Write, false))
                    return Errors.Permission.UnauthorizedAccess;
                // to rename a directory, it must be executable
                if (!_fileSystemPermissionsService.CanAccessPath(path, FileAccessMode.Execute, false))
                    return Errors.Permission.UnauthorizedAccess;
                _fileSystem.Directory.Move(path.Path, newDirectoryPathResult.Value.Path);
                return newDirectoryPathResult.Value;
            }
            else
                return Errors.FileSystemManagement.InvalidPath;
        }
        else
            return Errors.FileSystemManagement.InvalidPath;
    }

    /// <summary>
    /// Deletes a directory at the specified path.
    /// </summary>
    /// <param name="path">The path of the directory to be deleted.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing either the result of deleting a directory, or an error.</returns>
    public ErrorOr<Deleted> DeleteDirectory(FileSystemPathId path)
    {
        // check if the user has access permissions to the provided path
        if (!_fileSystemPermissionsService.CanAccessPath(path, FileAccessMode.Delete, false))
            return Errors.Permission.UnauthorizedAccess;
        _fileSystem.DirectoryInfo.New(path.Path).Delete(true);
        return Result.Deleted;
    }
}
