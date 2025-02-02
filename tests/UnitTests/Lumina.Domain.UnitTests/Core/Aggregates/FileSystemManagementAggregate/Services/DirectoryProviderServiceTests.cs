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
/// Contains unit tests for the <see cref="DirectoryProviderService"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class DirectoryProviderServiceTests
{
    private readonly IFixture _fixture;
    private readonly IFileSystem _mockFileSystem;
    private readonly IFileSystemPermissionsService _mockFileSystemPermissionsService;
    private readonly DirectoryProviderService _sut;
    private readonly FileSystemPathIdFixture _fileSystemPathIdFixture;
    private static readonly bool s_isUnix = RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
    private readonly string _pathSource = s_isUnix ? "/Source" : @"C:\Source";
    private readonly string _pathSourceSubDir = s_isUnix ? "/Source/SubDir" : @"C:\Source\SubDir";
    private readonly string _pathSourceSubDir1 = s_isUnix ? "/Source/SubDir1" : @"C:\Source\SubDir1";
    private readonly string _pathSourceSubDir2 = s_isUnix ? "/Source/SubDir2" : @"C:\Source\SubDir2";
    private readonly string _pathSourceFile1 = s_isUnix ? "/Source/file1.txt" : @"C:\Source\file1.txt";
    private readonly string _pathSourceFile2 = s_isUnix ? "/Source/file2.txt" : @"C:\Source\file2.txt";
    private readonly string _pathDestinationFile1 = s_isUnix ? "/Destination/file1.txt" : @"C:\Destination\file1.txt";
    private readonly string _pathDestinationFile2 = s_isUnix ? "/Destination/file2.txt" : @"C:\Destination\file2.txt";
    private readonly string _pathDestination = s_isUnix ? "/Destination" : @"C:\Destination";
    private readonly string _pathDestinationSubDir = s_isUnix ? "/Destination/SubDir" : @"C:\Destination\SubDir";
    private readonly string _pathDestinationSubDir1 = s_isUnix ? "/Destination/SubDir1" : @"C:\Destination\SubDir1";
    private readonly string _pathDestinationSubDir2 = s_isUnix ? "/Destination/SubDir2" : @"C:\Destination\SubDir2";
    private readonly string _pathDestinationFile = s_isUnix ? "/Destination/file.txt" : @"C:\Destination\file.txt";
    private readonly string _pathVisible1 = s_isUnix ? "/Visible" : @"C:\Visible";
    private readonly string _pathHidden1 = s_isUnix ? "/Hidden" : @"C:\Hidden";
    private readonly string _pathVisible2 = s_isUnix ? "/Visible/" : @"C:\Visible\";
    private readonly string _pathHidden2 = s_isUnix ? "/Hidden/" : @"C:\Hidden\";
    private readonly string _pathValid1 = s_isUnix ? "/Valid" : @"C:\Valid";
    private readonly string _pathInvalid1 = s_isUnix ? "/Invalid" : @"C:\Invalid";
    private readonly string _pathValid2 = s_isUnix ? "/Valid/" : @"C:\Valid\";
    private readonly string _pathInvalid2 = s_isUnix ? "/Invalid/" : @"C:\Invalid\";
    private readonly char _dirSeparator = s_isUnix ? '/' : '\\';

    /// <summary>
    /// Initializes a new instance of the <see cref="DirectoryProviderServiceTests"/> class.
    /// </summary>
    public DirectoryProviderServiceTests()
    {
        _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
        _mockFileSystem = Substitute.For<IFileSystem>();
        _mockFileSystemPermissionsService = Substitute.For<IFileSystemPermissionsService>();
        _sut = new DirectoryProviderService(_mockFileSystem, _mockFileSystemPermissionsService);
        _fileSystemPathIdFixture = new();
    }

    [Fact]
    public void GetSubdirectoryPaths_WhenUserHasNoAccess_ShouldReturnUnauthorizedAccessError()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId();
        _mockFileSystemPermissionsService.CanAccessPath(path, FileAccessMode.ListDirectory, false).Returns(false);

        // Act
        ErrorOr<IEnumerable<FileSystemPathId>> result = _sut.GetSubdirectoryPaths(path, false);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(Errors.Permission.UnauthorizedAccess, result.FirstError);
    }

    [Fact]
    public void GetSubdirectoryPaths_WhenUserHasAccess_ShouldReturnSubdirectories()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId();
        string[] subdirectories = _fixture.CreateMany<string>(3).ToArray();
        _mockFileSystemPermissionsService.CanAccessPath(path, FileAccessMode.ListDirectory, false).Returns(true);
        _mockFileSystem.Directory.GetDirectories(path.Path).Returns(subdirectories);
        _mockFileSystem.Path.DirectorySeparatorChar.Returns(_dirSeparator);

        // Act
        ErrorOr<IEnumerable<FileSystemPathId>> result = _sut.GetSubdirectoryPaths(path, false);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(3, result.Value.Count());
        Assert.Equal(
            subdirectories.Select(x => x + _dirSeparator).OrderBy(x => x),
            result.Value.Select(x => x.Path).OrderBy(x => x)
        );
    }

    [Fact]
    public void GetSubdirectoryPaths_WhenIncludeHiddenElementsIsFalse_ShouldExcludeHiddenDirectories()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId();
        string[] subdirectories = [_pathVisible1, _pathHidden1];
        _mockFileSystemPermissionsService.CanAccessPath(path, FileAccessMode.ListDirectory, false).Returns(true);
        _mockFileSystem.Directory.GetDirectories(path.Path).Returns(subdirectories);
        _mockFileSystem.Path.DirectorySeparatorChar.Returns(_dirSeparator);
        _mockFileSystem.File.GetAttributes(_pathVisible1).Returns(FileAttributes.Directory);
        _mockFileSystem.File.GetAttributes(_pathHidden1).Returns(FileAttributes.Directory | FileAttributes.Hidden);

        // Act
        ErrorOr<IEnumerable<FileSystemPathId>> result = _sut.GetSubdirectoryPaths(path, false);

        // Assert
        Assert.False(result.IsError);
        Assert.Single(result.Value);
        Assert.Equal(_pathVisible2, result.Value.First().Path);
    }

    [Fact]
    public void GetSubdirectoryPaths_WhenIncludeHiddenElementsIsTrue_ShouldIncludeHiddenDirectories()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId();
        string[] subdirectories = [_pathVisible1, _pathHidden1];
        _mockFileSystemPermissionsService.CanAccessPath(path, FileAccessMode.ListDirectory, false).Returns(true);
        _mockFileSystem.Directory.GetDirectories(path.Path).Returns(subdirectories);
        _mockFileSystem.Path.DirectorySeparatorChar.Returns(_dirSeparator);
        _mockFileSystem.File.GetAttributes(_pathVisible1).Returns(FileAttributes.Directory);
        _mockFileSystem.File.GetAttributes(_pathHidden1).Returns(FileAttributes.Directory | FileAttributes.Hidden);

        // Act
        ErrorOr<IEnumerable<FileSystemPathId>> result = _sut.GetSubdirectoryPaths(path, true);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(2, result.Value.Count());
        Assert.Equal(
            new[] { _pathVisible2, _pathHidden2 }.OrderBy(p => p),
            result.Value.Select(x => x.Path).OrderBy(p => p)
        );
    }

    [Fact]
    public void GetSubdirectoryPaths_WhenGetAttributesThrowsException_ShouldStillAddDirectory()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId();
        string[] subdirectories = [_pathValid1, _pathInvalid1];
        _mockFileSystemPermissionsService.CanAccessPath(path, FileAccessMode.ListDirectory, false).Returns(true);
        _mockFileSystem.Directory.GetDirectories(path.Path).Returns(subdirectories);
        _mockFileSystem.Path.DirectorySeparatorChar.Returns(_dirSeparator);
        _mockFileSystem.File.GetAttributes(_pathValid1).Returns(FileAttributes.Directory);
        _mockFileSystem.File.GetAttributes(_pathInvalid1).Throws(new Exception("Access denied"));

        // Act
        ErrorOr<IEnumerable<FileSystemPathId>> result = _sut.GetSubdirectoryPaths(path, false);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(2, result.Value.Count());
        Assert.Equal(_pathInvalid2, result.Value.First().Path);
        Assert.Equal(_pathValid2, result.Value.Last().Path);
    }

    [Fact]
    public void GetSubdirectoryPaths_WhenCalled_ShouldOrderDirectoriesAlphabetically()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId();
        string[] subdirectories = s_isUnix
            ? ["/C", "/A", "/B"]
            : [@"C:\C", @"C:\A", @"C:\B"];
        _mockFileSystemPermissionsService.CanAccessPath(path, FileAccessMode.ListDirectory, false).Returns(true);
        _mockFileSystem.Directory.GetDirectories(path.Path).Returns(subdirectories);
        _mockFileSystem.Path.DirectorySeparatorChar.Returns(_dirSeparator);
        _mockFileSystem.File.GetAttributes(Arg.Any<string>()).Returns(FileAttributes.Directory);
        string[] expectedPaths = s_isUnix
            ? ["/A/", "/B/", "/C/"]
            : [@"C:\A\", @"C:\B\", @"C:\C\"];

        // Act
        ErrorOr<IEnumerable<FileSystemPathId>> result = _sut.GetSubdirectoryPaths(path, false);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(expectedPaths, result.Value.Select(x => x.Path));
    }

    [Fact]
    public void GetSubdirectoryPaths_WhenDirectoryHasNoSubdirectories_ShouldReturnEmptyList()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId();
        _mockFileSystemPermissionsService.CanAccessPath(path, FileAccessMode.ListDirectory, false).Returns(true);
        _mockFileSystem.Directory.GetDirectories(path.Path).Returns([]);

        // Act
        ErrorOr<IEnumerable<FileSystemPathId>> result = _sut.GetSubdirectoryPaths(path, false);

        // Assert
        Assert.False(result.IsError);
        Assert.Empty(result.Value);
    }

    [Fact]
    public void DirectoryExists_WhenDirectoryExists_ShouldReturnTrue()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId();
        _mockFileSystem.Directory.Exists(path.Path).Returns(true);

        // Act
        ErrorOr<bool> result = _sut.DirectoryExists(path);

        // Assert
        Assert.False(result.IsError);
        Assert.True(result.Value);
    }

    [Fact]
    public void DirectoryExists_WhenDirectoryDoesNotExist_ShouldReturnFalse()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId();
        _mockFileSystem.Directory.Exists(path.Path).Returns(false);

        // Act
        ErrorOr<bool> result = _sut.DirectoryExists(path);

        // Assert
        Assert.False(result.IsError);
        Assert.False(result.Value);
    }

    [Fact]
    public void GetFileName_WhenPathIsValid_ShouldReturnFileName()
    {
        // Arrange
        string fullPath = s_isUnix
            ? "/folder/subfolder/file.txt"
            : @"C:\folder\subfolder\file.txt";
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId(fullPath);
        _mockFileSystem.Path.GetFileName(fullPath).Returns("file.txt");

        // Act
        ErrorOr<string> result = _sut.GetFileName(path);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal("file.txt", result.Value);
    }

    [Fact]
    public void GetFileName_WhenPathEndsWithDirectorySeparator_ShouldReturnLastSegment()
    {
        // Arrange
        string fullPath = s_isUnix
            ? "/folder/subfolder/"
            : @"C:\folder\subfolder\";
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId(fullPath);
        _mockFileSystem.Path.GetFileName(fullPath[..^1]).Returns("subfolder");
        _mockFileSystem.Path.DirectorySeparatorChar.Returns(_dirSeparator);

        // Act
        ErrorOr<string> result = _sut.GetFileName(path);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal("subfolder", result.Value);
    }

    [Fact]
    public void GetFileName_WhenPathIsRoot_ShouldReturnEmptyString()
    {
        // Arrange
        string fullPath = s_isUnix
            ? "/C/"
            : @"/";
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId(fullPath);
        _mockFileSystem.Path.GetFileName(fullPath[..^1]).Returns(string.Empty);
        _mockFileSystem.Path.DirectorySeparatorChar.Returns(_dirSeparator);

        // Act
        ErrorOr<string> result = _sut.GetFileName(path);

        // Assert
        Assert.False(result.IsError);
        Assert.Empty(result.Value);
    }

    [Fact]
    public void GetLastWriteTime_WhenUserHasAccess_ShouldReturnLastWriteTime()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId();
        DateTime expectedDateTime = DateTime.Now;
        _mockFileSystemPermissionsService.CanAccessPath(path, FileAccessMode.ReadProperties, false).Returns(true);
        _mockFileSystem.Directory.GetLastWriteTime(path.Path).Returns(expectedDateTime);

        // Act
        ErrorOr<Optional<DateTime>> result = _sut.GetLastWriteTime(path);

        // Assert
        Assert.False(result.IsError);
        Assert.True(result.Value.HasValue);
        Assert.Equal(expectedDateTime, result.Value.Value);
    }

    [Fact]
    public void GetLastWriteTime_WhenUserHasNoAccess_ShouldReturnUnauthorizedAccessError()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId();
        _mockFileSystemPermissionsService.CanAccessPath(path, FileAccessMode.ReadProperties, false).Returns(false);

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
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId();
        DateTime expectedDateTime = DateTime.Now;
        _mockFileSystemPermissionsService.CanAccessPath(path, FileAccessMode.ReadProperties, false).Returns(true);
        _mockFileSystem.Directory.GetCreationTime(path.Path).Returns(expectedDateTime);

        // Act
        ErrorOr<Optional<DateTime>> result = _sut.GetCreationTime(path);

        // Assert
        Assert.False(result.IsError);
        Assert.True(result.Value.HasValue);
        Assert.Equal(expectedDateTime, result.Value.Value);
    }

    [Fact]
    public void GetCreationTime_WhenUserHasNoAccess_ShouldReturnUnauthorizedAccessError()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId();
        _mockFileSystemPermissionsService.CanAccessPath(path, FileAccessMode.ReadProperties, false).Returns(false);

        // Act
        ErrorOr<Optional<DateTime>> result = _sut.GetCreationTime(path);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(Errors.Permission.UnauthorizedAccess, result.FirstError);
    }

    [Fact]
    public void CreateDirectory_WhenUserHasAccess_ShouldCreateDirectoryAndReturnPath()
    {
        // Arrange
        FileSystemPathId parentPath = _fileSystemPathIdFixture.CreateFileSystemPathId(@"C:\Parent");
        string directoryName = "NewDirectory";
        string fullPath = s_isUnix
            ? "/Parent/NewDirectory"
            : @"C:\Parent\NewDirectory";

        _mockFileSystemPermissionsService.CanAccessPath(parentPath, FileAccessMode.Write, false).Returns(true);
        _mockFileSystem.Path.Combine(parentPath.Path, directoryName).Returns(fullPath);

        IDirectoryInfo mockDirectoryInfo = Substitute.For<IDirectoryInfo>();
        mockDirectoryInfo.FullName.Returns(fullPath);
        _mockFileSystem.Directory.CreateDirectory(fullPath).Returns(mockDirectoryInfo);

        // Act
        ErrorOr<FileSystemPathId> result = _sut.CreateDirectory(parentPath, directoryName);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(fullPath, result.Value.Path);
        _mockFileSystem.Directory.Received(1).CreateDirectory(fullPath);
    }

    [Fact]
    public void CreateDirectory_WhenUserHasNoAccess_ShouldReturnUnauthorizedAccessError()
    {
        // Arrange
        FileSystemPathId parentPath = _fileSystemPathIdFixture.CreateFileSystemPathId();
        string directoryName = "NewDirectory";

        _mockFileSystemPermissionsService.CanAccessPath(parentPath, FileAccessMode.Write, false).Returns(false);

        // Act
        ErrorOr<FileSystemPathId> result = _sut.CreateDirectory(parentPath, directoryName);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(Errors.Permission.UnauthorizedAccess, result.FirstError);
        _mockFileSystem.Directory.DidNotReceive().CreateDirectory(Arg.Any<string>());
    }

    [Fact]
    public void CreateDirectory_WhenParentPathIsNull_ShouldReturnError()
    {
        // Arrange
        FileSystemPathId parentPath = null!;
        string directoryName = "NewDirectory";

        // Act
        ErrorOr<FileSystemPathId> result = _sut.CreateDirectory(parentPath, directoryName);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(ErrorType.Failure, result.FirstError.Type);
    }

    [Fact]
    public void CreateDirectory_WhenCreatedDirectoryPathIsInvalid_ShouldReturnError()
    {
        // Arrange
        FileSystemPathId parentPath = _fileSystemPathIdFixture.CreateFileSystemPathId(@"C:\Parent");
        string directoryName = "NewDirectory";
        string invalidPath = "Invalid:Path";

        _mockFileSystemPermissionsService.CanAccessPath(parentPath, FileAccessMode.Write, false).Returns(true);
        _mockFileSystem.Path.Combine(parentPath.Path, directoryName).Returns(invalidPath);
        _mockFileSystem.Directory.CreateDirectory(invalidPath).Returns(Substitute.For<IDirectoryInfo>());

        // Act
        ErrorOr<FileSystemPathId> result = _sut.CreateDirectory(parentPath, directoryName);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(ErrorType.Validation, result.FirstError.Type);
    }

    [Fact]
    public void CopyDirectory_WhenSourceDirectoryExistsAndUserHasAccess_ShouldCopyDirectoryAndReturnPath()
    {
        // Arrange
        FileSystemPathId sourcePath = _fileSystemPathIdFixture.CreateFileSystemPathId(_pathSource);
        FileSystemPathId destinationPath = _fileSystemPathIdFixture.CreateFileSystemPathId(_pathDestination);
        bool overrideExisting = false;

        _mockFileSystem.Directory.Exists(sourcePath.Path).Returns(true);
        _mockFileSystem.Directory.Exists(destinationPath.Path).Returns(false);
        _mockFileSystem.Directory.CreateDirectory(destinationPath.Path).Returns(Substitute.For<IDirectoryInfo>());

        IDirectoryInfo sourceDirectoryInfo = Substitute.For<IDirectoryInfo>();
        sourceDirectoryInfo.GetFiles().Returns([]);
        sourceDirectoryInfo.GetDirectories().Returns([]);
        _mockFileSystem.DirectoryInfo.New(sourcePath.Path).Returns(sourceDirectoryInfo);

        _mockFileSystem.Path.Combine(Arg.Any<string>(), Arg.Any<string>())
            .Returns(callInfo => Path.Combine(callInfo.ArgAt<string>(0), callInfo.ArgAt<string>(1)));

        // Act
        ErrorOr<FileSystemPathId> result = _sut.CopyDirectory(sourcePath, destinationPath, overrideExisting);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(destinationPath.Path, result.Value.Path);
        _mockFileSystem.Directory.Received(1).CreateDirectory(destinationPath.Path);
    }

    [Fact]
    public void CopyDirectory_WhenSourceDirectoryDoesNotExist_ShouldReturnError()
    {
        // Arrange
        FileSystemPathId sourcePath = _fileSystemPathIdFixture.CreateFileSystemPathId();
        FileSystemPathId destinationPath = _fileSystemPathIdFixture.CreateFileSystemPathId();
        bool overrideExisting = false;

        _mockFileSystem.Directory.Exists(sourcePath.Path).Returns(false);

        // Act
        ErrorOr<FileSystemPathId> result = _sut.CopyDirectory(sourcePath, destinationPath, overrideExisting);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(Errors.FileSystemManagement.DirectoryNotFound, result.FirstError);
    }

    [Fact]
    public void CopyDirectory_WhenDestinationDirectoryExists_ShouldCreateUniqueDirectoryName()
    {
        // Arrange
        FileSystemPathId sourcePath = _fileSystemPathIdFixture.CreateFileSystemPathId(_pathSource);
        FileSystemPathId destinationPath = _fileSystemPathIdFixture.CreateFileSystemPathId(_pathDestination);
        string expectedNewDirectoryPath = s_isUnix
            ? "/Destination - Copy (1)"
            : @"C:\Destination - Copy (1)";
        bool overrideExisting = false;

        _mockFileSystem.Directory.Exists(sourcePath.Path).Returns(true);
        _mockFileSystem.Directory.Exists(destinationPath.Path).Returns(true);
        _mockFileSystem.Directory.Exists(_pathDestination).Returns(true);
        _mockFileSystem.Directory.Exists(expectedNewDirectoryPath).Returns(false);
        _mockFileSystem.Directory.CreateDirectory(expectedNewDirectoryPath).Returns(Substitute.For<IDirectoryInfo>());

        IDirectoryInfo sourceDirectoryInfo = Substitute.For<IDirectoryInfo>();
        sourceDirectoryInfo.GetFiles().Returns([]);
        sourceDirectoryInfo.GetDirectories().Returns([]);
        _mockFileSystem.DirectoryInfo.New(sourcePath.Path).Returns(sourceDirectoryInfo);

        _mockFileSystem.Path.Combine(Arg.Any<string>(), Arg.Any<string>())
            .Returns(callInfo => Path.Combine(callInfo.ArgAt<string>(0), callInfo.ArgAt<string>(1)));

        // Act
        ErrorOr<FileSystemPathId> result = _sut.CopyDirectory(sourcePath, destinationPath, overrideExisting);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(expectedNewDirectoryPath, result.Value.Path);
        _mockFileSystem.Directory.Received(1).CreateDirectory(expectedNewDirectoryPath);
    }

    [Fact]
    public void CopyDirectory_WhenCopyingFilesAndSubdirectories_ShouldCopyAllContents()
    {
        // Arrange
        FileSystemPathId sourcePath = _fileSystemPathIdFixture.CreateFileSystemPathId(_pathSource);
        FileSystemPathId destinationPath = _fileSystemPathIdFixture.CreateFileSystemPathId(_pathDestination);
        bool overrideExisting = true;


        // ensure all relevant directories exist
        _mockFileSystem.Directory.Exists(_pathSource).Returns(true);
        _mockFileSystem.Directory.Exists(_pathSourceSubDir).Returns(true);
        _mockFileSystem.Directory.Exists(_pathDestination).Returns(false);
        _mockFileSystem.Directory.CreateDirectory(Arg.Any<string>()).Returns(callInfo =>
        {
            IDirectoryInfo mockDirInfo = Substitute.For<IDirectoryInfo>();
            mockDirInfo.FullName.Returns(callInfo.Arg<string>());
            return mockDirInfo;
        });

        IFileInfo mockFile = Substitute.For<IFileInfo>();
        mockFile.Name.Returns("file.txt");
        mockFile.Attributes.Returns(FileAttributes.Normal);

        IDirectoryInfo mockSubDir = Substitute.For<IDirectoryInfo>();
        mockSubDir.Name.Returns("SubDir");
        mockSubDir.FullName.Returns(_pathSourceSubDir);

        IDirectoryInfo sourceDirectoryInfo = Substitute.For<IDirectoryInfo>();
        sourceDirectoryInfo.GetFiles().Returns([mockFile]);
        sourceDirectoryInfo.GetDirectories().Returns([mockSubDir]);
        _mockFileSystem.DirectoryInfo.New(_pathSource).Returns(sourceDirectoryInfo);

        // mock subdirectory info
        IDirectoryInfo subDirInfo = Substitute.For<IDirectoryInfo>();
        subDirInfo.GetFiles().Returns([]);
        subDirInfo.GetDirectories().Returns([]);
        _mockFileSystem.DirectoryInfo.New(_pathSourceSubDir).Returns(subDirInfo);

        // mock the Path.Combine method for all necessary cases
        _mockFileSystem.Path.Combine(Arg.Any<string>(), Arg.Any<string>())
            .Returns(callInfo => Path.Combine(callInfo.ArgAt<string>(0), callInfo.ArgAt<string>(1)));

        // Act
        ErrorOr<FileSystemPathId> result = _sut.CopyDirectory(sourcePath, destinationPath, overrideExisting);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(_pathDestination, result.Value.Path);
        _mockFileSystem.Directory.Received(1).CreateDirectory(_pathDestination);
        _mockFileSystem.Directory.Received(1).CreateDirectory(_pathDestinationSubDir);
        mockFile.Received(1).CopyTo(_pathDestinationFile, overrideExisting);
        _mockFileSystem.File.Received(1).SetAttributes(_pathDestinationFile, FileAttributes.Normal);
    }

    [Fact]
    public void CopyDirectory_WhenExceptionOccurs_ShouldReturnError()
    {
        // Arrange
        FileSystemPathId sourcePath = _fileSystemPathIdFixture.CreateFileSystemPathId();
        FileSystemPathId destinationPath = _fileSystemPathIdFixture.CreateFileSystemPathId();
        bool overrideExisting = false;

        _mockFileSystem.Directory.Exists(sourcePath.Path).Returns(true);
        _mockFileSystem.Directory.CreateDirectory(Arg.Any<string>()).Throws(new Exception("Simulated error"));

        // Act
        ErrorOr<FileSystemPathId> result = _sut.CopyDirectory(sourcePath, destinationPath, overrideExisting);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(Errors.FileSystemManagement.DirectoryCopyError, result.FirstError);
    }

    [Fact]
    public void MoveDirectory_WhenSourceDirectoryDoesNotExist_ShouldReturnDirectoryNotFoundError()
    {
        // Arrange
        FileSystemPathId sourcePath = _fileSystemPathIdFixture.CreateFileSystemPathId(
            s_isUnix ? "/NonExistentSource" : @"C:\NonExistentSource"
        );
        FileSystemPathId destinationPath = _fileSystemPathIdFixture.CreateFileSystemPathId(_pathDestination);
        bool overrideExisting = false;

        _mockFileSystem.Directory.Exists(sourcePath.Path).Returns(false);

        // Act
        ErrorOr<FileSystemPathId> result = _sut.MoveDirectory(sourcePath, destinationPath, overrideExisting);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(Errors.FileSystemManagement.DirectoryNotFound, result.FirstError);
    }

    [Fact]
    public void MoveDirectory_WhenDestinationDirectoryDoesNotExist_ShouldPerformSimpleMove()
    {
        // Arrange
        FileSystemPathId sourcePath = _fileSystemPathIdFixture.CreateFileSystemPathId(_pathSource);
        FileSystemPathId destinationPath = _fileSystemPathIdFixture.CreateFileSystemPathId(_pathDestination);
        bool overrideExisting = false;

        _mockFileSystem.Directory.Exists(sourcePath.Path).Returns(true);
        _mockFileSystem.Directory.Exists(destinationPath.Path).Returns(false);

        // Act
        ErrorOr<FileSystemPathId> result = _sut.MoveDirectory(sourcePath, destinationPath, overrideExisting);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(destinationPath.Path, result.Value.Path);
        _mockFileSystem.Directory.Received(1).Move(sourcePath.Path, destinationPath.Path);
    }

    [Fact]
    public void MoveDirectory_WhenDestinationDirectoryExists_ShouldMergeDirectories()
    {
        // Arrange
        FileSystemPathId sourcePath = _fileSystemPathIdFixture.CreateFileSystemPathId(_pathSource);
        FileSystemPathId destinationPath = _fileSystemPathIdFixture.CreateFileSystemPathId(_pathDestination);
        bool overrideExisting = true;

        _mockFileSystem.Directory.Exists(sourcePath.Path).Returns(true);
        _mockFileSystem.Directory.Exists(destinationPath.Path).Returns(true);

        _mockFileSystem.Directory.GetFiles(sourcePath.Path).Returns([_pathSourceFile1]);
        _mockFileSystem.Directory.GetDirectories(sourcePath.Path).Returns([_pathSourceSubDir]);

        _mockFileSystem.Directory.GetFiles(_pathSourceSubDir).Returns([]);
        _mockFileSystem.Directory.GetDirectories(_pathSourceSubDir).Returns([]);

        _mockFileSystem.Path.GetFileName(_pathSourceFile1).Returns("file1.txt");
        _mockFileSystem.Path.GetFileName(_pathSourceSubDir).Returns("SubDir");

        _mockFileSystem.Path.Combine(Arg.Any<string>(), Arg.Any<string>())
            .Returns(callInfo => Path.Combine(callInfo.ArgAt<string>(0), callInfo.ArgAt<string>(1)));

        _mockFileSystem.Directory.EnumerateFileSystemEntries(sourcePath.Path).Returns([]);

        // Act
        ErrorOr<FileSystemPathId> result = _sut.MoveDirectory(sourcePath, destinationPath, overrideExisting);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(destinationPath.Path, result.Value.Path);
        _mockFileSystem.File.Received(1).Move(_pathSourceFile1, _pathDestinationFile1);
        _mockFileSystem.Directory.Received(1).Move(
            _pathSourceSubDir,
            s_isUnix ? "/Destination/SubDir" : @"C:\Destination\SubDir"
        );
        _mockFileSystem.Directory.Received(1).Delete(sourcePath.Path);
    }

    [Fact]
    public void MoveDirectory_WhenExceptionOccurs_ShouldReturnDirectoryMoveError()
    {
        // Arrange
        FileSystemPathId sourcePath = _fileSystemPathIdFixture.CreateFileSystemPathId(_pathSource);
        FileSystemPathId destinationPath = _fileSystemPathIdFixture.CreateFileSystemPathId(_pathDestination);
        bool overrideExisting = false;

        _mockFileSystem.Directory.Exists(sourcePath.Path).Returns(true);
        _mockFileSystem.Directory.Exists(destinationPath.Path).Returns(false);
        _mockFileSystem.Directory.When(x => x.Move(Arg.Any<string>(), Arg.Any<string>()))
            .Do(x => throw new IOException("Simulated IO error"));

        // Act
        ErrorOr<FileSystemPathId> result = _sut.MoveDirectory(sourcePath, destinationPath, overrideExisting);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(Errors.FileSystemManagement.DirectoryMoveError, result.FirstError);
    }

    [Fact]
    public void MergeDirectories_WhenSourceContainsFilesAndSubdirectories_ShouldMergeCorrectly()
    {
        // Arrange
        FileSystemPathId sourcePath = _fileSystemPathIdFixture.CreateFileSystemPathId(_pathSource);
        FileSystemPathId destPath = _fileSystemPathIdFixture.CreateFileSystemPathId(_pathDestination);
        bool overwrite = true;

        _mockFileSystem.Directory.Exists(sourcePath.Path).Returns(true);
        _mockFileSystem.Directory.Exists(destPath.Path).Returns(true);

        _mockFileSystem.Directory.GetFiles(sourcePath.Path).Returns([_pathSourceFile1, _pathSourceFile2]);
        _mockFileSystem.Directory.GetDirectories(sourcePath.Path).Returns([_pathSourceSubDir1, _pathSourceSubDir2]);

        _mockFileSystem.Path.GetFileName(_pathSourceFile1).Returns("file1.txt");
        _mockFileSystem.Path.GetFileName(_pathSourceFile2).Returns("file2.txt");
        _mockFileSystem.Path.GetFileName(_pathSourceSubDir1).Returns("SubDir1");
        _mockFileSystem.Path.GetFileName(_pathSourceSubDir2).Returns("SubDir2");

        _mockFileSystem.File.Exists(_pathDestinationFile1).Returns(true);
        _mockFileSystem.File.Exists(_pathDestinationFile2).Returns(false);

        _mockFileSystem.Directory.Exists(_pathDestinationSubDir1).Returns(true);
        _mockFileSystem.Directory.Exists(_pathDestinationSubDir2).Returns(false);

        _mockFileSystem.Path.Combine(Arg.Any<string>(), Arg.Any<string>())
            .Returns(callInfo => Path.Combine(callInfo.ArgAt<string>(0), callInfo.ArgAt<string>(1)));

        _mockFileSystem.Directory.EnumerateFileSystemEntries(sourcePath.Path).Returns([]);

        // Act
        ErrorOr<FileSystemPathId> result = _sut.MoveDirectory(sourcePath, destPath, overwrite);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(destPath, result.Value);
        _mockFileSystem.File.Received(1).Delete(_pathDestinationFile1);
        _mockFileSystem.File.Received(1).Move(_pathSourceFile1, _pathDestinationFile1);
        _mockFileSystem.File.Received(1).Move(_pathSourceFile2, _pathDestinationFile2);
        _mockFileSystem.Directory.Received(1).Move(_pathSourceSubDir2, _pathDestinationSubDir2);
        _mockFileSystem.Directory.Received(1).Delete(sourcePath.Path);
    }

    [Fact]
    public void MoveDirectory_WhenOverwriteIsFalse_ShouldMoveOnlyNonExistingFiles()
    {
        // Arrange
        FileSystemPathId sourcePath = _fileSystemPathIdFixture.CreateFileSystemPathId(_pathSource);
        FileSystemPathId destPath = _fileSystemPathIdFixture.CreateFileSystemPathId(_pathDestination);
        bool overwrite = false;

        _mockFileSystem.Directory.Exists(sourcePath.Path).Returns(true);
        _mockFileSystem.Directory.Exists(destPath.Path).Returns(true);

        _mockFileSystem.Directory.GetFiles(sourcePath.Path).Returns([_pathSourceFile1, _pathSourceFile2]);
        _mockFileSystem.Directory.GetDirectories(sourcePath.Path).Returns([]);

        _mockFileSystem.Path.GetFileName(_pathSourceFile1).Returns("file1.txt");
        _mockFileSystem.Path.GetFileName(_pathSourceFile2).Returns("file2.txt");

        _mockFileSystem.File.Exists(_pathDestinationFile1).Returns(true);
        _mockFileSystem.File.Exists(_pathDestinationFile2).Returns(false);

        _mockFileSystem.Path.Combine(Arg.Any<string>(), Arg.Any<string>())
            .Returns(callInfo => Path.Combine(callInfo.ArgAt<string>(0), callInfo.ArgAt<string>(1)));

        _mockFileSystem.Directory.EnumerateFileSystemEntries(sourcePath.Path).Returns([_pathSourceFile2]);

        // Act
        ErrorOr<FileSystemPathId> result = _sut.MoveDirectory(sourcePath, destPath, overwrite);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(destPath, result.Value);
        _mockFileSystem.File.DidNotReceive().Delete(Arg.Any<string>());
        _mockFileSystem.File.DidNotReceive().Move(_pathSourceFile1, _pathDestinationFile1);
        _mockFileSystem.File.Received(1).Move(_pathSourceFile2, _pathDestinationFile2);
        _mockFileSystem.Directory.DidNotReceive().Delete(sourcePath.Path);
    }

    [Fact]
    public void MergeDirectories_WhenSourceIsEmpty_ShouldDeleteSourceDirectory()
    {
        // Arrange
        string sourceDir = _pathSource;
        string destDir = _pathDestination;
        bool overwrite = true;

        _mockFileSystem.Directory.GetFiles(sourceDir).Returns([]);
        _mockFileSystem.Directory.GetDirectories(sourceDir).Returns([]);
        _mockFileSystem.Directory.EnumerateFileSystemEntries(sourceDir).Returns([]);
        _mockFileSystem.Directory.Exists(sourceDir).Returns(true);
        _mockFileSystem.Directory.Exists(destDir).Returns(true);

        // Act
        _sut.MoveDirectory(FileSystemPathId.Create(sourceDir).Value, FileSystemPathId.Create(destDir).Value, overwrite);

        // Assert
        _mockFileSystem.Directory.Received(1).Delete(sourceDir);
    }

    [Fact]
    public void MergeDirectories_WhenSourceContainsNestedDirectories_ShouldMergeRecursively()
    {
        // Arrange
        FileSystemPathId sourcePath = _fileSystemPathIdFixture.CreateFileSystemPathId(_pathSource);
        FileSystemPathId destPath = _fileSystemPathIdFixture.CreateFileSystemPathId(_pathDestination);
        bool overwrite = true;

        _mockFileSystem.Directory.Exists(sourcePath.Path).Returns(true);
        _mockFileSystem.Directory.Exists(destPath.Path).Returns(true);

        _mockFileSystem.Directory.Exists(_pathSourceSubDir).Returns(true);
        _mockFileSystem.Directory.Exists(s_isUnix ? "/Destination/SubDir" : @"C:\Destination\SubDir").Returns(true);

        _mockFileSystem.Directory.GetFiles(sourcePath.Path).Returns([]);
        _mockFileSystem.Directory.GetDirectories(sourcePath.Path).Returns([_pathSourceSubDir]);

        _mockFileSystem.Directory.GetFiles(_pathSourceSubDir).Returns(
            s_isUnix ? ["/Source/SubDir/file.txt"] : [@"C:\Source\SubDir\file.txt"]
        );
        _mockFileSystem.Directory.GetDirectories(_pathSourceSubDir).Returns([]);

        _mockFileSystem.Path.GetFileName(_pathSourceSubDir).Returns("SubDir");
        _mockFileSystem.Path.GetFileName(s_isUnix ? "/Source/SubDir/file.txt" : @"C:\Source\SubDir\file.txt").Returns("file.txt");

        _mockFileSystem.File.Exists(s_isUnix ? "/Destination/SubDir/file.txt" : @"C:\Destination\SubDir\file.txt").Returns(false);

        _mockFileSystem.Path.Combine(Arg.Any<string>(), Arg.Any<string>())
            .Returns(callInfo => Path.Combine(callInfo.ArgAt<string>(0), callInfo.ArgAt<string>(1)));

        _mockFileSystem.Directory.EnumerateFileSystemEntries(sourcePath.Path).Returns([_pathSourceSubDir]);
        _mockFileSystem.Directory.EnumerateFileSystemEntries(_pathSourceSubDir).Returns([]);

        // Act
        ErrorOr<FileSystemPathId> result = _sut.MoveDirectory(sourcePath, destPath, overwrite);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(destPath, result.Value);
        _mockFileSystem.File.Received(1).Move(
            s_isUnix ? "/Source/SubDir/file.txt" : @"C:\Source\SubDir\file.txt",
            s_isUnix ? "/Destination/SubDir/file.txt" : @"C:\Destination\SubDir\file.txt"
        );
        _mockFileSystem.Directory.Received(1).Delete(_pathSourceSubDir);
        _mockFileSystem.Directory.DidNotReceive().Delete(sourcePath.Path);
    }

    [Fact]
    public void RenameDirectory_WhenParentDirectoryIsWritable_ShouldRenamesuccessfuly()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId(
            s_isUnix ? "/OldName" : @"C:\OldName"
        );
        string newName = "NewName";
        string parentPath = s_isUnix ? "/" : @"C:\";
        string newPath = s_isUnix ? "/NewName" : @"C:\NewName";

        IDirectoryInfo parentDirectoryInfo = Substitute.For<IDirectoryInfo>();
        parentDirectoryInfo.FullName.Returns(parentPath);
        _mockFileSystem.Directory.GetParent(path.Path).Returns(parentDirectoryInfo);
        _mockFileSystem.Path.Combine(parentPath, newName).Returns(newPath);
        _mockFileSystemPermissionsService.CanAccessPath(Arg.Any<FileSystemPathId>(), FileAccessMode.Write, false).Returns(true);
        _mockFileSystemPermissionsService.CanAccessPath(path, FileAccessMode.Execute, false).Returns(true);

        // Act
        ErrorOr<FileSystemPathId> result = _sut.RenameDirectory(path, newName);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(newPath, result.Value.Path);
        _mockFileSystem.Directory.Received(1).Move(path.Path, newPath);
    }

    [Fact]
    public void RenameDirectory_WhenParentDirectoryIsNotWritable_ShouldReturnUnauthorizedAccessError()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId(
            s_isUnix ? "/OldName" : @"C:\OldName"
        );
        string newName = "NewName";
        string parentPath = s_isUnix ? "/" : @"C:\";
        string newPath = s_isUnix ? "/NewName" : @"C:\NewName";

        IDirectoryInfo parentDirectoryInfo = Substitute.For<IDirectoryInfo>();
        parentDirectoryInfo.FullName.Returns(parentPath);
        _mockFileSystem.Directory.GetParent(path.Path).Returns(parentDirectoryInfo);
        _mockFileSystem.Path.Combine(parentPath, newName).Returns(newPath);
        _mockFileSystemPermissionsService.CanAccessPath(Arg.Any<FileSystemPathId>(), FileAccessMode.Write, false).Returns(false);

        // Act
        ErrorOr<FileSystemPathId> result = _sut.RenameDirectory(path, newName);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(Errors.Permission.UnauthorizedAccess, result.FirstError);
        _mockFileSystem.Directory.DidNotReceive().Move(Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public void RenameDirectory_WhenDirectoryIsNotExecutable_ShouldReturnUnauthorizedAccessError()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId(
            s_isUnix ? "/OldName" : @"C:\OldName"
        );
        string newName = "NewName";
        string parentPath = s_isUnix ? "/" : @"C:\";
        string newPath = s_isUnix ? "/NewName" : @"C:\NewName";

        IDirectoryInfo parentDirectoryInfo = Substitute.For<IDirectoryInfo>();
        parentDirectoryInfo.FullName.Returns(parentPath);
        _mockFileSystem.Directory.GetParent(path.Path).Returns(parentDirectoryInfo);
        _mockFileSystem.Path.Combine(parentPath, newName).Returns(newPath);
        _mockFileSystemPermissionsService.CanAccessPath(Arg.Any<FileSystemPathId>(), FileAccessMode.Write, false).Returns(true);
        _mockFileSystemPermissionsService.CanAccessPath(path, FileAccessMode.Execute, false).Returns(false);

        // Act
        ErrorOr<FileSystemPathId> result = _sut.RenameDirectory(path, newName);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(Errors.Permission.UnauthorizedAccess, result.FirstError);
        _mockFileSystem.Directory.DidNotReceive().Move(Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public void RenameDirectory_WhenParentDirectoryIsNull_ShouldReturnInvalidPathError()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId(
            s_isUnix ? "/OldName" : @"C:\OldName"
        );
        string newName = "NewName";

        _mockFileSystem.Directory.GetParent(path.Path).Returns((IDirectoryInfo)null!);

        // Act
        ErrorOr<FileSystemPathId> result = _sut.RenameDirectory(path, newName);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(Errors.FileSystemManagement.InvalidPath, result.FirstError);
        _mockFileSystem.Directory.DidNotReceive().Move(Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public void RenameDirectory_WhenNewPathIsInvalid_ShouldReturnInvalidPathError()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId(
            s_isUnix ? "/OldName" : @"C:\OldName"
        );
        string newName = "NewName";
        string parentPath = s_isUnix ? "/" : @"C:\";

        IDirectoryInfo parentDirectoryInfo = Substitute.For<IDirectoryInfo>();
        parentDirectoryInfo.FullName.Returns(parentPath);
        _mockFileSystem.Directory.GetParent(path.Path).Returns(parentDirectoryInfo);
        _mockFileSystem.Path.Combine(parentPath, newName).Returns((string)null!);

        // Act
        ErrorOr<FileSystemPathId> result = _sut.RenameDirectory(path, newName);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(Errors.FileSystemManagement.InvalidPath, result.FirstError);
        _mockFileSystem.Directory.DidNotReceive().Move(Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public void DeleteDirectory_WhenDirectoryExistsAndHasDeletePermission_ShouldDeletesuccessfuly()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId(
            s_isUnix ? "/DirectoryToDelete" : @"C:\DirectoryToDelete"
        );
        _mockFileSystemPermissionsService.CanAccessPath(path, FileAccessMode.Delete, false).Returns(true);
        IDirectoryInfo mockDirectoryInfo = Substitute.For<IDirectoryInfo>();
        _mockFileSystem.DirectoryInfo.New(path.Path).Returns(mockDirectoryInfo);

        // Act
        ErrorOr<Deleted> result = _sut.DeleteDirectory(path);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(Result.Deleted, result.Value);
        mockDirectoryInfo.Received(1).Delete(true);
    }

    [Fact]
    public void DeleteDirectory_WhenNoDeletePermission_ShouldReturnUnauthorizedAccessError()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId(
            s_isUnix ? "/DirectoryToDelete" : @"C:\DirectoryToDelete"
        );
        _mockFileSystemPermissionsService.CanAccessPath(path, FileAccessMode.Delete, false).Returns(false);

        // Act
        ErrorOr<Deleted> result = _sut.DeleteDirectory(path);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(Errors.Permission.UnauthorizedAccess, result.FirstError);
        _mockFileSystem.DirectoryInfo.DidNotReceive().New(Arg.Any<string>());
    }
}
