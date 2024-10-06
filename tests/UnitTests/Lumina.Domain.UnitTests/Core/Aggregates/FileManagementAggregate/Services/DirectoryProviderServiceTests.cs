#region ========================================================================= USING =====================================================================================
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using ErrorOr;
using FluentAssertions;
using Lumina.Contracts.Enums.FileSystem;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.Services;
using Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.ValueObjects;
using Lumina.Domain.UnitTests.Core.Aggregates.FileManagementAggregate.ValueObjects.Fixtures;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
#endregion

namespace Lumina.Domain.UnitTests.Core.Aggregates.FileManagementAggregate.Services;

/// <summary>
/// Contains unit tests for the <see cref="DirectoryProviderService"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class DirectoryProviderServiceTests
{
    #region ================================================================== FIELD MEMBERS ================================================================================
    private readonly IFixture _fixture;
    private readonly IFileSystem _mockFileSystem;
    private readonly IFileSystemPermissionsService _mockFileSystemPermissionsService;
    private readonly DirectoryProviderService _sut;
    private readonly FileSystemPathIdFixture _fileSystemPathIdFixture;
    #endregion

    #region ====================================================================== CTOR =====================================================================================
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
    #endregion

    #region ===================================================================== METHODS ===================================================================================
    [Fact]
    public void GetSubdirectoryPaths_WhenUserHasNoAccess_ShouldReturnUnauthorizedAccessError()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId();
        _mockFileSystemPermissionsService.CanAccessPath(path, FileAccessMode.ListDirectory, false).Returns(false);

        // Act
        ErrorOr<IEnumerable<FileSystemPathId>> result = _sut.GetSubdirectoryPaths(path, false);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.Permission.UnauthorizedAccess);
    }

    [Fact]
    public void GetSubdirectoryPaths_WhenUserHasAccess_ShouldReturnSubdirectories()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId();
        string[] subdirectories = _fixture.CreateMany<string>(3).ToArray();
        _mockFileSystemPermissionsService.CanAccessPath(path, FileAccessMode.ListDirectory, false).Returns(true);
        _mockFileSystem.Directory.GetDirectories(path.Path).Returns(subdirectories);
        _mockFileSystem.Path.DirectorySeparatorChar.Returns('\\');

        // Act
        ErrorOr<IEnumerable<FileSystemPathId>> result = _sut.GetSubdirectoryPaths(path, false);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().HaveCount(3);
        result.Value.Select(x => x.Path).Should().BeEquivalentTo(subdirectories.Select(x => x + '\\'));
    }

    [Fact]
    public void GetSubdirectoryPaths_WhenIncludeHiddenElementsIsFalse_ShouldExcludeHiddenDirectories()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId();
        string[] subdirectories = [@"C:\Visible", @"C:\Hidden"];
        _mockFileSystemPermissionsService.CanAccessPath(path, FileAccessMode.ListDirectory, false).Returns(true);
        _mockFileSystem.Directory.GetDirectories(path.Path).Returns(subdirectories);
        _mockFileSystem.Path.DirectorySeparatorChar.Returns('\\');
        _mockFileSystem.File.GetAttributes(@"C:\Visible").Returns(FileAttributes.Directory);
        _mockFileSystem.File.GetAttributes(@"C:\Hidden").Returns(FileAttributes.Directory | FileAttributes.Hidden);

        // Act
        ErrorOr<IEnumerable<FileSystemPathId>> result = _sut.GetSubdirectoryPaths(path, false);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().HaveCount(1);
        result.Value.First().Path.Should().Be(@"C:\Visible\");
    }

    [Fact]
    public void GetSubdirectoryPaths_WhenIncludeHiddenElementsIsTrue_ShouldIncludeHiddenDirectories()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId();
        string[] subdirectories = [@"C:\Visible", @"C:\Hidden"];
        _mockFileSystemPermissionsService.CanAccessPath(path, FileAccessMode.ListDirectory, false).Returns(true);
        _mockFileSystem.Directory.GetDirectories(path.Path).Returns(subdirectories);
        _mockFileSystem.Path.DirectorySeparatorChar.Returns('\\');
        _mockFileSystem.File.GetAttributes(@"C:\Visible").Returns(FileAttributes.Directory);
        _mockFileSystem.File.GetAttributes(@"C:\Hidden").Returns(FileAttributes.Directory | FileAttributes.Hidden);

        // Act
        ErrorOr<IEnumerable<FileSystemPathId>> result = _sut.GetSubdirectoryPaths(path, true);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().HaveCount(2);
        result.Value.Select(x => x.Path).Should().BeEquivalentTo([@"C:\Visible\", @"C:\Hidden\"]);
    }

    [Fact]
    public void GetSubdirectoryPaths_WhenGetAttributesThrowsException_ShouldStillAddDirectory()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId();
        string[] subdirectories = [@"C:\Valid", @"C:\Invalid"];
        _mockFileSystemPermissionsService.CanAccessPath(path, FileAccessMode.ListDirectory, false).Returns(true);
        _mockFileSystem.Directory.GetDirectories(path.Path).Returns(subdirectories);
        _mockFileSystem.Path.DirectorySeparatorChar.Returns('\\');
        _mockFileSystem.File.GetAttributes(@"C:\Valid").Returns(FileAttributes.Directory);
        _mockFileSystem.File.GetAttributes(@"C:\Invalid").Throws(new Exception("Access denied"));

        // Act
        ErrorOr<IEnumerable<FileSystemPathId>> result = _sut.GetSubdirectoryPaths(path, false);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().HaveCount(2);
        result.Value.First().Path.Should().Be(@"C:\Invalid\");
        result.Value.Last().Path.Should().Be(@"C:\Valid\");
    }

    [Fact]
    public void GetSubdirectoryPaths_ShouldOrderDirectoriesAlphabetically()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId();
        string[] subdirectories = [@"C:\C", @"C:\A", @"C:\B"];
        _mockFileSystemPermissionsService.CanAccessPath(path, FileAccessMode.ListDirectory, false).Returns(true);
        _mockFileSystem.Directory.GetDirectories(path.Path).Returns(subdirectories);
        _mockFileSystem.Path.DirectorySeparatorChar.Returns('\\');
        _mockFileSystem.File.GetAttributes(Arg.Any<string>()).Returns(FileAttributes.Directory);

        // Act
        ErrorOr<IEnumerable<FileSystemPathId>> result = _sut.GetSubdirectoryPaths(path, false);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Select(x => x.Path).Should().BeEquivalentTo([@"C:\A\", @"C:\B\", @"C:\C\"], options => options.WithStrictOrdering());
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
        result.IsError.Should().BeFalse();
        result.Value.Should().BeEmpty();
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
        result.IsError.Should().BeFalse();
        result.Value.Should().BeTrue();
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
        result.IsError.Should().BeFalse();
        result.Value.Should().BeFalse();
    }

    [Fact]
    public void GetFileName_WhenPathIsValid_ShouldReturnFileName()
    {
        // Arrange
        string fullPath = @"C:\folder\subfolder\file.txt";
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId(fullPath);
        _mockFileSystem.Path.GetFileName(fullPath).Returns("file.txt");

        // Act
        ErrorOr<string> result = _sut.GetFileName(path);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be("file.txt");
    }

    [Fact]
    public void GetFileName_WhenPathEndsWithDirectorySeparator_ShouldReturnLastSegment()
    {
        // Arrange
        string fullPath = @"C:\folder\subfolder\";
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId(fullPath);
        _mockFileSystem.Path.GetFileName(fullPath[..^1]).Returns("subfolder");
        _mockFileSystem.Path.DirectorySeparatorChar.Returns('\\');

        // Act
        ErrorOr<string> result = _sut.GetFileName(path);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be("subfolder");
    }

    [Fact]
    public void GetFileName_WhenPathIsRoot_ShouldReturnEmptyString()
    {
        // Arrange
        string fullPath = @"C:\";
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId(fullPath);
        _mockFileSystem.Path.GetFileName(fullPath[..^1]).Returns(string.Empty);
        _mockFileSystem.Path.DirectorySeparatorChar.Returns('\\');

        // Act
        ErrorOr<string> result = _sut.GetFileName(path);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeEmpty();
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
        result.IsError.Should().BeFalse();
        result.Value.HasValue.Should().BeTrue();
        result.Value.Value.Should().Be(expectedDateTime);
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
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.Permission.UnauthorizedAccess);
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
        result.IsError.Should().BeFalse();
        result.Value.HasValue.Should().BeTrue();
        result.Value.Value.Should().Be(expectedDateTime);
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
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.Permission.UnauthorizedAccess);
    }

    [Fact]
    public void CreateDirectory_WhenUserHasAccess_ShouldCreateDirectoryAndReturnPath()
    {
        // Arrange
        FileSystemPathId parentPath = _fileSystemPathIdFixture.CreateFileSystemPathId(@"C:\Parent");
        string directoryName = "NewDirectory";
        string fullPath = @"C:\Parent\NewDirectory";

        _mockFileSystemPermissionsService.CanAccessPath(parentPath, FileAccessMode.Write, false).Returns(true);
        _mockFileSystem.Path.Combine(parentPath.Path, directoryName).Returns(fullPath);

        IDirectoryInfo mockDirectoryInfo = Substitute.For<IDirectoryInfo>();
        mockDirectoryInfo.FullName.Returns(fullPath);
        _mockFileSystem.Directory.CreateDirectory(fullPath).Returns(mockDirectoryInfo);

        // Act
        ErrorOr<FileSystemPathId> result = _sut.CreateDirectory(parentPath, directoryName);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Path.Should().Be(fullPath);
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
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.Permission.UnauthorizedAccess);
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
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Failure);
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
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public void CopyDirectory_WhenSourceDirectoryExistsAndUserHasAccess_ShouldCopyDirectoryAndReturnPath()
    {
        // Arrange
        FileSystemPathId sourcePath = _fileSystemPathIdFixture.CreateFileSystemPathId(@"C:\Source");
        FileSystemPathId destinationPath = _fileSystemPathIdFixture.CreateFileSystemPathId(@"C:\Destination");
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
        result.IsError.Should().BeFalse();
        result.Value.Path.Should().Be(destinationPath.Path);
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
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.FileManagement.DirectoryNotFound);
    }

    [Fact]
    public void CopyDirectory_WhenDestinationDirectoryExists_ShouldCreateUniqueDirectoryName()
    {
        // Arrange
        FileSystemPathId sourcePath = _fileSystemPathIdFixture.CreateFileSystemPathId(@"C:\Source");
        FileSystemPathId destinationPath = _fileSystemPathIdFixture.CreateFileSystemPathId(@"C:\Destination");
        string expectedNewDirectoryPath = @"C:\Destination - Copy (1)";
        bool overrideExisting = false;

        _mockFileSystem.Directory.Exists(sourcePath.Path).Returns(true);
        _mockFileSystem.Directory.Exists(destinationPath.Path).Returns(true);
        _mockFileSystem.Directory.Exists(@"C:\Destination").Returns(true);
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
        result.IsError.Should().BeFalse();
        result.Value.Path.Should().Be(expectedNewDirectoryPath);
        _mockFileSystem.Directory.Received(1).CreateDirectory(expectedNewDirectoryPath);
    }

    [Fact]
    public void CopyDirectory_WhenCopyingFilesAndSubdirectories_ShouldCopyAllContents()
    {
        // Arrange
        FileSystemPathId sourcePath = _fileSystemPathIdFixture.CreateFileSystemPathId(@"C:\Source");
        FileSystemPathId destinationPath = _fileSystemPathIdFixture.CreateFileSystemPathId(@"C:\Destination");
        bool overrideExisting = true;

        // ensure all relevant directories exist
        _mockFileSystem.Directory.Exists(@"C:\Source").Returns(true);
        _mockFileSystem.Directory.Exists(@"C:\Source\SubDir").Returns(true);
        _mockFileSystem.Directory.Exists(@"C:\Destination").Returns(false);
        _mockFileSystem.Directory.CreateDirectory(Arg.Any<string>()).Returns(callInfo => {
            IDirectoryInfo mockDirInfo = Substitute.For<IDirectoryInfo>();
            mockDirInfo.FullName.Returns(callInfo.Arg<string>());
            return mockDirInfo;
        });

        IFileInfo mockFile = Substitute.For<IFileInfo>();
        mockFile.Name.Returns("file.txt");
        mockFile.Attributes.Returns(FileAttributes.Normal);

        IDirectoryInfo mockSubDir = Substitute.For<IDirectoryInfo>();
        mockSubDir.Name.Returns("SubDir");
        mockSubDir.FullName.Returns(@"C:\Source\SubDir");

        IDirectoryInfo sourceDirectoryInfo = Substitute.For<IDirectoryInfo>();
        sourceDirectoryInfo.GetFiles().Returns([mockFile]);
        sourceDirectoryInfo.GetDirectories().Returns([mockSubDir]);
        _mockFileSystem.DirectoryInfo.New(@"C:\Source").Returns(sourceDirectoryInfo);

        // mock subdirectory info
        IDirectoryInfo subDirInfo = Substitute.For<IDirectoryInfo>();
        subDirInfo.GetFiles().Returns([]);
        subDirInfo.GetDirectories().Returns([]);
        _mockFileSystem.DirectoryInfo.New(@"C:\Source\SubDir").Returns(subDirInfo);

        // mock the Path.Combine method for all necessary cases
        _mockFileSystem.Path.Combine(Arg.Any<string>(), Arg.Any<string>())
            .Returns(callInfo => Path.Combine(callInfo.ArgAt<string>(0), callInfo.ArgAt<string>(1)));

        // Act
        ErrorOr<FileSystemPathId> result = _sut.CopyDirectory(sourcePath, destinationPath, overrideExisting);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Path.Should().Be(@"C:\Destination");
        _mockFileSystem.Directory.Received(1).CreateDirectory(@"C:\Destination");
        _mockFileSystem.Directory.Received(1).CreateDirectory(@"C:\Destination\SubDir");
        mockFile.Received(1).CopyTo(@"C:\Destination\file.txt", overrideExisting);
        _mockFileSystem.File.Received(1).SetAttributes(@"C:\Destination\file.txt", FileAttributes.Normal);
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
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.FileManagement.DirectoryCopyError);
    }

    [Fact]
    public void MoveDirectory_WhenSourceDirectoryDoesNotExist_ShouldReturnDirectoryNotFoundError()
    {
        // Arrange
        FileSystemPathId sourcePath = _fileSystemPathIdFixture.CreateFileSystemPathId(@"C:\NonExistentSource");
        FileSystemPathId destinationPath = _fileSystemPathIdFixture.CreateFileSystemPathId(@"C:\Destination");
        bool overrideExisting = false;

        _mockFileSystem.Directory.Exists(sourcePath.Path).Returns(false);

        // Act
        ErrorOr<FileSystemPathId> result = _sut.MoveDirectory(sourcePath, destinationPath, overrideExisting);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.FileManagement.DirectoryNotFound);
    }

    [Fact]
    public void MoveDirectory_WhenDestinationDirectoryDoesNotExist_ShouldPerformSimpleMove()
    {
        // Arrange
        FileSystemPathId sourcePath = _fileSystemPathIdFixture.CreateFileSystemPathId(@"C:\Source");
        FileSystemPathId destinationPath = _fileSystemPathIdFixture.CreateFileSystemPathId(@"C:\Destination");
        bool overrideExisting = false;

        _mockFileSystem.Directory.Exists(sourcePath.Path).Returns(true);
        _mockFileSystem.Directory.Exists(destinationPath.Path).Returns(false);

        // Act
        ErrorOr<FileSystemPathId> result = _sut.MoveDirectory(sourcePath, destinationPath, overrideExisting);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Path.Should().Be(destinationPath.Path);
        _mockFileSystem.Directory.Received(1).Move(sourcePath.Path, destinationPath.Path);
    }

    [Fact]
    public void MoveDirectory_WhenDestinationDirectoryExists_ShouldMergeDirectories()
    {
        // Arrange
        FileSystemPathId sourcePath = _fileSystemPathIdFixture.CreateFileSystemPathId(@"C:\Source");
        FileSystemPathId destinationPath = _fileSystemPathIdFixture.CreateFileSystemPathId(@"C:\Destination");
        bool overrideExisting = true;

        _mockFileSystem.Directory.Exists(sourcePath.Path).Returns(true);
        _mockFileSystem.Directory.Exists(destinationPath.Path).Returns(true);

        _mockFileSystem.Directory.GetFiles(sourcePath.Path).Returns([@"C:\Source\file1.txt"]);
        _mockFileSystem.Directory.GetDirectories(sourcePath.Path).Returns([@"C:\Source\SubDir"]);

        _mockFileSystem.Directory.GetFiles(@"C:\Source\SubDir").Returns([]);
        _mockFileSystem.Directory.GetDirectories(@"C:\Source\SubDir").Returns([]);

        _mockFileSystem.Path.GetFileName(@"C:\Source\file1.txt").Returns("file1.txt");
        _mockFileSystem.Path.GetFileName(@"C:\Source\SubDir").Returns("SubDir");

        _mockFileSystem.Path.Combine(Arg.Any<string>(), Arg.Any<string>())
            .Returns(callInfo => Path.Combine(callInfo.ArgAt<string>(0), callInfo.ArgAt<string>(1)));

        _mockFileSystem.Directory.EnumerateFileSystemEntries(sourcePath.Path).Returns([]);

        // Act
        ErrorOr<FileSystemPathId> result = _sut.MoveDirectory(sourcePath, destinationPath, overrideExisting);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Path.Should().Be(destinationPath.Path);
        _mockFileSystem.File.Received(1).Move(@"C:\Source\file1.txt", @"C:\Destination\file1.txt");
        _mockFileSystem.Directory.Received(1).Move(@"C:\Source\SubDir", @"C:\Destination\SubDir");
        _mockFileSystem.Directory.Received(1).Delete(sourcePath.Path);
    }

    [Fact]
    public void MoveDirectory_WhenExceptionOccurs_ShouldReturnDirectoryMoveError()
    {
        // Arrange
        FileSystemPathId sourcePath = _fileSystemPathIdFixture.CreateFileSystemPathId(@"C:\Source");
        FileSystemPathId destinationPath = _fileSystemPathIdFixture.CreateFileSystemPathId(@"C:\Destination");
        bool overrideExisting = false;

        _mockFileSystem.Directory.Exists(sourcePath.Path).Returns(true);
        _mockFileSystem.Directory.Exists(destinationPath.Path).Returns(false);
        _mockFileSystem.Directory.When(x => x.Move(Arg.Any<string>(), Arg.Any<string>()))
            .Do(x => throw new IOException("Simulated IO error"));

        // Act
        ErrorOr<FileSystemPathId> result = _sut.MoveDirectory(sourcePath, destinationPath, overrideExisting);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.FileManagement.DirectoryMoveError);
    }

    [Fact]
    public void MergeDirectories_WhenSourceContainsFilesAndSubdirectories_ShouldMergeCorrectly()
    {
        // Arrange
        FileSystemPathId sourcePath = _fileSystemPathIdFixture.CreateFileSystemPathId(@"C:\Source");
        FileSystemPathId destPath = _fileSystemPathIdFixture.CreateFileSystemPathId(@"C:\Destination");
        bool overwrite = true;

        _mockFileSystem.Directory.Exists(sourcePath.Path).Returns(true);
        _mockFileSystem.Directory.Exists(destPath.Path).Returns(true);

        _mockFileSystem.Directory.GetFiles(sourcePath.Path).Returns([@"C:\Source\file1.txt", @"C:\Source\file2.txt"]);
        _mockFileSystem.Directory.GetDirectories(sourcePath.Path).Returns([@"C:\Source\SubDir1", @"C:\Source\SubDir2"]);

        _mockFileSystem.Path.GetFileName(@"C:\Source\file1.txt").Returns("file1.txt");
        _mockFileSystem.Path.GetFileName(@"C:\Source\file2.txt").Returns("file2.txt");
        _mockFileSystem.Path.GetFileName(@"C:\Source\SubDir1").Returns("SubDir1");
        _mockFileSystem.Path.GetFileName(@"C:\Source\SubDir2").Returns("SubDir2");

        _mockFileSystem.File.Exists(@"C:\Destination\file1.txt").Returns(true);
        _mockFileSystem.File.Exists(@"C:\Destination\file2.txt").Returns(false);

        _mockFileSystem.Directory.Exists(@"C:\Destination\SubDir1").Returns(true);
        _mockFileSystem.Directory.Exists(@"C:\Destination\SubDir2").Returns(false);

        _mockFileSystem.Path.Combine(Arg.Any<string>(), Arg.Any<string>())
            .Returns(callInfo => Path.Combine(callInfo.ArgAt<string>(0), callInfo.ArgAt<string>(1)));

        _mockFileSystem.Directory.EnumerateFileSystemEntries(sourcePath.Path).Returns([]);

        // Act
        ErrorOr<FileSystemPathId> result = _sut.MoveDirectory(sourcePath, destPath, overwrite);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(destPath);
        _mockFileSystem.File.Received(1).Delete(@"C:\Destination\file1.txt");
        _mockFileSystem.File.Received(1).Move(@"C:\Source\file1.txt", @"C:\Destination\file1.txt");
        _mockFileSystem.File.Received(1).Move(@"C:\Source\file2.txt", @"C:\Destination\file2.txt");
        _mockFileSystem.Directory.Received(1).Move(@"C:\Source\SubDir2", @"C:\Destination\SubDir2");
        _mockFileSystem.Directory.Received(1).Delete(sourcePath.Path);
    }

    [Fact]
    public void MoveDirectory_WhenOverwriteIsFalse_ShouldMoveOnlyNonExistingFiles()
    {
        // Arrange
        FileSystemPathId sourcePath = _fileSystemPathIdFixture.CreateFileSystemPathId(@"C:\Source");
        FileSystemPathId destPath = _fileSystemPathIdFixture.CreateFileSystemPathId(@"C:\Destination");
        bool overwrite = false;

        _mockFileSystem.Directory.Exists(sourcePath.Path).Returns(true);
        _mockFileSystem.Directory.Exists(destPath.Path).Returns(true);

        _mockFileSystem.Directory.GetFiles(sourcePath.Path).Returns([@"C:\Source\file1.txt", @"C:\Source\file2.txt"]);
        _mockFileSystem.Directory.GetDirectories(sourcePath.Path).Returns([]);

        _mockFileSystem.Path.GetFileName(@"C:\Source\file1.txt").Returns("file1.txt");
        _mockFileSystem.Path.GetFileName(@"C:\Source\file2.txt").Returns("file2.txt");

        _mockFileSystem.File.Exists(@"C:\Destination\file1.txt").Returns(true);
        _mockFileSystem.File.Exists(@"C:\Destination\file2.txt").Returns(false);

        _mockFileSystem.Path.Combine(Arg.Any<string>(), Arg.Any<string>())
            .Returns(callInfo => Path.Combine(callInfo.ArgAt<string>(0), callInfo.ArgAt<string>(1)));

        _mockFileSystem.Directory.EnumerateFileSystemEntries(sourcePath.Path).Returns([@"C:\Source\file2.txt"]);

        // Act
        ErrorOr<FileSystemPathId> result = _sut.MoveDirectory(sourcePath, destPath, overwrite);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(destPath);
        _mockFileSystem.File.DidNotReceive().Delete(Arg.Any<string>());
        _mockFileSystem.File.DidNotReceive().Move(@"C:\Source\file1.txt", @"C:\Destination\file1.txt");
        _mockFileSystem.File.Received(1).Move(@"C:\Source\file2.txt", @"C:\Destination\file2.txt");
        _mockFileSystem.Directory.DidNotReceive().Delete(sourcePath.Path);
    }

    [Fact]
    public void MergeDirectories_WhenSourceIsEmpty_ShouldDeleteSourceDirectory()
    {
        // Arrange
        string sourceDir = @"C:\Source";
        string destDir = @"C:\Destination";
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
        FileSystemPathId sourcePath = _fileSystemPathIdFixture.CreateFileSystemPathId(@"C:\Source");
        FileSystemPathId destPath = _fileSystemPathIdFixture.CreateFileSystemPathId(@"C:\Destination");
        bool overwrite = true;

        _mockFileSystem.Directory.Exists(sourcePath.Path).Returns(true);
        _mockFileSystem.Directory.Exists(destPath.Path).Returns(true);

        _mockFileSystem.Directory.Exists(@"C:\Source\SubDir").Returns(true);
        _mockFileSystem.Directory.Exists(@"C:\Destination\SubDir").Returns(true);

        _mockFileSystem.Directory.GetFiles(sourcePath.Path).Returns([]);
        _mockFileSystem.Directory.GetDirectories(sourcePath.Path).Returns([@"C:\Source\SubDir"]);

        _mockFileSystem.Directory.GetFiles(@"C:\Source\SubDir").Returns([@"C:\Source\SubDir\file.txt"]);
        _mockFileSystem.Directory.GetDirectories(@"C:\Source\SubDir").Returns([]);

        _mockFileSystem.Path.GetFileName(@"C:\Source\SubDir").Returns("SubDir");
        _mockFileSystem.Path.GetFileName(@"C:\Source\SubDir\file.txt").Returns("file.txt");

        _mockFileSystem.File.Exists(@"C:\Destination\SubDir\file.txt").Returns(false);

        _mockFileSystem.Path.Combine(Arg.Any<string>(), Arg.Any<string>())
            .Returns(callInfo => Path.Combine(callInfo.ArgAt<string>(0), callInfo.ArgAt<string>(1)));

        _mockFileSystem.Directory.EnumerateFileSystemEntries(sourcePath.Path).Returns([@"C:\Source\SubDir"]);
        _mockFileSystem.Directory.EnumerateFileSystemEntries(@"C:\Source\SubDir").Returns([]);

        // Act
        ErrorOr<FileSystemPathId> result = _sut.MoveDirectory(sourcePath, destPath, overwrite);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(destPath);
        _mockFileSystem.File.Received(1).Move(@"C:\Source\SubDir\file.txt", @"C:\Destination\SubDir\file.txt");
        _mockFileSystem.Directory.Received(1).Delete(@"C:\Source\SubDir");
        _mockFileSystem.Directory.DidNotReceive().Delete(sourcePath.Path);
    }

    [Fact]
    public void RenameDirectory_WhenParentDirectoryIsWritable_ShouldRenameSuccessfully()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId(@"C:\OldName");
        string newName = "NewName";
        string parentPath = @"C:\";
        string newPath = @"C:\NewName";

        IDirectoryInfo parentDirectoryInfo = Substitute.For<IDirectoryInfo>();
        parentDirectoryInfo.FullName.Returns(parentPath);
        _mockFileSystem.Directory.GetParent(path.Path).Returns(parentDirectoryInfo);
        _mockFileSystem.Path.Combine(parentPath, newName).Returns(newPath);
        _mockFileSystemPermissionsService.CanAccessPath(Arg.Any<FileSystemPathId>(), FileAccessMode.Write, false).Returns(true);
        _mockFileSystemPermissionsService.CanAccessPath(path, FileAccessMode.Execute, false).Returns(true);

        // Act
        ErrorOr<FileSystemPathId> result = _sut.RenameDirectory(path, newName);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Path.Should().Be(newPath);
        _mockFileSystem.Directory.Received(1).Move(path.Path, newPath);
    }

    [Fact]
    public void RenameDirectory_WhenParentDirectoryIsNotWritable_ShouldReturnUnauthorizedAccessError()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId(@"C:\OldName");
        string newName = "NewName";
        string parentPath = @"C:\";
        string newPath = @"C:\NewName";

        IDirectoryInfo parentDirectoryInfo = Substitute.For<IDirectoryInfo>();
        parentDirectoryInfo.FullName.Returns(parentPath);
        _mockFileSystem.Directory.GetParent(path.Path).Returns(parentDirectoryInfo);
        _mockFileSystem.Path.Combine(parentPath, newName).Returns(newPath);
        _mockFileSystemPermissionsService.CanAccessPath(Arg.Any<FileSystemPathId>(), FileAccessMode.Write, false).Returns(false);

        // Act
        ErrorOr<FileSystemPathId> result = _sut.RenameDirectory(path, newName);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.Permission.UnauthorizedAccess);
        _mockFileSystem.Directory.DidNotReceive().Move(Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public void RenameDirectory_WhenDirectoryIsNotExecutable_ShouldReturnUnauthorizedAccessError()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId(@"C:\OldName");
        string newName = "NewName";
        string parentPath = @"C:\";
        string newPath = @"C:\NewName";

        IDirectoryInfo parentDirectoryInfo = Substitute.For<IDirectoryInfo>();
        parentDirectoryInfo.FullName.Returns(parentPath);
        _mockFileSystem.Directory.GetParent(path.Path).Returns(parentDirectoryInfo);
        _mockFileSystem.Path.Combine(parentPath, newName).Returns(newPath);
        _mockFileSystemPermissionsService.CanAccessPath(Arg.Any<FileSystemPathId>(), FileAccessMode.Write, false).Returns(true);
        _mockFileSystemPermissionsService.CanAccessPath(path, FileAccessMode.Execute, false).Returns(false);

        // Act
        ErrorOr<FileSystemPathId> result = _sut.RenameDirectory(path, newName);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.Permission.UnauthorizedAccess);
        _mockFileSystem.Directory.DidNotReceive().Move(Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public void RenameDirectory_WhenParentDirectoryIsNull_ShouldReturnInvalidPathError()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId(@"C:\OldName");
        string newName = "NewName";

        _mockFileSystem.Directory.GetParent(path.Path).Returns((IDirectoryInfo)null!);

        // Act
        ErrorOr<FileSystemPathId> result = _sut.RenameDirectory(path, newName);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.FileManagement.InvalidPath);
        _mockFileSystem.Directory.DidNotReceive().Move(Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public void RenameDirectory_WhenNewPathIsInvalid_ShouldReturnInvalidPathError()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId(@"C:\OldName");
        string newName = "NewName";
        string parentPath = @"C:\";

        IDirectoryInfo parentDirectoryInfo = Substitute.For<IDirectoryInfo>();
        parentDirectoryInfo.FullName.Returns(parentPath);
        _mockFileSystem.Directory.GetParent(path.Path).Returns(parentDirectoryInfo);
        _mockFileSystem.Path.Combine(parentPath, newName).Returns((string)null!);

        // Act
        ErrorOr<FileSystemPathId> result = _sut.RenameDirectory(path, newName);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.FileManagement.InvalidPath);
        _mockFileSystem.Directory.DidNotReceive().Move(Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public void DeleteDirectory_WhenDirectoryExistsAndHasDeletePermission_ShouldDeleteSuccessfully()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId(@"C:\DirectoryToDelete");
        _mockFileSystemPermissionsService.CanAccessPath(path, FileAccessMode.Delete, false).Returns(true);
        IDirectoryInfo mockDirectoryInfo = Substitute.For<IDirectoryInfo>();
        _mockFileSystem.DirectoryInfo.New(path.Path).Returns(mockDirectoryInfo);

        // Act
        ErrorOr<Deleted> result = _sut.DeleteDirectory(path);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(Result.Deleted);
        mockDirectoryInfo.Received(1).Delete(true);
    }

    [Fact]
    public void DeleteDirectory_WhenNoDeletePermission_ShouldReturnUnauthorizedAccessError()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId(@"C:\DirectoryToDelete");
        _mockFileSystemPermissionsService.CanAccessPath(path, FileAccessMode.Delete, false).Returns(false);

        // Act
        ErrorOr<Deleted> result = _sut.DeleteDirectory(path);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.Permission.UnauthorizedAccess);
        _mockFileSystem.DirectoryInfo.DidNotReceive().New(Arg.Any<string>());
    }
    #endregion
}
