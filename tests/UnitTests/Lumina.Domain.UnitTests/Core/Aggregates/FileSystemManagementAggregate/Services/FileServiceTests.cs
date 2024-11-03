#region ========================================================================= USING =====================================================================================
using ErrorOr;
using FluentAssertions;
using Lumina.Domain.Common.Enums.FileSystem;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Entities;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Services;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Strategies.Environment;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Strategies.Platform;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.ValueObjects;
using Lumina.Domain.UnitTests.Core.Aggregates.FileSystemManagementAggregate.Entities.Fixtures;
using Lumina.Domain.UnitTests.Core.Aggregates.FileSystemManagementAggregate.ValueObjects.Fixtures;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;

#endregion

namespace Lumina.Domain.UnitTests.Core.Aggregates.FileSystemManagementAggregate.Services;

/// <summary>
/// Contains unit tests for the <see cref="FileService"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class FileServiceTests
{
    private readonly IEnvironmentContext _mockEnvironmentContext;
    private readonly IPlatformContextManager _mockPlatformContextManager;
    private readonly IPlatformContext _mockPlatformContext;
    private readonly FileService _sut;
    private readonly FileSystemPathIdFixture _fileSystemPathIdFixture;
    private readonly FileFixture _fileFixture;
    private static readonly bool s_isLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
    private readonly string _pathTestDir = s_isLinux ? "/TestDir" : @"C:\TestDir";
    private readonly string _pathTestDirFile1 = s_isLinux ? "/TestDir/File1.txt" : @"C:\TestDir\File1.txt";
    private readonly string _pathTestDirFile2 = s_isLinux ? "/TestDir/File2.txt" : @"C:\TestDir\File2.txt";
    private readonly string _pathTestDirInaccessible = s_isLinux ? "/TestDir/InaccessibleFile.txt" : @"C:\TestDir\InaccessibleFile.txt";
    private readonly string _pathDestination1 = s_isLinux ? "/Destination/" : @"C:\Destination\";
    private readonly string _pathDestination2 = s_isLinux ? "/Destination" : @"C:\Destination";
    private readonly string _pathSourceFile = s_isLinux ? "/Source/file.txt" : @"C:\Source\file.txt";
    private readonly string _pathDestinationFile = s_isLinux ? "/Destination/file.txt" : @"C:\Destination\file.txt";
    private readonly string _pathSourceOldFile = s_isLinux ? "/Source/oldfile.txt" : @"C:\Source\oldfile.txt";
    private readonly string _pathSourceNewFile = s_isLinux ? "/Source/newfile.txt" : @"C:\Source\newfile.txt";
    private readonly string _pathSourceExistingFile = s_isLinux ? "/Source/existingfile.txt" : @"C:\Source\existingfile.txt";
    private readonly string _pathSourceNonExistingFile = s_isLinux ? "/Source/nonexistent.txt" : @"C:\Source\nonexistent.txt";
    private readonly char _dirSeparator = s_isLinux ? '/' : '\\';

    /// <summary>
    /// Initializes a new instance of the <see cref="FileServiceTests"/> class.
    /// </summary>
    public FileServiceTests()
    {
        _mockEnvironmentContext = Substitute.For<IEnvironmentContext>();
        _mockPlatformContextManager = Substitute.For<IPlatformContextManager>();
        _mockPlatformContext = Substitute.For<IPlatformContext>();
        _mockPlatformContextManager.GetCurrentContext().Returns(_mockPlatformContext);
        _sut = new FileService(_mockEnvironmentContext, _mockPlatformContextManager);
        _fileSystemPathIdFixture = new FileSystemPathIdFixture();
        _fileFixture = new FileFixture();
    }

    [Fact]
    public void GetFiles_WithValidPath_ShouldReturnListOfFiles()
    {
        // Arrange
        string path = _pathTestDir;
        bool includeHiddenElements = false;
        FileSystemPathId pathId = _fileSystemPathIdFixture.CreateFileSystemPathId(path);
        FileSystemPathId[] filePaths =
        [
            _fileSystemPathIdFixture.CreateFileSystemPathId(_pathTestDirFile1),
            _fileSystemPathIdFixture.CreateFileSystemPathId(_pathTestDirFile2)
        ];

        _mockEnvironmentContext.FileProviderService.GetFilePaths(pathId, includeHiddenElements)
            .Returns(ErrorOrFactory.From(filePaths.AsEnumerable()));

        foreach (FileSystemPathId filePath in filePaths)
        {
            _mockEnvironmentContext.FileProviderService.GetFileName(filePath)
                .Returns(ErrorOrFactory.From(System.IO.Path.GetFileName(filePath.Path)));
            _mockEnvironmentContext.FileProviderService.GetLastWriteTime(filePath)
                .Returns(ErrorOrFactory.From(Optional<DateTime>.FromNullable(DateTime.Now)));
            _mockEnvironmentContext.FileProviderService.GetCreationTime(filePath)
                .Returns(ErrorOrFactory.From(Optional<DateTime>.FromNullable(DateTime.Now.AddDays(-1))));
            _mockEnvironmentContext.FileProviderService.GetSize(filePath)
                .Returns(ErrorOrFactory.From((long?)1024));
        }

        // Act
        ErrorOr<IEnumerable<File>> result = _sut.GetFiles(path, includeHiddenElements);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().HaveCount(2);
        result.Value.Select(f => f.Name).Should().BeEquivalentTo(["File1.txt", "File2.txt"]);
    }

    [Fact]
    public void GetFiles_WithInvalidPath_ShouldReturnError()
    {
        // Arrange
        string invalidPath = string.Empty;
        bool includeHiddenElements = false;

        // Act
        ErrorOr<IEnumerable<File>> result = _sut.GetFiles(invalidPath, includeHiddenElements);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.FileSystemManagement.InvalidPath);
    }

    [Fact]
    public void GetFiles_WhenFileProviderServiceReturnsError_ShouldPropagateError()
    {
        // Arrange
        string path = _pathTestDir;
        bool includeHiddenElements = false;
        FileSystemPathId pathId = _fileSystemPathIdFixture.CreateFileSystemPathId(path);

        _mockEnvironmentContext.FileProviderService.GetFilePaths(pathId, includeHiddenElements)
            .Returns(Errors.Permission.UnauthorizedAccess);

        // Act
        ErrorOr<IEnumerable<File>> result = _sut.GetFiles(path, includeHiddenElements);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.Permission.UnauthorizedAccess);
    }

    [Fact]
    public void GetFiles_WhenFileDetailsAreInaccessible_ShouldReturnInaccessibleFile()
    {
        // Arrange
        string path = _pathTestDir;
        bool includeHiddenElements = false;
        FileSystemPathId pathId = _fileSystemPathIdFixture.CreateFileSystemPathId(path);
        FileSystemPathId filePath = _fileSystemPathIdFixture.CreateFileSystemPathId(_pathTestDirInaccessible);

        _mockEnvironmentContext.FileProviderService.GetFilePaths(pathId, includeHiddenElements)
            .Returns(ErrorOrFactory.From(new[] { filePath }.AsEnumerable()));

        _mockEnvironmentContext.FileProviderService.GetFileName(filePath)
            .Returns(Errors.Permission.UnauthorizedAccess);
        _mockEnvironmentContext.FileProviderService.GetLastWriteTime(filePath)
            .Returns(Errors.Permission.UnauthorizedAccess);
        _mockEnvironmentContext.FileProviderService.GetCreationTime(filePath)
            .Returns(Errors.Permission.UnauthorizedAccess);
        _mockEnvironmentContext.FileProviderService.GetSize(filePath)
            .Returns(ErrorOrFactory.From((long?)null));

        // Act
        ErrorOr<IEnumerable<File>> result = _sut.GetFiles(path, includeHiddenElements);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().HaveCount(1);
        result.Value.First().Status.Should().Be(FileSystemItemStatus.Inaccessible);
    }

    [Fact]
    public void GetFilesOverload_WithValidFile_ShouldReturnListOfFiles()
    {
        // Arrange
        File parentFile = _fileFixture.CreateFile();
        bool includeHiddenElements = false;
        FileSystemPathId[] filePaths =
        [
            _fileSystemPathIdFixture.CreateFileSystemPathId(_pathTestDirFile1),
            _fileSystemPathIdFixture.CreateFileSystemPathId(_pathTestDirFile2)
        ];

        _mockEnvironmentContext.FileProviderService.GetFilePaths(parentFile.Id, includeHiddenElements)
            .Returns(ErrorOrFactory.From(filePaths.AsEnumerable()));

        foreach (FileSystemPathId filePath in filePaths)
        {
            _mockEnvironmentContext.FileProviderService.GetFileName(filePath)
                .Returns(ErrorOrFactory.From(System.IO.Path.GetFileName(filePath.Path)));
            _mockEnvironmentContext.FileProviderService.GetLastWriteTime(filePath)
                .Returns(ErrorOrFactory.From(Optional<DateTime>.FromNullable(DateTime.Now)));
            _mockEnvironmentContext.FileProviderService.GetCreationTime(filePath)
                .Returns(ErrorOrFactory.From(Optional<DateTime>.FromNullable(DateTime.Now.AddDays(-1))));
            _mockEnvironmentContext.FileProviderService.GetSize(filePath)
                .Returns(ErrorOrFactory.From((long?)1024));
        }

        // Act
        ErrorOr<IEnumerable<File>> result = _sut.GetFiles(parentFile, includeHiddenElements);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().HaveCount(2);
        result.Value.Select(f => f.Name).Should().BeEquivalentTo(["File1.txt", "File2.txt"]);
    }

    [Fact]
    public void GetFilesOverload_WhenFileProviderServiceReturnsError_ShouldPropagateError()
    {
        // Arrange
        File parentFile = _fileFixture.CreateFile();
        bool includeHiddenElements = false;

        _mockEnvironmentContext.FileProviderService.GetFilePaths(parentFile.Id, includeHiddenElements)
            .Returns(Errors.Permission.UnauthorizedAccess);

        // Act
        ErrorOr<IEnumerable<File>> result = _sut.GetFiles(parentFile, includeHiddenElements);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.Permission.UnauthorizedAccess);
    }

    [Fact]
    public void GetFilesOverload_WhenFileDetailsAreInaccessible_ShouldReturnInaccessibleFile()
    {
        // Arrange
        File parentFile = _fileFixture.CreateFile();
        bool includeHiddenElements = false;
        FileSystemPathId filePath = _fileSystemPathIdFixture.CreateFileSystemPathId(_pathTestDirInaccessible);

        _mockEnvironmentContext.FileProviderService.GetFilePaths(parentFile.Id, includeHiddenElements)
            .Returns(ErrorOrFactory.From(new[] { filePath }.AsEnumerable()));

        _mockEnvironmentContext.FileProviderService.GetFileName(filePath)
            .Returns(Errors.Permission.UnauthorizedAccess);
        _mockEnvironmentContext.FileProviderService.GetLastWriteTime(filePath)
            .Returns(Errors.Permission.UnauthorizedAccess);
        _mockEnvironmentContext.FileProviderService.GetCreationTime(filePath)
            .Returns(Errors.Permission.UnauthorizedAccess);
        _mockEnvironmentContext.FileProviderService.GetSize(filePath)
            .Returns(ErrorOrFactory.From((long?)null));

        // Act
        ErrorOr<IEnumerable<File>> result = _sut.GetFiles(parentFile, includeHiddenElements);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().HaveCount(1);
        result.Value.First().Status.Should().Be(FileSystemItemStatus.Inaccessible);
    }

    [Fact]
    public void GetFiles_WithValidFileSystemPathId_ShouldReturnListOfFiles()
    {
        // Arrange
        FileSystemPathId pathId = _fileSystemPathIdFixture.CreateFileSystemPathId(_pathTestDir);
        bool includeHiddenElements = false;
        FileSystemPathId[] filePaths =
        [
            _fileSystemPathIdFixture.CreateFileSystemPathId(_pathTestDirFile1),
            _fileSystemPathIdFixture.CreateFileSystemPathId(_pathTestDirFile2)
        ];

        _mockEnvironmentContext.FileProviderService.GetFilePaths(pathId, includeHiddenElements)
            .Returns(ErrorOrFactory.From(filePaths.AsEnumerable()));

        foreach (FileSystemPathId filePath in filePaths)
        {
            _mockEnvironmentContext.FileProviderService.GetFileName(filePath)
                .Returns(ErrorOrFactory.From(System.IO.Path.GetFileName(filePath.Path)));
            _mockEnvironmentContext.FileProviderService.GetLastWriteTime(filePath)
                .Returns(ErrorOrFactory.From(Optional<DateTime>.FromNullable(DateTime.Now)));
            _mockEnvironmentContext.FileProviderService.GetCreationTime(filePath)
                .Returns(ErrorOrFactory.From(Optional<DateTime>.FromNullable(DateTime.Now.AddDays(-1))));
            _mockEnvironmentContext.FileProviderService.GetSize(filePath)
                .Returns(ErrorOrFactory.From((long?)1024));
        }

        // Act
        ErrorOr<IEnumerable<File>> result = _sut.GetFiles(pathId, includeHiddenElements);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().HaveCount(2);
        result.Value.Select(f => f.Name).Should().BeEquivalentTo(["File1.txt", "File2.txt"]);
    }

    [Fact]
    public void GetFilesWithFileSystemPathId_WhenFileProviderServiceReturnsError_ShouldPropagateError()
    {
        // Arrange
        FileSystemPathId pathId = _fileSystemPathIdFixture.CreateFileSystemPathId(_pathTestDir);
        bool includeHiddenElements = false;

        _mockEnvironmentContext.FileProviderService.GetFilePaths(pathId, includeHiddenElements)
            .Returns(Errors.Permission.UnauthorizedAccess);

        // Act
        ErrorOr<IEnumerable<File>> result = _sut.GetFiles(pathId, includeHiddenElements);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.Permission.UnauthorizedAccess);
    }

    [Fact]
    public void GetFilesWithFileSystemPathId_WhenFileDetailsAreInaccessible_ShouldReturnInaccessibleFile()
    {
        // Arrange
        FileSystemPathId pathId = _fileSystemPathIdFixture.CreateFileSystemPathId(_pathTestDir);
        bool includeHiddenElements = false;
        FileSystemPathId filePath = _fileSystemPathIdFixture.CreateFileSystemPathId(_pathTestDirInaccessible);

        _mockEnvironmentContext.FileProviderService.GetFilePaths(pathId, includeHiddenElements)
            .Returns(ErrorOrFactory.From(new[] { filePath }.AsEnumerable()));

        _mockEnvironmentContext.FileProviderService.GetFileName(filePath)
            .Returns(Errors.Permission.UnauthorizedAccess);
        _mockEnvironmentContext.FileProviderService.GetLastWriteTime(filePath)
            .Returns(Errors.Permission.UnauthorizedAccess);
        _mockEnvironmentContext.FileProviderService.GetCreationTime(filePath)
            .Returns(Errors.Permission.UnauthorizedAccess);
        _mockEnvironmentContext.FileProviderService.GetSize(filePath)
            .Returns(ErrorOrFactory.From((long?)null));

        // Act
        ErrorOr<IEnumerable<File>> result = _sut.GetFiles(pathId, includeHiddenElements);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().HaveCount(1);
        result.Value.First().Status.Should().Be(FileSystemItemStatus.Inaccessible);
    }

    [Fact]
    public void CopyFile_WithValidPaths_ShouldReturnCopiedFile()
    {
        // Arrange
        string sourcePath = _pathSourceFile;
        string destinationPath = _pathDestination1;
        bool overrideExisting = true;
        FileSystemPathId sourcePathId = _fileSystemPathIdFixture.CreateFileSystemPathId(sourcePath);
        FileSystemPathId destinationPathId = _fileSystemPathIdFixture.CreateFileSystemPathId(destinationPath);
        FileSystemPathId copiedFilePathId = _fileSystemPathIdFixture.CreateFileSystemPathId(_pathDestinationFile);

        _mockPlatformContext.PathStrategy.PathSeparator.Returns(_dirSeparator);
        _mockPlatformContext.PathStrategy.CombinePath(Arg.Any<FileSystemPathId>(), Arg.Any<string>())
            .Returns(copiedFilePathId);
        _mockEnvironmentContext.FileProviderService.FileExists(sourcePathId).Returns(true);
        _mockEnvironmentContext.FileProviderService.CopyFile(sourcePathId, destinationPathId, overrideExisting)
            .Returns(copiedFilePathId);
        _mockEnvironmentContext.FileProviderService.GetFileName(copiedFilePathId).Returns("file.txt");
        _mockEnvironmentContext.FileProviderService.GetLastWriteTime(copiedFilePathId)
            .Returns(Optional<DateTime>.FromNullable(DateTime.Now));
        _mockEnvironmentContext.FileProviderService.GetCreationTime(copiedFilePathId)
            .Returns(Optional<DateTime>.FromNullable(DateTime.Now.AddDays(-1)));
        _mockEnvironmentContext.FileProviderService.GetSize(copiedFilePathId).Returns((long?)1024);

        // Act
        ErrorOr<File> result = _sut.CopyFile(sourcePath, destinationPath, overrideExisting);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be("file.txt");
        result.Value.Status.Should().Be(FileSystemItemStatus.Accessible);
    }

    [Fact]
    public void CopyFile_WhenSourceDoesNotExist_ShouldReturnError()
    {
        // Arrange
        string sourcePath = _pathSourceNonExistingFile;
        string destinationPath = _pathDestination1;
        bool overrideExisting = true;
        FileSystemPathId sourcePathId = _fileSystemPathIdFixture.CreateFileSystemPathId(sourcePath);

        _mockEnvironmentContext.FileProviderService.FileExists(sourcePathId).Returns(ErrorOrFactory.From(false));

        // Act
        ErrorOr<File> result = _sut.CopyFile(sourcePath, destinationPath, overrideExisting);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.FileSystemManagement.FileNotFound);
    }

    [Fact]
    public void CopyFile_WhenFileProviderServiceReturnsError_ShouldPropagateError()
    {
        // Arrange
        string sourcePath = _pathSourceFile;
        string destinationPath = _pathDestination1;
        bool overrideExisting = true;
        FileSystemPathId sourcePathId = _fileSystemPathIdFixture.CreateFileSystemPathId(sourcePath);
        FileSystemPathId destinationPathId = _fileSystemPathIdFixture.CreateFileSystemPathId(destinationPath);

        _mockPlatformContext.PathStrategy.PathSeparator.Returns(_dirSeparator);
        _mockEnvironmentContext.FileProviderService.FileExists(sourcePathId).Returns(ErrorOrFactory.From(true));
        _mockEnvironmentContext.FileProviderService.CopyFile(sourcePathId, destinationPathId, overrideExisting)
            .Returns(Errors.Permission.UnauthorizedAccess);

        // Act
        ErrorOr<File> result = _sut.CopyFile(sourcePath, destinationPath, overrideExisting);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.Permission.UnauthorizedAccess);
    }

    [Fact]
    public void CopyFile_WhenRetrievingFileInfoFails_ShouldReturnInaccessibleFile()
    {
        // Arrange
        string sourcePath = _pathSourceFile;
        string destinationPath = _pathDestination1;
        bool overrideExisting = true;
        FileSystemPathId sourcePathId = _fileSystemPathIdFixture.CreateFileSystemPathId(sourcePath);
        FileSystemPathId destinationPathId = _fileSystemPathIdFixture.CreateFileSystemPathId(destinationPath);
        FileSystemPathId copiedFilePathId = _fileSystemPathIdFixture.CreateFileSystemPathId(_pathDestinationFile);

        _mockPlatformContext.PathStrategy.PathSeparator.Returns(_dirSeparator);
        _mockEnvironmentContext.FileProviderService.FileExists(sourcePathId).Returns(ErrorOrFactory.From(true));
        _mockEnvironmentContext.FileProviderService.CopyFile(sourcePathId, destinationPathId, overrideExisting)
            .Returns(ErrorOrFactory.From(copiedFilePathId));

        _mockEnvironmentContext.FileProviderService.GetFileName(copiedFilePathId).Returns(Errors.Permission.UnauthorizedAccess);
        _mockEnvironmentContext.FileProviderService.GetLastWriteTime(copiedFilePathId).Returns(Errors.Permission.UnauthorizedAccess);
        _mockEnvironmentContext.FileProviderService.GetCreationTime(copiedFilePathId).Returns(Errors.Permission.UnauthorizedAccess);
        _mockEnvironmentContext.FileProviderService.GetSize(copiedFilePathId).Returns(ErrorOrFactory.From((long?)null));

        // Act
        ErrorOr<File> result = _sut.CopyFile(sourcePath, destinationPath, overrideExisting);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().NotBeNull();
        result.Value.Status.Should().Be(FileSystemItemStatus.Inaccessible);
    }

    [Fact]
    public void CopyFile_WithInvalidSourcePath_ShouldReturnError()
    {
        // Arrange
        string invalidSourcePath = string.Empty;
        string destinationPath = _pathDestination1;
        bool overrideExisting = true;

        // Act
        ErrorOr<File> result = _sut.CopyFile(invalidSourcePath, destinationPath, overrideExisting);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.FileSystemManagement.InvalidPath);
    }

    [Fact]
    public void CopyFile_WithInvalidDestinationPath_ShouldReturnError()
    {
        // Arrange
        string sourcePath = _pathSourceFile;
        string invalidDestinationPath = string.Empty;
        bool overrideExisting = true;
        _mockPlatformContext.PathStrategy.PathSeparator.Returns(_dirSeparator);

        // Act
        ErrorOr<File> result = _sut.CopyFile(sourcePath, invalidDestinationPath, overrideExisting);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.FileSystemManagement.InvalidPath);
    }

    [Fact]
    public void CopyFile_WithValidFileSystemPathIds_ShouldReturnCopiedFile()
    {
        // Arrange
        FileSystemPathId sourcePathId = _fileSystemPathIdFixture.CreateFileSystemPathId(_pathSourceFile);
        FileSystemPathId destinationPathId = _fileSystemPathIdFixture.CreateFileSystemPathId(_pathDestination2);
        FileSystemPathId copiedFilePathId = _fileSystemPathIdFixture.CreateFileSystemPathId(_pathDestinationFile);
        bool overrideExisting = true;

        _mockEnvironmentContext.FileProviderService.FileExists(sourcePathId).Returns(true);
        _mockEnvironmentContext.FileProviderService.CopyFile(sourcePathId, destinationPathId, overrideExisting)
            .Returns(copiedFilePathId);
        _mockEnvironmentContext.FileProviderService.GetFileName(copiedFilePathId).Returns("file.txt");
        _mockEnvironmentContext.FileProviderService.GetLastWriteTime(copiedFilePathId)
            .Returns(Optional<DateTime>.FromNullable(DateTime.Now));
        _mockEnvironmentContext.FileProviderService.GetCreationTime(copiedFilePathId)
            .Returns(Optional<DateTime>.FromNullable(DateTime.Now.AddDays(-1)));
        _mockEnvironmentContext.FileProviderService.GetSize(copiedFilePathId).Returns((long?)1024);

        // Act
        ErrorOr<File> result = _sut.CopyFile(sourcePathId, destinationPathId, overrideExisting);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be("file.txt");
        result.Value.Status.Should().Be(FileSystemItemStatus.Accessible);
    }

    [Fact]
    public void CopyFileWithFileSystemPathIds_WhenSourceDoesNotExist_ShouldReturnError()
    {
        // Arrange
        FileSystemPathId sourcePathId = _fileSystemPathIdFixture.CreateFileSystemPathId(_pathSourceNonExistingFile);
        FileSystemPathId destinationPathId = _fileSystemPathIdFixture.CreateFileSystemPathId(_pathDestination2);
        bool overrideExisting = true;

        _mockEnvironmentContext.FileProviderService.FileExists(sourcePathId).Returns(false);

        // Act
        ErrorOr<File> result = _sut.CopyFile(sourcePathId, destinationPathId, overrideExisting);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.FileSystemManagement.FileNotFound);
    }

    [Fact]
    public void CopyFileWithFileSystemPathIds_WhenFileProviderServiceReturnsError_ShouldPropagateError()
    {
        // Arrange
        FileSystemPathId sourcePathId = _fileSystemPathIdFixture.CreateFileSystemPathId(_pathSourceFile);
        FileSystemPathId destinationPathId = _fileSystemPathIdFixture.CreateFileSystemPathId(_pathDestination2);
        bool overrideExisting = true;

        _mockEnvironmentContext.FileProviderService.FileExists(sourcePathId).Returns(true);
        _mockEnvironmentContext.FileProviderService.CopyFile(sourcePathId, destinationPathId, overrideExisting)
            .Returns(Errors.Permission.UnauthorizedAccess);

        // Act
        ErrorOr<File> result = _sut.CopyFile(sourcePathId, destinationPathId, overrideExisting);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.Permission.UnauthorizedAccess);
    }

    [Fact]
    public void CopyFileWithFileSystemPathIds_WhenRetrievingFileInfoFails_ShouldReturnInaccessibleFile()
    {
        // Arrange
        FileSystemPathId sourcePathId = _fileSystemPathIdFixture.CreateFileSystemPathId(_pathSourceFile);
        FileSystemPathId destinationPathId = _fileSystemPathIdFixture.CreateFileSystemPathId(_pathDestination2);
        FileSystemPathId copiedFilePathId = _fileSystemPathIdFixture.CreateFileSystemPathId(_pathDestinationFile);
        bool overrideExisting = true;

        _mockEnvironmentContext.FileProviderService.FileExists(sourcePathId).Returns(true);
        _mockEnvironmentContext.FileProviderService.CopyFile(sourcePathId, destinationPathId, overrideExisting)
            .Returns(copiedFilePathId);

        _mockEnvironmentContext.FileProviderService.GetFileName(copiedFilePathId).Returns(Errors.Permission.UnauthorizedAccess);
        _mockEnvironmentContext.FileProviderService.GetLastWriteTime(copiedFilePathId).Returns(Errors.Permission.UnauthorizedAccess);
        _mockEnvironmentContext.FileProviderService.GetCreationTime(copiedFilePathId).Returns(Errors.Permission.UnauthorizedAccess);
        _mockEnvironmentContext.FileProviderService.GetSize(copiedFilePathId).Returns(ErrorOrFactory.From((long?)null));

        // Act
        ErrorOr<File> result = _sut.CopyFile(sourcePathId, destinationPathId, overrideExisting);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().NotBeNull();
        result.Value.Status.Should().Be(FileSystemItemStatus.Inaccessible);
    }

    [Fact]
    public void MoveFile_WithValidPaths_ShouldReturnMovedFile()
    {
        // Arrange
        string sourcePath = _pathSourceFile;
        string destinationPath = _pathDestination1;
        bool overrideExisting = true;
        FileSystemPathId sourcePathId = _fileSystemPathIdFixture.CreateFileSystemPathId(sourcePath);
        FileSystemPathId destinationPathId = _fileSystemPathIdFixture.CreateFileSystemPathId(destinationPath);
        FileSystemPathId movedFilePathId = _fileSystemPathIdFixture.CreateFileSystemPathId(_pathDestinationFile);

        _mockPlatformContext.PathStrategy.PathSeparator.Returns(_dirSeparator);
        _mockPlatformContext.PathStrategy.CombinePath(Arg.Any<FileSystemPathId>(), Arg.Any<string>())
            .Returns(movedFilePathId);
        _mockEnvironmentContext.FileProviderService.FileExists(sourcePathId).Returns(true);
        _mockEnvironmentContext.FileProviderService.MoveFile(sourcePathId, destinationPathId, overrideExisting)
            .Returns(movedFilePathId);
        _mockEnvironmentContext.FileProviderService.GetFileName(movedFilePathId).Returns("file.txt");
        _mockEnvironmentContext.FileProviderService.GetLastWriteTime(movedFilePathId)
            .Returns(Optional<DateTime>.FromNullable(DateTime.Now));
        _mockEnvironmentContext.FileProviderService.GetCreationTime(movedFilePathId)
            .Returns(Optional<DateTime>.FromNullable(DateTime.Now.AddDays(-1)));
        _mockEnvironmentContext.FileProviderService.GetSize(movedFilePathId).Returns((long?)1024);

        // Act
        ErrorOr<File> result = _sut.MoveFile(sourcePath, destinationPath, overrideExisting);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be("file.txt");
        result.Value.Status.Should().Be(FileSystemItemStatus.Accessible);
    }

    [Fact]
    public void MoveFile_WhenSourceDoesNotExist_ShouldReturnError()
    {
        // Arrange
        string sourcePath = _pathSourceNonExistingFile;
        string destinationPath = _pathDestination2;
        bool overrideExisting = true;
        FileSystemPathId sourcePathId = _fileSystemPathIdFixture.CreateFileSystemPathId(sourcePath);

        _mockPlatformContext.PathStrategy.PathSeparator.Returns(_dirSeparator);
        _mockEnvironmentContext.FileProviderService.FileExists(sourcePathId).Returns(false);

        // Act
        ErrorOr<File> result = _sut.MoveFile(sourcePath, destinationPath, overrideExisting);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.FileSystemManagement.FileNotFound);
    }

    [Fact]
    public void MoveFile_WhenFileProviderServiceReturnsError_ShouldPropagateError()
    {
        // Arrange
        string sourcePath = _pathSourceFile;
        string destinationPath = _pathDestination1;
        bool overrideExisting = true;
        FileSystemPathId sourcePathId = _fileSystemPathIdFixture.CreateFileSystemPathId(sourcePath);
        FileSystemPathId destinationPathId = _fileSystemPathIdFixture.CreateFileSystemPathId(destinationPath);

        _mockPlatformContext.PathStrategy.PathSeparator.Returns(_dirSeparator);
        _mockEnvironmentContext.FileProviderService.FileExists(sourcePathId).Returns(true);
        _mockEnvironmentContext.FileProviderService.MoveFile(sourcePathId, destinationPathId, overrideExisting)
            .Returns(Errors.Permission.UnauthorizedAccess);

        // Act
        ErrorOr<File> result = _sut.MoveFile(sourcePath, destinationPath, overrideExisting);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.Permission.UnauthorizedAccess);
    }

    [Fact]
    public void MoveFile_WhenRetrievingFileInfoFails_ShouldReturnInaccessibleFile()
    {
        // Arrange
        string sourcePath = _pathSourceFile;
        string destinationPath = _pathDestination1;
        bool overrideExisting = true;
        FileSystemPathId sourcePathId = _fileSystemPathIdFixture.CreateFileSystemPathId(sourcePath);
        FileSystemPathId destinationPathId = _fileSystemPathIdFixture.CreateFileSystemPathId(destinationPath);
        FileSystemPathId movedFilePathId = _fileSystemPathIdFixture.CreateFileSystemPathId(_pathDestinationFile);

        _mockPlatformContext.PathStrategy.PathSeparator.Returns(_dirSeparator);
        _mockPlatformContext.PathStrategy.CombinePath(Arg.Any<FileSystemPathId>(), Arg.Any<string>())
            .Returns(movedFilePathId);
        _mockEnvironmentContext.FileProviderService.FileExists(sourcePathId).Returns(true);
        _mockEnvironmentContext.FileProviderService.MoveFile(sourcePathId, destinationPathId, overrideExisting)
            .Returns(movedFilePathId);

        _mockEnvironmentContext.FileProviderService.GetFileName(movedFilePathId).Returns(Errors.Permission.UnauthorizedAccess);
        _mockEnvironmentContext.FileProviderService.GetLastWriteTime(movedFilePathId).Returns(Errors.Permission.UnauthorizedAccess);
        _mockEnvironmentContext.FileProviderService.GetCreationTime(movedFilePathId).Returns(Errors.Permission.UnauthorizedAccess);
        _mockEnvironmentContext.FileProviderService.GetSize(movedFilePathId).Returns(ErrorOrFactory.From((long?)null));

        // Act
        ErrorOr<File> result = _sut.MoveFile(sourcePath, destinationPath, overrideExisting);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().NotBeNull();
        result.Value.Status.Should().Be(FileSystemItemStatus.Inaccessible);
    }

    [Fact]
    public void MoveFile_WithInvalidSourcePath_ShouldReturnError()
    {
        // Arrange
        string invalidSourcePath = string.Empty;
        string destinationPath = _pathDestination1;
        bool overrideExisting = true;

        _mockPlatformContext.PathStrategy.PathSeparator.Returns(_dirSeparator);

        // Act
        ErrorOr<File> result = _sut.MoveFile(invalidSourcePath, destinationPath, overrideExisting);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.FileSystemManagement.InvalidPath);
    }

    [Fact]
    public void MoveFile_WithInvalidDestinationPath_ShouldReturnError()
    {
        // Arrange
        string sourcePath = _pathSourceFile;
        string invalidDestinationPath = string.Empty;
        bool overrideExisting = true;

        _mockPlatformContext.PathStrategy.PathSeparator.Returns(_dirSeparator);

        // Act
        ErrorOr<File> result = _sut.MoveFile(sourcePath, invalidDestinationPath, overrideExisting);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.FileSystemManagement.InvalidPath);
    }

    [Fact]
    public void MoveFile_WithValidFileSystemPathIds_ShouldReturnMovedFile()
    {
        // Arrange
        FileSystemPathId sourcePathId = _fileSystemPathIdFixture.CreateFileSystemPathId(_pathSourceFile);
        FileSystemPathId destinationPathId = _fileSystemPathIdFixture.CreateFileSystemPathId(_pathDestination2);
        FileSystemPathId movedFilePathId = _fileSystemPathIdFixture.CreateFileSystemPathId(_pathDestinationFile);
        bool overrideExisting = true;

        _mockEnvironmentContext.FileProviderService.FileExists(sourcePathId).Returns(true);
        _mockEnvironmentContext.FileProviderService.MoveFile(sourcePathId, destinationPathId, overrideExisting)
            .Returns(movedFilePathId);
        _mockEnvironmentContext.FileProviderService.GetFileName(movedFilePathId).Returns("file.txt");
        _mockEnvironmentContext.FileProviderService.GetLastWriteTime(movedFilePathId)
            .Returns(Optional<DateTime>.FromNullable(DateTime.Now));
        _mockEnvironmentContext.FileProviderService.GetCreationTime(movedFilePathId)
            .Returns(Optional<DateTime>.FromNullable(DateTime.Now.AddDays(-1)));
        _mockEnvironmentContext.FileProviderService.GetSize(movedFilePathId).Returns((long?)1024);

        // Act
        ErrorOr<File> result = _sut.MoveFile(sourcePathId, destinationPathId, overrideExisting);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be("file.txt");
        result.Value.Status.Should().Be(FileSystemItemStatus.Accessible);
    }

    [Fact]
    public void MoveFileWithFileSystemPathIds_WhenSourceDoesNotExist_ShouldReturnError()
    {
        // Arrange
        FileSystemPathId sourcePathId = _fileSystemPathIdFixture.CreateFileSystemPathId(
            s_isLinux ? "/Source/nonexistent.txt" : _pathSourceNonExistingFile
        );
        FileSystemPathId destinationPathId = _fileSystemPathIdFixture.CreateFileSystemPathId(_pathDestination2);
        bool overrideExisting = true;

        _mockEnvironmentContext.FileProviderService.FileExists(sourcePathId).Returns(false);

        // Act
        ErrorOr<File> result = _sut.MoveFile(sourcePathId, destinationPathId, overrideExisting);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.FileSystemManagement.FileNotFound);
    }

    [Fact]
    public void MoveFileWithFileSystemPathIds_WhenFileProviderServiceReturnsError_ShouldPropagateError()
    {
        // Arrange
        FileSystemPathId sourcePathId = _fileSystemPathIdFixture.CreateFileSystemPathId(_pathSourceFile);
        FileSystemPathId destinationPathId = _fileSystemPathIdFixture.CreateFileSystemPathId(_pathDestination1);
        bool overrideExisting = true;

        _mockEnvironmentContext.FileProviderService.FileExists(sourcePathId).Returns(true);
        _mockEnvironmentContext.FileProviderService.MoveFile(sourcePathId, destinationPathId, overrideExisting)
            .Returns(Errors.Permission.UnauthorizedAccess);

        // Act
        ErrorOr<File> result = _sut.MoveFile(sourcePathId, destinationPathId, overrideExisting);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.Permission.UnauthorizedAccess);
    }

    [Fact]
    public void MoveFileWithFileSystemPathIds_WhenRetrievingFileInfoFails_ShouldReturnInaccessibleFile()
    {
        // Arrange
        FileSystemPathId sourcePathId = _fileSystemPathIdFixture.CreateFileSystemPathId(_pathSourceFile);
        FileSystemPathId destinationPathId = _fileSystemPathIdFixture.CreateFileSystemPathId(_pathDestination1);
        FileSystemPathId movedFilePathId = _fileSystemPathIdFixture.CreateFileSystemPathId(_pathDestinationFile);
        bool overrideExisting = true;

        _mockEnvironmentContext.FileProviderService.FileExists(sourcePathId).Returns(true);
        _mockEnvironmentContext.FileProviderService.MoveFile(sourcePathId, destinationPathId, overrideExisting)
            .Returns(movedFilePathId);

        _mockEnvironmentContext.FileProviderService.GetFileName(movedFilePathId).Returns(Errors.Permission.UnauthorizedAccess);
        _mockEnvironmentContext.FileProviderService.GetLastWriteTime(movedFilePathId).Returns(Errors.Permission.UnauthorizedAccess);
        _mockEnvironmentContext.FileProviderService.GetCreationTime(movedFilePathId).Returns(Errors.Permission.UnauthorizedAccess);
        _mockEnvironmentContext.FileProviderService.GetSize(movedFilePathId).Returns(ErrorOrFactory.From((long?)null));

        // Act
        ErrorOr<File> result = _sut.MoveFile(sourcePathId, destinationPathId, overrideExisting);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().NotBeNull();
        result.Value.Status.Should().Be(FileSystemItemStatus.Inaccessible);
    }

    [Fact]
    public void MoveFile_WhenFileExistsReturnsError_ShouldPropagateError()
    {
        // Arrange
        string sourcePath = _pathSourceFile;
        string destinationPath = _pathDestination1;
        bool overrideExisting = true;
        FileSystemPathId sourcePathId = _fileSystemPathIdFixture.CreateFileSystemPathId(sourcePath);

        _mockPlatformContext.PathStrategy.PathSeparator.Returns(_dirSeparator);
        _mockEnvironmentContext.FileProviderService.FileExists(sourcePathId)
            .Returns(Errors.Permission.UnauthorizedAccess);

        // Act
        ErrorOr<File> result = _sut.MoveFile(sourcePath, destinationPath, overrideExisting);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.Permission.UnauthorizedAccess);
    }

    [Fact]
    public void RenameFile_WithValidPathAndName_ShouldReturnRenamedFile()
    {
        // Arrange
        string path = _pathSourceOldFile;
        string newName = "newfile.txt";
        FileSystemPathId pathId = _fileSystemPathIdFixture.CreateFileSystemPathId(path);
        FileSystemPathId newPathId = _fileSystemPathIdFixture.CreateFileSystemPathId(_pathSourceNewFile);

        _mockPlatformContext.PathStrategy.PathSeparator.Returns(_dirSeparator);
        _mockPlatformContext.PathStrategy.CombinePath(Arg.Any<FileSystemPathId>(), newName)
            .Returns(ErrorOrFactory.From(newPathId));
        _mockEnvironmentContext.FileProviderService.FileExists(newPathId).Returns(false);
        _mockEnvironmentContext.FileProviderService.RenameFile(pathId, newName)
            .Returns(ErrorOrFactory.From(newPathId));
        _mockEnvironmentContext.FileProviderService.GetFileName(newPathId).Returns(ErrorOrFactory.From(newName));
        _mockEnvironmentContext.FileProviderService.GetLastWriteTime(newPathId)
            .Returns(ErrorOrFactory.From(Optional<DateTime>.FromNullable(DateTime.Now)));
        _mockEnvironmentContext.FileProviderService.GetCreationTime(newPathId)
            .Returns(ErrorOrFactory.From(Optional<DateTime>.FromNullable(DateTime.Now.AddDays(-1))));
        _mockEnvironmentContext.FileProviderService.GetSize(newPathId).Returns(ErrorOrFactory.From((long?)1024));

        // Act
        ErrorOr<File> result = _sut.RenameFile(path, newName);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be(newName);
        result.Value.Status.Should().Be(FileSystemItemStatus.Accessible);
    }

    [Fact]
    public void RenameFile_WhenNewNameAlreadyExists_ShouldReturnError()
    {
        // Arrange
        string path = _pathSourceOldFile;
        string newName = "existingfile.txt";
        FileSystemPathId newPathId = _fileSystemPathIdFixture.CreateFileSystemPathId(_pathSourceExistingFile);

        _mockPlatformContext.PathStrategy.PathSeparator.Returns(_dirSeparator);
        _mockPlatformContext.PathStrategy.CombinePath(Arg.Any<FileSystemPathId>(), newName)
            .Returns(ErrorOrFactory.From(newPathId));
        _mockEnvironmentContext.FileProviderService.FileExists(newPathId).Returns(true);

        // Act
        ErrorOr<File> result = _sut.RenameFile(path, newName);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.FileSystemManagement.FileAlreadyExists);
    }

    [Fact]
    public void RenameFile_WhenFileProviderServiceReturnsError_ShouldPropagateError()
    {
        // Arrange
        string path = _pathSourceOldFile;
        string newName = "newfile.txt";
        FileSystemPathId pathId = _fileSystemPathIdFixture.CreateFileSystemPathId(path);
        FileSystemPathId newPathId = _fileSystemPathIdFixture.CreateFileSystemPathId(_pathSourceNewFile);

        _mockPlatformContext.PathStrategy.PathSeparator.Returns(_dirSeparator);
        _mockPlatformContext.PathStrategy.CombinePath(Arg.Any<FileSystemPathId>(), newName)
            .Returns(ErrorOrFactory.From(newPathId));
        _mockEnvironmentContext.FileProviderService.FileExists(newPathId).Returns(false);
        _mockEnvironmentContext.FileProviderService.RenameFile(pathId, newName)
            .Returns(Errors.Permission.UnauthorizedAccess);

        // Act
        ErrorOr<File> result = _sut.RenameFile(path, newName);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.Permission.UnauthorizedAccess);
    }

    [Fact]
    public void RenameFile_WhenRetrievingFileInfoFails_ShouldReturnInaccessibleFile()
    {
        // Arrange
        string path = _pathSourceOldFile;
        string newName = "newfile.txt";
        FileSystemPathId pathId = _fileSystemPathIdFixture.CreateFileSystemPathId(path);
        FileSystemPathId newPathId = _fileSystemPathIdFixture.CreateFileSystemPathId(_pathSourceNewFile);

        _mockPlatformContext.PathStrategy.PathSeparator.Returns(_dirSeparator);
        _mockPlatformContext.PathStrategy.CombinePath(Arg.Any<FileSystemPathId>(), newName)
            .Returns(ErrorOrFactory.From(newPathId));
        _mockEnvironmentContext.FileProviderService.FileExists(newPathId).Returns(false);
        _mockEnvironmentContext.FileProviderService.RenameFile(pathId, newName)
            .Returns(ErrorOrFactory.From(newPathId));

        _mockEnvironmentContext.FileProviderService.GetFileName(newPathId).Returns(Errors.Permission.UnauthorizedAccess);
        _mockEnvironmentContext.FileProviderService.GetLastWriteTime(newPathId).Returns(Errors.Permission.UnauthorizedAccess);
        _mockEnvironmentContext.FileProviderService.GetCreationTime(newPathId).Returns(Errors.Permission.UnauthorizedAccess);
        _mockEnvironmentContext.FileProviderService.GetSize(newPathId).Returns(ErrorOrFactory.From((long?)null));

        // Act
        ErrorOr<File> result = _sut.RenameFile(path, newName);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().NotBeNull();
        result.Value.Status.Should().Be(FileSystemItemStatus.Inaccessible);
    }

    [Fact]
    public void RenameFile_WithInvalidPath_ShouldReturnError()
    {
        // Arrange
        string invalidPath = string.Empty;
        string newName = "newfile.txt";

        _mockPlatformContext.PathStrategy.PathSeparator.Returns(_dirSeparator);

        // Act
        ErrorOr<File> result = _sut.RenameFile(invalidPath, newName);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.FileSystemManagement.InvalidPath);
    }

    [Fact]
    public void RenameFileWithValidFileSystemPathIdAndName_ShouldReturnRenamedFile()
    {
        // Arrange
        FileSystemPathId pathId = _fileSystemPathIdFixture.CreateFileSystemPathId(_pathSourceOldFile);
        string newName = "newfile.txt";
        FileSystemPathId newPathId = _fileSystemPathIdFixture.CreateFileSystemPathId(_pathSourceNewFile);

        _mockPlatformContext.PathStrategy.CombinePath(Arg.Any<FileSystemPathId>(), newName)
            .Returns(ErrorOrFactory.From(newPathId));
        _mockEnvironmentContext.FileProviderService.FileExists(newPathId).Returns(false);
        _mockEnvironmentContext.FileProviderService.RenameFile(pathId, newName)
            .Returns(ErrorOrFactory.From(newPathId));
        _mockEnvironmentContext.FileProviderService.GetFileName(newPathId).Returns(ErrorOrFactory.From(newName));
        _mockEnvironmentContext.FileProviderService.GetLastWriteTime(newPathId)
            .Returns(ErrorOrFactory.From(Optional<DateTime>.FromNullable(DateTime.Now)));
        _mockEnvironmentContext.FileProviderService.GetCreationTime(newPathId)
            .Returns(ErrorOrFactory.From(Optional<DateTime>.FromNullable(DateTime.Now.AddDays(-1))));
        _mockEnvironmentContext.FileProviderService.GetSize(newPathId).Returns(ErrorOrFactory.From((long?)1024));

        // Act
        ErrorOr<File> result = _sut.RenameFile(pathId, newName);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be(newName);
        result.Value.Status.Should().Be(FileSystemItemStatus.Accessible);
    }

    [Fact]
    public void RenameFileWithFileSystemPathId_WhenNewNameAlreadyExists_ShouldReturnError()
    {
        // Arrange
        FileSystemPathId pathId = _fileSystemPathIdFixture.CreateFileSystemPathId(_pathSourceOldFile);
        string newName = "existingfile.txt";
        FileSystemPathId newPathId = _fileSystemPathIdFixture.CreateFileSystemPathId(_pathSourceExistingFile);

        _mockPlatformContext.PathStrategy.CombinePath(Arg.Any<FileSystemPathId>(), newName)
            .Returns(ErrorOrFactory.From(newPathId));
        _mockEnvironmentContext.FileProviderService.FileExists(newPathId).Returns(true);

        // Act
        ErrorOr<File> result = _sut.RenameFile(pathId, newName);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.FileSystemManagement.FileAlreadyExists);
    }

    [Fact]
    public void RenameFileWithFileSystemPathId_WhenFileProviderServiceReturnsError_ShouldPropagateError()
    {
        // Arrange
        FileSystemPathId pathId = _fileSystemPathIdFixture.CreateFileSystemPathId(_pathSourceOldFile);
        string newName = "newfile.txt";
        FileSystemPathId newPathId = _fileSystemPathIdFixture.CreateFileSystemPathId(_pathSourceNewFile);

        _mockPlatformContext.PathStrategy.CombinePath(Arg.Any<FileSystemPathId>(), newName)
            .Returns(ErrorOrFactory.From(newPathId));
        _mockEnvironmentContext.FileProviderService.FileExists(newPathId).Returns(false);
        _mockEnvironmentContext.FileProviderService.RenameFile(pathId, newName)
            .Returns(Errors.Permission.UnauthorizedAccess);

        // Act
        ErrorOr<File> result = _sut.RenameFile(pathId, newName);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.Permission.UnauthorizedAccess);
    }

    [Fact]
    public void RenameFileWithFileSystemPathId_WhenRetrievingFileInfoFails_ShouldReturnInaccessibleFile()
    {
        // Arrange
        FileSystemPathId pathId = _fileSystemPathIdFixture.CreateFileSystemPathId(_pathSourceOldFile);
        string newName = "newfile.txt";
        FileSystemPathId newPathId = _fileSystemPathIdFixture.CreateFileSystemPathId(_pathSourceNewFile);

        _mockPlatformContext.PathStrategy.CombinePath(Arg.Any<FileSystemPathId>(), newName)
            .Returns(ErrorOrFactory.From(newPathId));
        _mockEnvironmentContext.FileProviderService.FileExists(newPathId).Returns(false);
        _mockEnvironmentContext.FileProviderService.RenameFile(pathId, newName)
            .Returns(ErrorOrFactory.From(newPathId));

        _mockEnvironmentContext.FileProviderService.GetFileName(newPathId).Returns(Errors.Permission.UnauthorizedAccess);
        _mockEnvironmentContext.FileProviderService.GetLastWriteTime(newPathId).Returns(Errors.Permission.UnauthorizedAccess);
        _mockEnvironmentContext.FileProviderService.GetCreationTime(newPathId).Returns(Errors.Permission.UnauthorizedAccess);
        _mockEnvironmentContext.FileProviderService.GetSize(newPathId).Returns(ErrorOrFactory.From((long?)null));

        // Act
        ErrorOr<File> result = _sut.RenameFile(pathId, newName);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().NotBeNull();
        result.Value.Status.Should().Be(FileSystemItemStatus.Inaccessible);
    }

    [Fact]
    public void RenameFile_WhenCombinePathReturnsError_ShouldPropagateError()
    {
        // Arrange
        string path = _pathSourceOldFile;
        string newName = "newfile.txt";

        _mockPlatformContext.PathStrategy.PathSeparator.Returns(_dirSeparator);
        _mockPlatformContext.PathStrategy.CombinePath(Arg.Any<FileSystemPathId>(), newName)
            .Returns(Errors.FileSystemManagement.InvalidPath);

        // Act
        ErrorOr<File> result = _sut.RenameFile(path, newName);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.FileSystemManagement.InvalidPath);
    }

    [Fact]
    public void RenameFile_WhenFileExistsReturnsError_ShouldPropagateError()
    {
        // Arrange
        string path = _pathSourceOldFile;
        string newName = "newfile.txt";
        FileSystemPathId newPathId = _fileSystemPathIdFixture.CreateFileSystemPathId(_pathSourceNewFile);

        _mockPlatformContext.PathStrategy.PathSeparator.Returns(_dirSeparator);
        _mockPlatformContext.PathStrategy.CombinePath(Arg.Any<FileSystemPathId>(), newName)
            .Returns(ErrorOrFactory.From(newPathId));
        _mockEnvironmentContext.FileProviderService.FileExists(newPathId)
            .Returns(Errors.Permission.UnauthorizedAccess);

        // Act
        ErrorOr<File> result = _sut.RenameFile(path, newName);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.Permission.UnauthorizedAccess);
    }

    [Fact]
    public void RenameFileWithFileSystemPathId_WhenCombinePathReturnsError_ShouldPropagateError()
    {
        // Arrange
        FileSystemPathId pathId = _fileSystemPathIdFixture.CreateFileSystemPathId(_pathSourceOldFile);
        string newName = "newfile.txt";

        _mockPlatformContext.PathStrategy.CombinePath(Arg.Any<FileSystemPathId>(), newName)
            .Returns(Errors.FileSystemManagement.InvalidPath);

        // Act
        ErrorOr<File> result = _sut.RenameFile(pathId, newName);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.FileSystemManagement.InvalidPath);
    }

    [Fact]
    public void RenameFileWithFileSystemPathId_WhenFileExistsReturnsError_ShouldPropagateError()
    {
        // Arrange
        FileSystemPathId pathId = _fileSystemPathIdFixture.CreateFileSystemPathId(_pathSourceOldFile);
        string newName = "newfile.txt";
        FileSystemPathId newPathId = _fileSystemPathIdFixture.CreateFileSystemPathId(_pathSourceNewFile);

        _mockPlatformContext.PathStrategy.CombinePath(Arg.Any<FileSystemPathId>(), newName)
            .Returns(ErrorOrFactory.From(newPathId));
        _mockEnvironmentContext.FileProviderService.FileExists(newPathId)
            .Returns(Errors.Permission.UnauthorizedAccess);

        // Act
        ErrorOr<File> result = _sut.RenameFile(pathId, newName);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.Permission.UnauthorizedAccess);
    }

    [Fact]
    public void DeleteFile_WithValidPath_ShouldReturnDeleted()
    {
        // Arrange
        string path = _pathSourceFile;
        FileSystemPathId pathId = _fileSystemPathIdFixture.CreateFileSystemPathId(path);

        _mockEnvironmentContext.FileProviderService.DeleteFile(pathId)
            .Returns(Result.Deleted);

        // Act
        ErrorOr<Deleted> result = _sut.DeleteFile(path);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(Result.Deleted);
    }

    [Fact]
    public void DeleteFile_WithInvalidPath_ShouldReturnError()
    {
        // Arrange
        string invalidPath = string.Empty;

        // Act
        ErrorOr<Deleted> result = _sut.DeleteFile(invalidPath);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.FileSystemManagement.InvalidPath);
    }

    [Fact]
    public void DeleteFile_WhenFileProviderServiceReturnsError_ShouldPropagateError()
    {
        // Arrange
        string path = _pathSourceFile;
        FileSystemPathId pathId = _fileSystemPathIdFixture.CreateFileSystemPathId(path);

        _mockEnvironmentContext.FileProviderService.DeleteFile(pathId)
            .Returns(Errors.Permission.UnauthorizedAccess);

        // Act
        ErrorOr<Deleted> result = _sut.DeleteFile(path);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.Permission.UnauthorizedAccess);
    }

    [Fact]
    public void DeleteFile_WithValidFileSystemPathId_ShouldReturnDeleted()
    {
        // Arrange
        FileSystemPathId pathId = _fileSystemPathIdFixture.CreateFileSystemPathId(_pathSourceFile);

        _mockEnvironmentContext.FileProviderService.DeleteFile(pathId)
            .Returns(Result.Deleted);

        // Act
        ErrorOr<Deleted> result = _sut.DeleteFile(pathId);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(Result.Deleted);
    }

    [Fact]
    public void DeleteFileWithFileSystemPathId_WhenFileProviderServiceReturnsError_ShouldPropagateError()
    {
        // Arrange
        FileSystemPathId pathId = _fileSystemPathIdFixture.CreateFileSystemPathId(_pathSourceFile);

        _mockEnvironmentContext.FileProviderService.DeleteFile(pathId)
            .Returns(Errors.Permission.UnauthorizedAccess);

        // Act
        ErrorOr<Deleted> result = _sut.DeleteFile(pathId);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.Permission.UnauthorizedAccess);
    }
}
