#region ========================================================================= USING =====================================================================================
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using ErrorOr;
using Lumina.Domain.Common.Enums.FileSystem;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Services;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.ValueObjects;
using Lumina.Domain.UnitTests.Core.Aggregates.FileSystemManagementAggregate.ValueObjects.Fixtures;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Runtime.InteropServices;
#endregion

namespace Lumina.Domain.UnitTests.Core.Aggregates.FileSystemManagementAggregate.Services;

/// <summary>
/// Contains unit tests for the <see cref="FileProviderService"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class FileProviderServiceTests
{
    private readonly IFixture _fixture;
    private readonly IFileSystem _mockFileSystem;
    private readonly IFileSystemPermissionsService _mockFileSystemPermissionsService;
    private readonly FileProviderService _sut;
    private readonly FileSystemPathIdFixture _fileSystemPathIdFixture;
    private static readonly bool s_isUnix = RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
    private readonly string _pathVisible1 = s_isUnix ? "/Visible.txt" : @"C:\Visible.txt";
    private readonly string _pathHidden1 = s_isUnix ? "/Hidden.txt" : @"C:\Hidden.txt";
    private readonly string _pathValid1 = s_isUnix ? "/Valid.txt" : @"C:\Valid.txt";
    private readonly string _pathInvalid1 = s_isUnix ? "/Invalid.txt" : @"C:\Invalid.txt";
    private readonly string _pathSourceFile = s_isUnix ? "/Source/file.txt" : @"C:\Source\file.txt";
    private readonly string _pathDestinationFile = s_isUnix ? "/Destination/file.txt" : @"C:\Destination\file.txt";
    private readonly string _pathSource = s_isUnix ? "/Source" : @"C:\Source";
    private readonly string _pathDestination = s_isUnix ? "/Destination" : @"C:\Destination";

    /// <summary>
    /// Initializes a new instance of the <see cref="FileProviderServiceTests"/> class.
    /// </summary>
    public FileProviderServiceTests()
    {
        _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
        _mockFileSystem = Substitute.For<IFileSystem>();
        _mockFileSystemPermissionsService = Substitute.For<IFileSystemPermissionsService>();
        _sut = new FileProviderService(_mockFileSystem, _mockFileSystemPermissionsService);
        _fileSystemPathIdFixture = new();
    }

    [Fact]
    public void GetFilePaths_WhenUserHasNoAccess_ShouldReturnUnauthorizedAccessError()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId();
        _mockFileSystemPermissionsService.CanAccessPath(path, FileAccessMode.ListDirectory).Returns(false);

        // Act
        ErrorOr<IEnumerable<FileSystemPathId>> result = _sut.GetFilePaths(path, false);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(Errors.Permission.UnauthorizedAccess, result.FirstError);
    }

    [Fact]
    public void GetFilePaths_WhenUserHasAccess_ShouldReturnFiles()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId();
        string[] files = _fixture.CreateMany<string>(3).ToArray();
        _mockFileSystemPermissionsService.CanAccessPath(path, FileAccessMode.ListDirectory).Returns(true);
        _mockFileSystem.Directory.GetFiles(path.Path).Returns(files);

        // Act
        ErrorOr<IEnumerable<FileSystemPathId>> result = _sut.GetFilePaths(path, false);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(3, result.Value.Count());
        Assert.Equal(
            files.OrderBy(f => f),
            result.Value.Select(x => x.Path).OrderBy(f => f)
        );
    }

    [Fact]
    public void GetFilePaths_WhenIncludeHiddenElementsIsFalse_ShouldExcludeHiddenFiles()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId();
        string[] files = [_pathVisible1, _pathHidden1];
        _mockFileSystemPermissionsService.CanAccessPath(path, FileAccessMode.ListDirectory).Returns(true);
        _mockFileSystem.Directory.GetFiles(path.Path).Returns(files);
        _mockFileSystem.File.GetAttributes(_pathVisible1).Returns(FileAttributes.Normal);
        _mockFileSystem.File.GetAttributes(_pathHidden1).Returns(FileAttributes.Hidden);

        // Act
        ErrorOr<IEnumerable<FileSystemPathId>> result = _sut.GetFilePaths(path, false);

        // Assert
        Assert.False(result.IsError);
        Assert.Single(result.Value);
        Assert.Equal(_pathVisible1, result.Value.First().Path);
    }

    [Fact]
    public void GetFilePaths_WhenIncludeHiddenElementsIsTrue_ShouldIncludeHiddenFiles()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId();
        string[] files = [_pathVisible1, _pathHidden1];
        _mockFileSystemPermissionsService.CanAccessPath(path, FileAccessMode.ListDirectory).Returns(true);
        _mockFileSystem.Directory.GetFiles(path.Path).Returns(files);
        _mockFileSystem.File.GetAttributes(_pathVisible1).Returns(FileAttributes.Normal);
        _mockFileSystem.File.GetAttributes(_pathHidden1).Returns(FileAttributes.Hidden);

        // Act
        ErrorOr<IEnumerable<FileSystemPathId>> result = _sut.GetFilePaths(path, true);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(2, result.Value.Count());
        Assert.Equal(
            new[] { _pathVisible1, _pathHidden1 }.OrderBy(p => p),
            result.Value.Select(x => x.Path).OrderBy(p => p)
        );
    }

    [Fact]
    public void GetFilePaths_WhenGetAttributesThrowsException_ShouldStillAddFile()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId();
        string[] files = [_pathValid1, _pathInvalid1];
        _mockFileSystemPermissionsService.CanAccessPath(path, FileAccessMode.ListDirectory).Returns(true);
        _mockFileSystem.Directory.GetFiles(path.Path).Returns(files);
        _mockFileSystem.File.GetAttributes(_pathValid1).Returns(FileAttributes.Normal);
        _mockFileSystem.File.GetAttributes(_pathInvalid1).Throws(new Exception("Access denied"));

        // Act
        ErrorOr<IEnumerable<FileSystemPathId>> result = _sut.GetFilePaths(path, false);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(2, result.Value.Count());
        Assert.Equal([_pathInvalid1, _pathValid1], result.Value.Select(x => x.Path));
    }

    [Fact]
    public void GetFilePaths_WhenCalled_ShouldOrderFilesAlphabetically()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId();
        string[] files = s_isUnix
            ? ["/C.txt", "/A.txt", "/B.txt"]
            : [@"C:\C.txt", @"C:\A.txt", @"C:\B.txt"];
        string[] expectedFiles = s_isUnix
            ? ["/A.txt", "/B.txt", "/C.txt"]
            : [@"C:\A.txt", @"C:\B.txt", @"C:\C.txt"];
        _mockFileSystemPermissionsService.CanAccessPath(path, FileAccessMode.ListDirectory).Returns(true);
        _mockFileSystem.Directory.GetFiles(path.Path).Returns(files);
        _mockFileSystem.File.GetAttributes(Arg.Any<string>()).Returns(FileAttributes.Normal);

        // Act
        ErrorOr<IEnumerable<FileSystemPathId>> result = _sut.GetFilePaths(path, false);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(expectedFiles, result.Value.Select(x => x.Path));
    }

    [Fact]
    public void GetFilePaths_WhenDirectoryHasNoFiles_ShouldReturnEmptyList()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId();
        _mockFileSystemPermissionsService.CanAccessPath(path, FileAccessMode.ListDirectory).Returns(true);
        _mockFileSystem.Directory.GetFiles(path.Path).Returns([]);

        // Act
        ErrorOr<IEnumerable<FileSystemPathId>> result = _sut.GetFilePaths(path, false);

        // Assert
        Assert.False(result.IsError);
        Assert.Empty(result.Value);
    }

    [Fact]
    public void FileExists_WhenFileExists_ShouldReturnTrue()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId();
        _mockFileSystem.File.Exists(path.Path).Returns(true);

        // Act
        ErrorOr<bool> result = _sut.FileExists(path);

        // Assert
        Assert.False(result.IsError);
        Assert.True(result.Value);
    }

    [Fact]
    public void FileExists_WhenFileDoesNotExist_ShouldReturnFalse()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId();
        _mockFileSystem.File.Exists(path.Path).Returns(false);

        // Act
        ErrorOr<bool> result = _sut.FileExists(path);

        // Assert
        Assert.False(result.IsError);
        Assert.False(result.Value);
    }

    [Fact]
    public void GetFileName_WhenPathIsValid_ShouldReturnFileName()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId();
        string expectedFileName = "testfile.txt";
        _mockFileSystem.Path.GetFileName(path.Path).Returns(expectedFileName);

        // Act
        ErrorOr<string> result = _sut.GetFileName(path);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(expectedFileName, result.Value);
    }

    [Fact]
    public void GetFileName_WhenPathIsEmpty_ShouldReturnEmptyString()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId();
        _mockFileSystem.Path.GetFileName(path.Path).Returns(string.Empty);

        // Act
        ErrorOr<string> result = _sut.GetFileName(path);

        // Assert
        Assert.False(result.IsError);
        Assert.Empty(result.Value);
    }

    [Fact]
    public void GetFileAsync_WhenUserHasAccess_ShouldReturnFileContents()
    {
        // Arrange
        string tempFilePath = Path.GetTempFileName();
        try
        {
            byte[] expectedContents = [1, 2, 3, 4, 5];
            File.WriteAllBytes(tempFilePath, expectedContents);

            FileSystemPathId path = FileSystemPathId.Create(tempFilePath).Value;
            _mockFileSystemPermissionsService.CanAccessPath(path, FileAccessMode.ReadContents).Returns(true);

            // We need to update the _fileSystem mock to use the real file system for this method
            _mockFileSystem.File.ReadAllBytes(tempFilePath).Returns(x => File.ReadAllBytes(x.Arg<string>()));

            // Act
            ErrorOr<byte[]> result = _sut.GetFileAsync(path);

            // Assert
            Assert.False(result.IsError);
            Assert.Equal(expectedContents, result.Value);
        }
        finally
        {
            // clean up
            File.Delete(tempFilePath);
        }
    }

    [Fact]
    public void GetFileAsync_WhenUserHasNoAccess_ShouldReturnUnauthorizedAccessError()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId();
        _mockFileSystemPermissionsService.CanAccessPath(path, FileAccessMode.ReadContents).Returns(false);

        // Act
        ErrorOr<byte[]> result = _sut.GetFileAsync(path);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(Errors.Permission.UnauthorizedAccess, result.FirstError);
    }

    [Fact]
    public void GetFileAsync_WhenFileIsEmpty_ShouldReturnEmptyByteArray()
    {
        // Arrange
        string tempFilePath = Path.GetTempFileName();
        try
        {
            FileSystemPathId path = FileSystemPathId.Create(tempFilePath).Value;
            _mockFileSystemPermissionsService.CanAccessPath(path, FileAccessMode.ReadContents).Returns(true);

            // Act
            ErrorOr<byte[]> result = _sut.GetFileAsync(path);

            // Assert
            Assert.False(result.IsError);
            Assert.Empty(result.Value);
        }
        finally
        {
            // clean up
            File.Delete(tempFilePath);
        }
    }

    [Fact]
    public void GetLastWriteTime_WhenUserHasAccess_ShouldReturnLastWriteTime()
    {
        // Arrange
        string tempFilePath = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFilePath, "Test content");
            FileSystemPathId path = FileSystemPathId.Create(tempFilePath).Value;
            DateTime expectedDateTime = File.GetLastWriteTime(tempFilePath);

            _mockFileSystemPermissionsService.CanAccessPath(path, FileAccessMode.ReadProperties).Returns(true);
            _mockFileSystem.File.GetLastWriteTime(path.Path).Returns(expectedDateTime);

            // Act
            ErrorOr<Optional<DateTime>> result = _sut.GetLastWriteTime(path);

            // Assert
            Assert.False(result.IsError);
            Assert.True(result.Value.HasValue);
            Assert.Equal(expectedDateTime, result.Value.Value, TimeSpan.FromSeconds(1));
        }
        finally
        {
            File.Delete(tempFilePath);
        }
    }

    [Fact]
    public void GetLastWriteTime_WhenUserHasNoAccess_ShouldReturnUnauthorizedAccessError()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId();
        _mockFileSystemPermissionsService.CanAccessPath(path, FileAccessMode.ReadProperties).Returns(false);

        // Act
        ErrorOr<Optional<DateTime>> result = _sut.GetLastWriteTime(path);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(Errors.Permission.UnauthorizedAccess, result.FirstError);
    }

    [Fact]
    public void GetCreationTime_WhenUserHasAccess_ShouldReturnCreationTime()
    {
        // Arrange
        string tempFilePath = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFilePath, "Test content");
            FileSystemPathId path = FileSystemPathId.Create(tempFilePath).Value;
            DateTime expectedDateTime = File.GetCreationTime(tempFilePath);

            _mockFileSystemPermissionsService.CanAccessPath(path, FileAccessMode.ReadProperties).Returns(true);
            _mockFileSystem.File.GetCreationTime(path.Path).Returns(expectedDateTime);

            // Act
            ErrorOr<Optional<DateTime>> result = _sut.GetCreationTime(path);

            // Assert
            Assert.False(result.IsError);
            Assert.True(result.Value.HasValue);
            Assert.Equal(expectedDateTime, result.Value.Value, TimeSpan.FromSeconds(1));
        }
        finally
        {
            File.Delete(tempFilePath);
        }
    }

    [Fact]
    public void GetCreationTime_WhenUserHasNoAccess_ShouldReturnUnauthorizedAccessError()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId();
        _mockFileSystemPermissionsService.CanAccessPath(path, FileAccessMode.ReadProperties).Returns(false);

        // Act
        ErrorOr<Optional<DateTime>> result = _sut.GetCreationTime(path);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(Errors.Permission.UnauthorizedAccess, result.FirstError);
    }

    [Fact]
    public void GetSize_WhenUserHasAccess_ShouldReturnFileSize()
    {
        // Arrange
        string tempFilePath = Path.GetTempFileName();
        try
        {
            string content = "Test content";
            File.WriteAllText(tempFilePath, content);
            FileSystemPathId path = FileSystemPathId.Create(tempFilePath).Value;
            long expectedSize = new FileInfo(tempFilePath).Length;

            _mockFileSystemPermissionsService.CanAccessPath(path, FileAccessMode.ReadProperties).Returns(true);
            _mockFileSystem.FileInfo.New(path.Path).Returns(new FileInfoWrapper(_mockFileSystem, new FileInfo(tempFilePath)));

            // Act
            ErrorOr<long?> result = _sut.GetSize(path);

            // Assert
            Assert.False(result.IsError);
            Assert.NotNull(result.Value);
            Assert.Equal(expectedSize, result.Value);
        }
        finally
        {
            File.Delete(tempFilePath);
        }
    }

    [Fact]
    public void GetSize_WhenUserHasNoAccess_ShouldReturnUnauthorizedAccessError()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId();
        _mockFileSystemPermissionsService.CanAccessPath(path, FileAccessMode.ReadProperties).Returns(false);

        // Act
        ErrorOr<long?> result = _sut.GetSize(path);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(Errors.Permission.UnauthorizedAccess, result.FirstError);
    }

    [Fact]
    public void GetSize_WhenFileDoesNotExist_ShouldReturnZero()
    {
        // Arrange
        string nonExistentFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        FileSystemPathId path = FileSystemPathId.Create(nonExistentFilePath).Value;

        _mockFileSystemPermissionsService.CanAccessPath(path, FileAccessMode.ReadProperties).Returns(true);
        _mockFileSystem.FileInfo.New(path.Path).Returns((IFileInfo)null!);

        // Act
        ErrorOr<long?> result = _sut.GetSize(path);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(0, result.Value);
    }

    [Fact]
    public void CopyFile_WhenSourceFileExistsAndUserHasAccess_ShouldCopyFileAndReturnPath()
    {
        // Arrange
        FileSystemPathId sourcePath = _fileSystemPathIdFixture.CreateFileSystemPathId(_pathSourceFile);
        FileSystemPathId destinationPath = _fileSystemPathIdFixture.CreateFileSystemPathId(_pathDestination);
        bool overrideExisting = false;

        _mockFileSystem.File.Exists(sourcePath.Path).Returns(true);
        _mockFileSystem.File.Exists(Path.Combine(destinationPath.Path, "file.txt")).Returns(false);

        _mockFileSystem.Path.GetFileName(sourcePath.Path).Returns("file.txt");
        _mockFileSystem.Path.Combine(destinationPath.Path, "file.txt").Returns(_pathDestinationFile);

        // Act
        ErrorOr<FileSystemPathId> result = _sut.CopyFile(sourcePath, destinationPath, overrideExisting);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(_pathDestinationFile, result.Value.Path);
        _mockFileSystem.File.Received(1).Copy(sourcePath.Path, _pathDestinationFile, overrideExisting);
    }

    [Fact]
    public void CopyFile_WhenSourceFileDoesNotExist_ShouldReturnError()
    {
        // Arrange
        FileSystemPathId sourcePath = _fileSystemPathIdFixture.CreateFileSystemPathId();
        FileSystemPathId destinationPath = _fileSystemPathIdFixture.CreateFileSystemPathId();
        bool overrideExisting = false;

        _mockFileSystem.File.Exists(sourcePath.Path).Returns(false);

        // Act
        ErrorOr<FileSystemPathId> result = _sut.CopyFile(sourcePath, destinationPath, overrideExisting);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(Errors.FileSystemManagement.FileNotFound, result.FirstError);
    }

    [Fact]
    public void CopyFile_WhenDestinationFileExistsAndOverrideIsFalse_ShouldReturnFileAlreadyExistsError()
    {
        // Arrange
        FileSystemPathId sourcePath = _fileSystemPathIdFixture.CreateFileSystemPathId(_pathSourceFile);
        FileSystemPathId destinationPath = _fileSystemPathIdFixture.CreateFileSystemPathId(_pathDestination);
        string expectedDestinationFilePath = _pathDestinationFile;
        bool overrideExisting = false;

        _mockFileSystem.File.Exists(sourcePath.Path).Returns(true);
        _mockFileSystem.File.Exists(expectedDestinationFilePath).Returns(true);

        _mockFileSystem.Path.GetFileName(sourcePath.Path).Returns("file.txt");
        _mockFileSystem.Path.GetDirectoryName(sourcePath.Path).Returns(_pathSource);

        _mockFileSystem.Path.Combine(Arg.Any<string>(), Arg.Any<string>())
            .Returns(callInfo => Path.Combine(callInfo.ArgAt<string>(0), callInfo.ArgAt<string>(1)));

        // Act
        ErrorOr<FileSystemPathId> result = _sut.CopyFile(sourcePath, destinationPath, overrideExisting);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(Errors.FileSystemManagement.FileAlreadyExists, result.FirstError);
        _mockFileSystem.File.DidNotReceive().Copy(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>());
    }

    [Fact]
    public void CopyFile_WhenOverrideExistingIsTrue_ShouldOverwriteExistingFile()
    {
        // Arrange
        FileSystemPathId sourcePath = _fileSystemPathIdFixture.CreateFileSystemPathId(_pathSourceFile);
        FileSystemPathId destinationPath = _fileSystemPathIdFixture.CreateFileSystemPathId(_pathDestination);
        bool overrideExisting = true;

        _mockFileSystem.File.Exists(sourcePath.Path).Returns(true);
        _mockFileSystem.File.Exists(_pathDestinationFile).Returns(true);

        _mockFileSystem.Path.GetFileName(sourcePath.Path).Returns("file.txt");
        _mockFileSystem.Path.Combine(destinationPath.Path, "file.txt").Returns(_pathDestinationFile);

        // Act
        ErrorOr<FileSystemPathId> result = _sut.CopyFile(sourcePath, destinationPath, overrideExisting);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(_pathDestinationFile, result.Value.Path);
        _mockFileSystem.File.Received(1).Copy(sourcePath.Path, _pathDestinationFile, overrideExisting);
    }

    [Fact]
    public void CopyFile_WhenExceptionOccurs_ShouldReturnError()
    {
        // Arrange
        FileSystemPathId sourcePath = _fileSystemPathIdFixture.CreateFileSystemPathId();
        FileSystemPathId destinationPath = _fileSystemPathIdFixture.CreateFileSystemPathId();
        bool overrideExisting = false;

        _mockFileSystem.File.Exists(sourcePath.Path).Returns(true);
        _mockFileSystem.File.When(x => x.Copy(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>()))
                            .Do(x => { throw new Exception("Simulated error"); });

        // Act
        ErrorOr<FileSystemPathId> result = _sut.CopyFile(sourcePath, destinationPath, overrideExisting);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(Errors.FileSystemManagement.FileCopyError, result.FirstError);
    }

    [Fact]
    public void CopyFile_WhenCopyingToSameDirectory_ShouldCreateUniqueFileName()
    {
        // Arrange
        string sourceDirectory = _pathSource;
        FileSystemPathId sourcePath = _fileSystemPathIdFixture.CreateFileSystemPathId(Path.Combine(sourceDirectory, "file.txt"));
        FileSystemPathId destinationPath = _fileSystemPathIdFixture.CreateFileSystemPathId(sourceDirectory);
        string expectedNewFilePath = Path.Combine(sourceDirectory, "file - Copy (1).txt");
        bool overrideExisting = false;

        _mockFileSystem.File.Exists(sourcePath.Path).Returns(true);
        _mockFileSystem.File.Exists(Path.Combine(sourceDirectory, "file.txt")).Returns(true);
        _mockFileSystem.File.Exists(expectedNewFilePath).Returns(false);

        _mockFileSystem.Path.GetFileName(sourcePath.Path).Returns("file.txt");
        _mockFileSystem.Path.GetDirectoryName(sourcePath.Path).Returns(sourceDirectory);
        _mockFileSystem.Path.GetFileNameWithoutExtension(Arg.Any<string>()).Returns(x => Path.GetFileNameWithoutExtension(x.Arg<string>()));
        _mockFileSystem.Path.GetExtension(Arg.Any<string>()).Returns(x => Path.GetExtension(x.Arg<string>()));

        _mockFileSystem.Path.Combine(Arg.Any<string>(), Arg.Any<string>())
            .Returns(callInfo => Path.Combine(callInfo.ArgAt<string>(0), callInfo.ArgAt<string>(1)));

        // Simulate the behavior of CreateUniqueFilePath
        int copyNumber = 1;
        _mockFileSystem.File.Exists(Arg.Any<string>())
            .Returns(x =>
            {
                string path = x.Arg<string>();
                if (path == expectedNewFilePath)
                    return false;
                copyNumber++;
                return true;
            });

        // Act
        ErrorOr<FileSystemPathId> result = _sut.CopyFile(sourcePath, destinationPath, overrideExisting);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(expectedNewFilePath, result.Value.Path);
        _mockFileSystem.File.Received(1).Copy(sourcePath.Path, expectedNewFilePath, overrideExisting);
    }

    [Fact]
    public void MoveFile_WhenSourceFileDoesNotExist_ShouldReturnFileNotFoundError()
    {
        // Arrange
        FileSystemPathId sourcePath = _fileSystemPathIdFixture.CreateFileSystemPathId(
            s_isUnix ? "/NonExistentSource.txt" : @"C:\NonExistentSource.txt"
        );
        FileSystemPathId destinationPath = _fileSystemPathIdFixture.CreateFileSystemPathId(_pathDestination);
        bool overrideExisting = false;

        _mockFileSystem.File.Exists(sourcePath.Path).Returns(false);

        // Act
        ErrorOr<FileSystemPathId> result = _sut.MoveFile(sourcePath, destinationPath, overrideExisting);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(Errors.FileSystemManagement.FileNotFound, result.FirstError);
    }

    [Fact]
    public void MoveFile_WhenDestinationFileDoesNotExist_ShouldPerformSimpleMove()
    {
        // Arrange
        FileSystemPathId sourcePath = _fileSystemPathIdFixture.CreateFileSystemPathId(_pathSourceFile);
        FileSystemPathId destinationPath = _fileSystemPathIdFixture.CreateFileSystemPathId(_pathDestination);
        string destinationFilePath = _pathDestinationFile;
        bool overrideExisting = false;

        _mockFileSystem.File.Exists(sourcePath.Path).Returns(true);
        _mockFileSystem.File.Exists(destinationFilePath).Returns(false);
        _mockFileSystem.Path.GetFileName(sourcePath.Path).Returns("file.txt");
        _mockFileSystem.Path.Combine(destinationPath.Path, "file.txt").Returns(destinationFilePath);

        // Act
        ErrorOr<FileSystemPathId> result = _sut.MoveFile(sourcePath, destinationPath, overrideExisting);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(destinationFilePath, result.Value.Path);
        _mockFileSystem.File.Received(1).Move(sourcePath.Path, destinationFilePath);
    }

    [Fact]
    public void MoveFile_WhenDestinationFileExistsAndOverrideIsTrue_ShouldOverwriteFile()
    {
        // Arrange
        FileSystemPathId sourcePath = _fileSystemPathIdFixture.CreateFileSystemPathId(_pathSourceFile);
        FileSystemPathId destinationPath = _fileSystemPathIdFixture.CreateFileSystemPathId(_pathDestination);
        string destinationFilePath = _pathDestinationFile;
        bool overrideExisting = true;

        _mockFileSystem.File.Exists(sourcePath.Path).Returns(true);
        _mockFileSystem.File.Exists(destinationFilePath).Returns(true);
        _mockFileSystem.Path.GetFileName(sourcePath.Path).Returns("file.txt");
        _mockFileSystem.Path.Combine(destinationPath.Path, "file.txt").Returns(destinationFilePath);

        // Act
        ErrorOr<FileSystemPathId> result = _sut.MoveFile(sourcePath, destinationPath, overrideExisting);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(destinationFilePath, result.Value.Path);
        _mockFileSystem.File.Received(1).Move(sourcePath.Path, destinationFilePath, overrideExisting);
    }

    [Fact]
    public void MoveFile_WhenDestinationFileExistsAndOverrideIsFalse_ShouldReturnFileAlreadyExistsError()
    {
        // Arrange
        FileSystemPathId sourcePath = _fileSystemPathIdFixture.CreateFileSystemPathId(_pathSourceFile);
        FileSystemPathId destinationPath = _fileSystemPathIdFixture.CreateFileSystemPathId(_pathDestination);
        string destinationFilePath = _pathDestinationFile;
        bool overrideExisting = false;

        _mockFileSystem.File.Exists(sourcePath.Path).Returns(true);
        _mockFileSystem.File.Exists(destinationFilePath).Returns(true);
        _mockFileSystem.Path.GetFileName(sourcePath.Path).Returns("file.txt");
        _mockFileSystem.Path.Combine(destinationPath.Path, "file.txt").Returns(destinationFilePath);

        // Act
        ErrorOr<FileSystemPathId> result = _sut.MoveFile(sourcePath, destinationPath, overrideExisting);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(Errors.FileSystemManagement.FileAlreadyExists, result.FirstError);
    }

    [Fact]
    public void MoveFile_WhenExceptionOccurs_ShouldReturnFileMoveError()
    {
        // Arrange
        FileSystemPathId sourcePath = _fileSystemPathIdFixture.CreateFileSystemPathId(_pathSourceFile);
        FileSystemPathId destinationPath = _fileSystemPathIdFixture.CreateFileSystemPathId(_pathDestination);
        string destinationFilePath = _pathDestinationFile;
        bool overrideExisting = false;

        _mockFileSystem.File.Exists(sourcePath.Path).Returns(true);
        _mockFileSystem.File.Exists(destinationFilePath).Returns(false);
        _mockFileSystem.Path.GetFileName(sourcePath.Path).Returns("file.txt");
        _mockFileSystem.Path.Combine(destinationPath.Path, "file.txt").Returns(destinationFilePath);
        _mockFileSystem.File.When(x => x.Move(Arg.Any<string>(), Arg.Any<string>()))
            .Do(x => throw new IOException("Simulated IO error"));

        // Act
        ErrorOr<FileSystemPathId> result = _sut.MoveFile(sourcePath, destinationPath, overrideExisting);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(Errors.FileSystemManagement.FileMoveError, result.FirstError);
    }

    [Fact]
    public void RenameFile_WhenParentDirectoryIsWritable_ShouldRenamesuccessfuly()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId(
            s_isUnix ? "/OldName.txt" : @"C:\OldName.txt"
        );
        string newName = "NewName.txt";
        string parentPath = s_isUnix ? "/" : @"C:\";
        string newPath = s_isUnix ? "/NewName.txt" : @"C:\NewName.txt";

        IFileInfo fileInfo = Substitute.For<IFileInfo>();
        fileInfo.DirectoryName.Returns(parentPath);
        _mockFileSystem.FileInfo.New(path.Path).Returns(fileInfo);
        _mockFileSystem.Path.Combine(parentPath, newName).Returns(newPath);
        _mockFileSystemPermissionsService.CanAccessPath(Arg.Any<FileSystemPathId>(), FileAccessMode.Write).Returns(true);
        _mockFileSystemPermissionsService.CanAccessPath(path, FileAccessMode.Execute).Returns(true);

        // Act
        ErrorOr<FileSystemPathId> result = _sut.RenameFile(path, newName);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(newPath, result.Value.Path);
        _mockFileSystem.File.Received(1).Move(path.Path, newPath);
    }

    [Fact]
    public void RenameFile_WhenParentDirectoryIsNotWritable_ShouldReturnUnauthorizedAccessError()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId(
            s_isUnix ? "/OldName.txt" : @"C:\OldName.txt"
        );
        string newName = "NewName.txt";
        string parentPath = s_isUnix ? "/" : @"C:\";
        string newPath = s_isUnix ? "/NewName.txt" : @"C:\NewName.txt";

        IFileInfo fileInfo = Substitute.For<IFileInfo>();
        fileInfo.DirectoryName.Returns(parentPath);
        _mockFileSystem.FileInfo.New(path.Path).Returns(fileInfo);
        _mockFileSystem.Path.Combine(parentPath, newName).Returns(newPath);
        _mockFileSystemPermissionsService.CanAccessPath(Arg.Any<FileSystemPathId>(), FileAccessMode.Write).Returns(false);

        // Act
        ErrorOr<FileSystemPathId> result = _sut.RenameFile(path, newName);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(Errors.Permission.UnauthorizedAccess, result.FirstError);
        _mockFileSystem.File.DidNotReceive().Move(Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public void RenameFile_WhenFileIsNotExecutable_ShouldReturnUnauthorizedAccessError()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId(
            s_isUnix ? "/OldName.txt" : @"C:\OldName.txt"
        );
        string newName = "NewName.txt";
        string parentPath = s_isUnix ? "/" : @"C:\";
        string newPath = s_isUnix ? "/NewName.txt" : @"C:\NewName.txt";

        IFileInfo fileInfo = Substitute.For<IFileInfo>();
        fileInfo.DirectoryName.Returns(parentPath);
        _mockFileSystem.FileInfo.New(path.Path).Returns(fileInfo);
        _mockFileSystem.Path.Combine(parentPath, newName).Returns(newPath);
        _mockFileSystemPermissionsService.CanAccessPath(Arg.Any<FileSystemPathId>(), FileAccessMode.Write).Returns(true);
        _mockFileSystemPermissionsService.CanAccessPath(path, FileAccessMode.Execute).Returns(false);

        // Act
        ErrorOr<FileSystemPathId> result = _sut.RenameFile(path, newName);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(Errors.Permission.UnauthorizedAccess, result.FirstError);
        _mockFileSystem.File.DidNotReceive().Move(Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public void RenameFile_WhenParentDirectoryIsNull_ShouldReturnInvalidPathError()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId(
            s_isUnix ? "/OldName.txt" : @"C:\OldName.txt"
        );
        string newName = "NewName.txt";

        _mockFileSystem.FileInfo.New(path.Path).Returns((IFileInfo)null!);

        // Act
        ErrorOr<FileSystemPathId> result = _sut.RenameFile(path, newName);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(Errors.FileSystemManagement.CannotNavigateUp, result.FirstError);
        _mockFileSystem.File.DidNotReceive().Move(Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public void RenameFile_WhenNewPathIsInvalid_ShouldReturnInvalidPathError()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId(
            s_isUnix ? "/OldName.txt" : @"C:\OldName.txt"
        );
        string newName = "NewName.txt";
        string parentPath = s_isUnix ? "/" : @"C:\";

        IFileInfo fileInfo = Substitute.For<IFileInfo>();
        fileInfo.DirectoryName.Returns(parentPath);
        _mockFileSystem.FileInfo.New(path.Path).Returns(fileInfo);
        _mockFileSystem.Path.Combine(parentPath, newName).Returns((string)null!);

        // Act
        ErrorOr<FileSystemPathId> result = _sut.RenameFile(path, newName);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(Errors.FileSystemManagement.InvalidPath, result.FirstError);
        _mockFileSystem.File.DidNotReceive().Move(Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public void RenameFile_WhenParentDirectoryExistsButNotWritable_ShouldReturnUnauthorizedAccessError()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId(
            s_isUnix ? "/OldName.txt" : @"C:\OldName.txt"
        );
        string newName = "NewName.txt";
        string parentPath = s_isUnix ? "/" : @"C:\";
        string newPath = s_isUnix ? "/NewName.txt" : @"C:\NewName.txt";

        IFileInfo fileInfo = Substitute.For<IFileInfo>();
        fileInfo.DirectoryName.Returns(parentPath);
        _mockFileSystem.FileInfo.New(path.Path).Returns(fileInfo);
        _mockFileSystem.Path.Combine(parentPath, newName).Returns(newPath);

        // Mock successful creation of parent directory FileSystemPathId
        ErrorOr<FileSystemPathId> parentDirectoryPathId = FileSystemPathId.Create(parentPath);
        _mockFileSystem.Path.GetDirectoryName(path.Path).Returns(parentPath);

        // Mock permissions: executable for the file, but not writable for the parent directory
        _mockFileSystemPermissionsService.CanAccessPath(path, FileAccessMode.Execute).Returns(true);
        _mockFileSystemPermissionsService.CanAccessPath(Arg.Is<FileSystemPathId>(x => x.Path == parentPath), FileAccessMode.Write).Returns(false);

        // Act
        ErrorOr<FileSystemPathId> result = _sut.RenameFile(path, newName);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(Errors.Permission.UnauthorizedAccess, result.FirstError);
        _mockFileSystem.File.DidNotReceive().Move(Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public void DeleteFile_WhenFileExistsAndHasDeletePermission_ShouldDeletesuccessfuly()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId(
            s_isUnix ? "/FileToDelete.txt" : @"C:\FileToDelete.txt"
        );
        _mockFileSystemPermissionsService.CanAccessPath(path, FileAccessMode.Delete).Returns(true);
        _mockFileSystem.File.Exists(path.Path).Returns(true);

        // Act
        ErrorOr<Deleted> result = _sut.DeleteFile(path);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(Result.Deleted, result.Value);
        _mockFileSystem.File.Received(1).Delete(path.Path);
    }

    [Fact]
    public void DeleteFile_WhenNoDeletePermission_ShouldReturnUnauthorizedAccessError()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId(
            s_isUnix ? "/FileToDelete.txt" : @"C:\FileToDelete.txt"
        );
        _mockFileSystemPermissionsService.CanAccessPath(path, FileAccessMode.Delete).Returns(false);

        // Act
        ErrorOr<Deleted> result = _sut.DeleteFile(path);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(Errors.Permission.UnauthorizedAccess, result.FirstError);
        _mockFileSystem.File.DidNotReceive().Delete(Arg.Any<string>());
    }
}
