#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Entities;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Services;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Strategies.Environment;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Strategies.Platform;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Entities;
using Lumina.Domain.Fixtures.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.FileSystem;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;

#endregion

namespace Lumina.Domain.UnitTests.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Services;

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
    private readonly FileSystemPathIdFixture _fileSystemPathIdFixture = new();
    private readonly FileFixture _fileFixture = new();
    private static readonly bool s_isUnix = RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
    private readonly string _pathTestDir = s_isUnix ? "/TestDir" : @"C:\TestDir";
    private readonly string _pathTestDirFile1 = s_isUnix ? "/TestDir/File1.txt" : @"C:\TestDir\File1.txt";
    private readonly string _pathTestDirFile2 = s_isUnix ? "/TestDir/File2.txt" : @"C:\TestDir\File2.txt";
    private readonly string _pathTestDirInaccessible = s_isUnix ? "/TestDir/InaccessibleFile.txt" : @"C:\TestDir\InaccessibleFile.txt";
    private readonly string _pathDestination1 = s_isUnix ? "/Destination/" : @"C:\Destination\";
    private readonly string _pathDestination2 = s_isUnix ? "/Destination" : @"C:\Destination";
    private readonly string _pathSourceFile = s_isUnix ? "/Source/file.txt" : @"C:\Source\file.txt";
    private readonly string _pathDestinationFile = s_isUnix ? "/Destination/file.txt" : @"C:\Destination\file.txt";
    private readonly string _pathSourceOldFile = s_isUnix ? "/Source/oldfile.txt" : @"C:\Source\oldfile.txt";
    private readonly string _pathSourceNewFile = s_isUnix ? "/Source/newfile.txt" : @"C:\Source\newfile.txt";
    private readonly string _pathSourceExistingFile = s_isUnix ? "/Source/existingfile.txt" : @"C:\Source\existingfile.txt";
    private readonly string _pathSourceNonExistingFile = s_isUnix ? "/Source/nonexistent.txt" : @"C:\Source\nonexistent.txt";
    private readonly char _dirSeparator = s_isUnix ? '/' : '\\';

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
    }

    [Fact]
    public void GetFiles_WithValidPath_ShouldReturnListOfFiles()
    {
        // Arrange
        string path = _pathTestDir;
        bool includeHiddenElements = false;
        FileSystemPathId pathId = _fileSystemPathIdFixture.Create(path);
        FileSystemPathId[] filePaths =
        [
            _fileSystemPathIdFixture.Create(_pathTestDirFile1),
            _fileSystemPathIdFixture.Create(_pathTestDirFile2)
        ];

        _mockEnvironmentContext.FileProviderService.GetFilePaths(pathId, includeHiddenElements)
            .Returns(Result.From(filePaths.AsEnumerable()));

        foreach (FileSystemPathId filePath in filePaths)
        {
            _mockEnvironmentContext.FileProviderService.GetFileName(filePath)
                .Returns(Result.From(System.IO.Path.GetFileName(filePath.Path)));
            _mockEnvironmentContext.FileProviderService.GetLastWriteTime(filePath)
                .Returns(Result.From(Optional<DateTime>.FromNullable(DateTime.Now)));
            _mockEnvironmentContext.FileProviderService.GetCreationTime(filePath)
                .Returns(Result.From(Optional<DateTime>.FromNullable(DateTime.Now.AddDays(-1))));
            _mockEnvironmentContext.FileProviderService.GetSize(filePath)
                .Returns(Result.From((long?)1024));
        }

        // Act
        Result<IEnumerable<File>> result = _sut.GetFiles(path, includeHiddenElements);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(2, result.Value.Count());
        Assert.Equal(["File1.txt", "File2.txt"], result.Value.Select(f => f.Name));
    }

    [Fact]
    public void GetFiles_WithInvalidPath_ShouldReturnError()
    {
        // Arrange
        string invalidPath = string.Empty;
        bool includeHiddenElements = false;

        // Act
        Result<IEnumerable<File>> result = _sut.GetFiles(invalidPath, includeHiddenElements);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.FileSystemManagement.InvalidPath, result.FirstError);
    }

    [Fact]
    public void GetFiles_WhenFileProviderServiceReturnsError_ShouldPropagateError()
    {
        // Arrange
        string path = _pathTestDir;
        bool includeHiddenElements = false;
        FileSystemPathId pathId = _fileSystemPathIdFixture.Create(path);

        _mockEnvironmentContext.FileProviderService.GetFilePaths(pathId, includeHiddenElements)
            .Returns(Errors.Permission.UnauthorizedAccess);

        // Act
        Result<IEnumerable<File>> result = _sut.GetFiles(path, includeHiddenElements);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Permission.UnauthorizedAccess, result.FirstError);
    }

    [Fact]
    public void GetFiles_WhenFileDetailsAreInaccessible_ShouldReturnInaccessibleFile()
    {
        // Arrange
        string path = _pathTestDir;
        bool includeHiddenElements = false;
        FileSystemPathId pathId = _fileSystemPathIdFixture.Create(path);
        FileSystemPathId filePath = _fileSystemPathIdFixture.Create(_pathTestDirInaccessible);

        _mockEnvironmentContext.FileProviderService.GetFilePaths(pathId, includeHiddenElements)
            .Returns(Result.From(new[] { filePath }.AsEnumerable()));

        _mockEnvironmentContext.FileProviderService.GetFileName(filePath)
            .Returns(Errors.Permission.UnauthorizedAccess);
        _mockEnvironmentContext.FileProviderService.GetLastWriteTime(filePath)
            .Returns(Errors.Permission.UnauthorizedAccess);
        _mockEnvironmentContext.FileProviderService.GetCreationTime(filePath)
            .Returns(Errors.Permission.UnauthorizedAccess);
        _mockEnvironmentContext.FileProviderService.GetSize(filePath)
            .Returns(Result.From((long?)null));

        // Act
        Result<IEnumerable<File>> result = _sut.GetFiles(path, includeHiddenElements);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Single(result.Value);
        Assert.Equal(FileSystemItemStatus.Inaccessible, result.Value.First().Status);
    }

    [Fact]
    public void GetFilesOverload_WithValidFile_ShouldReturnListOfFiles()
    {
        // Arrange
        File parentFile = _fileFixture.Create();
        bool includeHiddenElements = false;
        FileSystemPathId[] filePaths =
        [
            _fileSystemPathIdFixture.Create(_pathTestDirFile1),
            _fileSystemPathIdFixture.Create(_pathTestDirFile2)
        ];

        _mockEnvironmentContext.FileProviderService.GetFilePaths(parentFile.Id, includeHiddenElements)
            .Returns(Result.From(filePaths.AsEnumerable()));

        foreach (FileSystemPathId filePath in filePaths)
        {
            _mockEnvironmentContext.FileProviderService.GetFileName(filePath)
                .Returns(Result.From(System.IO.Path.GetFileName(filePath.Path)));
            _mockEnvironmentContext.FileProviderService.GetLastWriteTime(filePath)
                .Returns(Result.From(Optional<DateTime>.FromNullable(DateTime.Now)));
            _mockEnvironmentContext.FileProviderService.GetCreationTime(filePath)
                .Returns(Result.From(Optional<DateTime>.FromNullable(DateTime.Now.AddDays(-1))));
            _mockEnvironmentContext.FileProviderService.GetSize(filePath)
                .Returns(Result.From((long?)1024));
        }

        // Act
        Result<IEnumerable<File>> result = _sut.GetFiles(parentFile, includeHiddenElements);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(2, result.Value.Count());
        Assert.Equal(["File1.txt", "File2.txt"], result.Value.Select(f => f.Name));
    }

    [Fact]
    public void GetFilesOverload_WhenFileProviderServiceReturnsError_ShouldPropagateError()
    {
        // Arrange
        File parentFile = _fileFixture.Create();
        bool includeHiddenElements = false;

        _mockEnvironmentContext.FileProviderService.GetFilePaths(parentFile.Id, includeHiddenElements)
            .Returns(Errors.Permission.UnauthorizedAccess);

        // Act
        Result<IEnumerable<File>> result = _sut.GetFiles(parentFile, includeHiddenElements);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Permission.UnauthorizedAccess, result.FirstError);
    }

    [Fact]
    public void GetFilesOverload_WhenFileDetailsAreInaccessible_ShouldReturnInaccessibleFile()
    {
        // Arrange
        File parentFile = _fileFixture.Create();
        bool includeHiddenElements = false;
        FileSystemPathId filePath = _fileSystemPathIdFixture.Create(_pathTestDirInaccessible);

        _mockEnvironmentContext.FileProviderService.GetFilePaths(parentFile.Id, includeHiddenElements)
            .Returns(Result.From(new[] { filePath }.AsEnumerable()));

        _mockEnvironmentContext.FileProviderService.GetFileName(filePath)
            .Returns(Errors.Permission.UnauthorizedAccess);
        _mockEnvironmentContext.FileProviderService.GetLastWriteTime(filePath)
            .Returns(Errors.Permission.UnauthorizedAccess);
        _mockEnvironmentContext.FileProviderService.GetCreationTime(filePath)
            .Returns(Errors.Permission.UnauthorizedAccess);
        _mockEnvironmentContext.FileProviderService.GetSize(filePath)
            .Returns(Result.From((long?)null));

        // Act
        Result<IEnumerable<File>> result = _sut.GetFiles(parentFile, includeHiddenElements);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Single(result.Value);
        Assert.Equal(FileSystemItemStatus.Inaccessible, result.Value.First().Status);
    }

    [Fact]
    public void GetFiles_WithValidFileSystemPathId_ShouldReturnListOfFiles()
    {
        // Arrange
        FileSystemPathId pathId = _fileSystemPathIdFixture.Create(_pathTestDir);
        bool includeHiddenElements = false;
        FileSystemPathId[] filePaths =
        [
            _fileSystemPathIdFixture.Create(_pathTestDirFile1),
            _fileSystemPathIdFixture.Create(_pathTestDirFile2)
        ];

        _mockEnvironmentContext.FileProviderService.GetFilePaths(pathId, includeHiddenElements)
            .Returns(Result.From(filePaths.AsEnumerable()));

        foreach (FileSystemPathId filePath in filePaths)
        {
            _mockEnvironmentContext.FileProviderService.GetFileName(filePath)
                .Returns(Result.From(System.IO.Path.GetFileName(filePath.Path)));
            _mockEnvironmentContext.FileProviderService.GetLastWriteTime(filePath)
                .Returns(Result.From(Optional<DateTime>.FromNullable(DateTime.Now)));
            _mockEnvironmentContext.FileProviderService.GetCreationTime(filePath)
                .Returns(Result.From(Optional<DateTime>.FromNullable(DateTime.Now.AddDays(-1))));
            _mockEnvironmentContext.FileProviderService.GetSize(filePath)
                .Returns(Result.From((long?)1024));
        }

        // Act
        Result<IEnumerable<File>> result = _sut.GetFiles(pathId, includeHiddenElements);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(2, result.Value.Count());
        Assert.Equal(["File1.txt", "File2.txt"], result.Value.Select(f => f.Name));
    }

    [Fact]
    public void GetFilesWithFileSystemPathId_WhenFileProviderServiceReturnsError_ShouldPropagateError()
    {
        // Arrange
        FileSystemPathId pathId = _fileSystemPathIdFixture.Create(_pathTestDir);
        bool includeHiddenElements = false;

        _mockEnvironmentContext.FileProviderService.GetFilePaths(pathId, includeHiddenElements)
            .Returns(Errors.Permission.UnauthorizedAccess);

        // Act
        Result<IEnumerable<File>> result = _sut.GetFiles(pathId, includeHiddenElements);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Permission.UnauthorizedAccess, result.FirstError);
    }

    [Fact]
    public void GetFilesWithFileSystemPathId_WhenFileDetailsAreInaccessible_ShouldReturnInaccessibleFile()
    {
        // Arrange
        FileSystemPathId pathId = _fileSystemPathIdFixture.Create(_pathTestDir);
        bool includeHiddenElements = false;
        FileSystemPathId filePath = _fileSystemPathIdFixture.Create(_pathTestDirInaccessible);

        _mockEnvironmentContext.FileProviderService.GetFilePaths(pathId, includeHiddenElements)
            .Returns(Result.From(new[] { filePath }.AsEnumerable()));

        _mockEnvironmentContext.FileProviderService.GetFileName(filePath)
            .Returns(Errors.Permission.UnauthorizedAccess);
        _mockEnvironmentContext.FileProviderService.GetLastWriteTime(filePath)
            .Returns(Errors.Permission.UnauthorizedAccess);
        _mockEnvironmentContext.FileProviderService.GetCreationTime(filePath)
            .Returns(Errors.Permission.UnauthorizedAccess);
        _mockEnvironmentContext.FileProviderService.GetSize(filePath)
            .Returns(Result.From((long?)null));

        // Act
        Result<IEnumerable<File>> result = _sut.GetFiles(pathId, includeHiddenElements);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Single(result.Value);
        Assert.Equal(FileSystemItemStatus.Inaccessible, result.Value.First().Status);
    }

    [Fact]
    public void GetFilesWithFileSystemPathId_WhenSizeRetrievalFails_ShouldReturnInaccessibleFileWithZeroSize()
    {
        // Arrange
        FileSystemPathId pathId = _fileSystemPathIdFixture.Create(_pathTestDir);
        bool includeHiddenElements = false;
        FileSystemPathId filePath = _fileSystemPathIdFixture.Create(_pathTestDirFile1);

        _mockEnvironmentContext.FileProviderService.GetFilePaths(pathId, includeHiddenElements)
            .Returns(Result.From(new[] { filePath }.AsEnumerable()));
        _mockEnvironmentContext.FileProviderService.GetFileName(filePath)
            .Returns(Result.From("File1.txt"));
        _mockEnvironmentContext.FileProviderService.GetLastWriteTime(filePath)
            .Returns(Result.From(Optional<DateTime>.FromNullable(DateTime.Now)));
        _mockEnvironmentContext.FileProviderService.GetCreationTime(filePath)
            .Returns(Result.From(Optional<DateTime>.FromNullable(DateTime.Now.AddDays(-1))));
        _mockEnvironmentContext.FileProviderService.GetSize(filePath)
            .Returns(Errors.Permission.UnauthorizedAccess);

        // Act
        Result<IEnumerable<File>> result = _sut.GetFiles(pathId, includeHiddenElements);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Single(result.Value);
        Assert.Equal(FileSystemItemStatus.Inaccessible, result.Value.First().Status);
        Assert.Equal(0, result.Value.First().Size);
    }

    [Fact]
    public void CopyFile_WithValidPaths_ShouldReturnCopiedFile()
    {
        // Arrange
        string sourcePath = _pathSourceFile;
        string destinationPath = _pathDestination1;
        bool overrideExisting = true;
        FileSystemPathId sourcePathId = _fileSystemPathIdFixture.Create(sourcePath);
        FileSystemPathId destinationPathId = _fileSystemPathIdFixture.Create(destinationPath);
        FileSystemPathId copiedFilePathId = _fileSystemPathIdFixture.Create(_pathDestinationFile);

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
        Result<File> result = _sut.CopyFile(sourcePath, destinationPath, overrideExisting);

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.Equal("file.txt", result.Value.Name);
        Assert.Equal(FileSystemItemStatus.Accessible, result.Value.Status);
    }

    [Fact]
    public void CopyFile_WhenSourceDoesNotExist_ShouldReturnError()
    {
        // Arrange
        string sourcePath = _pathSourceNonExistingFile;
        string destinationPath = _pathDestination1;
        bool overrideExisting = true;
        FileSystemPathId sourcePathId = _fileSystemPathIdFixture.Create(sourcePath);

        _mockEnvironmentContext.FileProviderService.FileExists(sourcePathId).Returns(Result.From(false));

        // Act
        Result<File> result = _sut.CopyFile(sourcePath, destinationPath, overrideExisting);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.FileSystemManagement.FileNotFound, result.FirstError);
    }

    [Fact]
    public void CopyFile_WhenFileProviderServiceReturnsError_ShouldPropagateError()
    {
        // Arrange
        string sourcePath = _pathSourceFile;
        string destinationPath = _pathDestination1;
        bool overrideExisting = true;
        FileSystemPathId sourcePathId = _fileSystemPathIdFixture.Create(sourcePath);
        FileSystemPathId destinationPathId = _fileSystemPathIdFixture.Create(destinationPath);

        _mockPlatformContext.PathStrategy.PathSeparator.Returns(_dirSeparator);
        _mockEnvironmentContext.FileProviderService.FileExists(sourcePathId).Returns(Result.From(true));
        _mockEnvironmentContext.FileProviderService.CopyFile(sourcePathId, destinationPathId, overrideExisting)
            .Returns(Errors.Permission.UnauthorizedAccess);

        // Act
        Result<File> result = _sut.CopyFile(sourcePath, destinationPath, overrideExisting);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Permission.UnauthorizedAccess, result.FirstError);
    }

    [Fact]
    public void CopyFile_WhenRetrievingFileInfoFails_ShouldReturnInaccessibleFile()
    {
        // Arrange
        string sourcePath = _pathSourceFile;
        string destinationPath = _pathDestination1;
        bool overrideExisting = true;
        FileSystemPathId sourcePathId = _fileSystemPathIdFixture.Create(sourcePath);
        FileSystemPathId destinationPathId = _fileSystemPathIdFixture.Create(destinationPath);
        FileSystemPathId copiedFilePathId = _fileSystemPathIdFixture.Create(_pathDestinationFile);

        _mockPlatformContext.PathStrategy.PathSeparator.Returns(_dirSeparator);
        _mockEnvironmentContext.FileProviderService.FileExists(sourcePathId).Returns(Result.From(true));
        _mockEnvironmentContext.FileProviderService.CopyFile(sourcePathId, destinationPathId, overrideExisting)
            .Returns(Result.From(copiedFilePathId));

        _mockEnvironmentContext.FileProviderService.GetFileName(copiedFilePathId).Returns(Errors.Permission.UnauthorizedAccess);
        _mockEnvironmentContext.FileProviderService.GetLastWriteTime(copiedFilePathId).Returns(Errors.Permission.UnauthorizedAccess);
        _mockEnvironmentContext.FileProviderService.GetCreationTime(copiedFilePathId).Returns(Errors.Permission.UnauthorizedAccess);
        _mockEnvironmentContext.FileProviderService.GetSize(copiedFilePathId).Returns(Result.From((long?)null));

        // Act
        Result<File> result = _sut.CopyFile(sourcePath, destinationPath, overrideExisting);

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.Equal(FileSystemItemStatus.Inaccessible, result.Value.Status);
    }

    [Fact]
    public void CopyFile_WhenFileExistsReturnsError_ShouldPropagateError()
    {
        // Arrange
        string sourcePath = _pathSourceFile;
        string destinationPath = _pathDestination1;
        bool overrideExisting = true;
        FileSystemPathId sourcePathId = _fileSystemPathIdFixture.Create(sourcePath);

        _mockPlatformContext.PathStrategy.PathSeparator.Returns(_dirSeparator);
        _mockEnvironmentContext.FileProviderService.FileExists(sourcePathId)
            .Returns(Errors.Permission.UnauthorizedAccess);

        // Act
        Result<File> result = _sut.CopyFile(sourcePath, destinationPath, overrideExisting);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Permission.UnauthorizedAccess, result.FirstError);
    }

    [Fact]
    public void CopyFile_WithInvalidSourcePath_ShouldReturnError()
    {
        // Arrange
        string invalidSourcePath = string.Empty;
        string destinationPath = _pathDestination1;
        bool overrideExisting = true;

        // Act
        Result<File> result = _sut.CopyFile(invalidSourcePath, destinationPath, overrideExisting);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.FileSystemManagement.InvalidPath, result.FirstError);
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
        Result<File> result = _sut.CopyFile(sourcePath, invalidDestinationPath, overrideExisting);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.FileSystemManagement.InvalidPath, result.FirstError);
    }

    [Fact]
    public void CopyFile_WithValidFileSystemPathIds_ShouldReturnCopiedFile()
    {
        // Arrange
        FileSystemPathId sourcePathId = _fileSystemPathIdFixture.Create(_pathSourceFile);
        FileSystemPathId destinationPathId = _fileSystemPathIdFixture.Create(_pathDestination2);
        FileSystemPathId copiedFilePathId = _fileSystemPathIdFixture.Create(_pathDestinationFile);
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
        Result<File> result = _sut.CopyFile(sourcePathId, destinationPathId, overrideExisting);

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.Equal("file.txt", result.Value.Name);
        Assert.Equal(FileSystemItemStatus.Accessible, result.Value.Status);
    }

    [Fact]
    public void CopyFileWithFileSystemPathIds_WhenSourceDoesNotExist_ShouldReturnError()
    {
        // Arrange
        FileSystemPathId sourcePathId = _fileSystemPathIdFixture.Create(_pathSourceNonExistingFile);
        FileSystemPathId destinationPathId = _fileSystemPathIdFixture.Create(_pathDestination2);
        bool overrideExisting = true;

        _mockEnvironmentContext.FileProviderService.FileExists(sourcePathId).Returns(false);

        // Act
        Result<File> result = _sut.CopyFile(sourcePathId, destinationPathId, overrideExisting);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.FileSystemManagement.FileNotFound, result.FirstError);
    }

    [Fact]
    public void CopyFileWithFileSystemPathIds_WhenFileProviderServiceReturnsError_ShouldPropagateError()
    {
        // Arrange
        FileSystemPathId sourcePathId = _fileSystemPathIdFixture.Create(_pathSourceFile);
        FileSystemPathId destinationPathId = _fileSystemPathIdFixture.Create(_pathDestination2);
        bool overrideExisting = true;

        _mockEnvironmentContext.FileProviderService.FileExists(sourcePathId).Returns(true);
        _mockEnvironmentContext.FileProviderService.CopyFile(sourcePathId, destinationPathId, overrideExisting)
            .Returns(Errors.Permission.UnauthorizedAccess);

        // Act
        Result<File> result = _sut.CopyFile(sourcePathId, destinationPathId, overrideExisting);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Permission.UnauthorizedAccess, result.FirstError);
    }

    [Fact]
    public void CopyFileWithFileSystemPathIds_WhenRetrievingFileInfoFails_ShouldReturnInaccessibleFile()
    {
        // Arrange
        FileSystemPathId sourcePathId = _fileSystemPathIdFixture.Create(_pathSourceFile);
        FileSystemPathId destinationPathId = _fileSystemPathIdFixture.Create(_pathDestination2);
        FileSystemPathId copiedFilePathId = _fileSystemPathIdFixture.Create(_pathDestinationFile);
        bool overrideExisting = true;

        _mockEnvironmentContext.FileProviderService.FileExists(sourcePathId).Returns(true);
        _mockEnvironmentContext.FileProviderService.CopyFile(sourcePathId, destinationPathId, overrideExisting)
            .Returns(copiedFilePathId);

        _mockEnvironmentContext.FileProviderService.GetFileName(copiedFilePathId).Returns(Errors.Permission.UnauthorizedAccess);
        _mockEnvironmentContext.FileProviderService.GetLastWriteTime(copiedFilePathId).Returns(Errors.Permission.UnauthorizedAccess);
        _mockEnvironmentContext.FileProviderService.GetCreationTime(copiedFilePathId).Returns(Errors.Permission.UnauthorizedAccess);
        _mockEnvironmentContext.FileProviderService.GetSize(copiedFilePathId).Returns(Result.From((long?)null));

        // Act
        Result<File> result = _sut.CopyFile(sourcePathId, destinationPathId, overrideExisting);

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.Equal(FileSystemItemStatus.Inaccessible, result.Value.Status);
    }

    [Fact]
    public void CopyFileWithFileSystemPathIds_WhenFileExistsReturnsError_ShouldPropagateError()
    {
        // Arrange
        FileSystemPathId sourcePathId = _fileSystemPathIdFixture.Create(_pathSourceFile);
        FileSystemPathId destinationPathId = _fileSystemPathIdFixture.Create(_pathDestination2);
        bool overrideExisting = true;

        _mockEnvironmentContext.FileProviderService.FileExists(sourcePathId)
            .Returns(Errors.Permission.UnauthorizedAccess);

        // Act
        Result<File> result = _sut.CopyFile(sourcePathId, destinationPathId, overrideExisting);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Permission.UnauthorizedAccess, result.FirstError);
    }

    [Fact]
    public void CopyFileWithFileSystemPathIds_WhenSizeRetrievalFails_ShouldReturnInaccessibleFileWithZeroSize()
    {
        // Arrange
        FileSystemPathId sourcePathId = _fileSystemPathIdFixture.Create(_pathSourceFile);
        FileSystemPathId destinationPathId = _fileSystemPathIdFixture.Create(_pathDestination2);
        FileSystemPathId copiedFilePathId = _fileSystemPathIdFixture.Create(_pathDestinationFile);
        bool overrideExisting = true;

        _mockEnvironmentContext.FileProviderService.FileExists(sourcePathId).Returns(true);
        _mockEnvironmentContext.FileProviderService.CopyFile(sourcePathId, destinationPathId, overrideExisting)
            .Returns(copiedFilePathId);
        _mockEnvironmentContext.FileProviderService.GetFileName(copiedFilePathId).Returns("file.txt");
        _mockEnvironmentContext.FileProviderService.GetLastWriteTime(copiedFilePathId)
            .Returns(Optional<DateTime>.FromNullable(DateTime.Now));
        _mockEnvironmentContext.FileProviderService.GetCreationTime(copiedFilePathId)
            .Returns(Optional<DateTime>.FromNullable(DateTime.Now.AddDays(-1)));
        _mockEnvironmentContext.FileProviderService.GetSize(copiedFilePathId)
            .Returns(Errors.Permission.UnauthorizedAccess);

        // Act
        Result<File> result = _sut.CopyFile(sourcePathId, destinationPathId, overrideExisting);

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.Equal(FileSystemItemStatus.Inaccessible, result.Value.Status);
        Assert.Equal(0, result.Value.Size);
    }

    [Fact]
    public void MoveFile_WithValidPaths_ShouldReturnMovedFile()
    {
        // Arrange
        string sourcePath = _pathSourceFile;
        string destinationPath = _pathDestination1;
        bool overrideExisting = true;
        FileSystemPathId sourcePathId = _fileSystemPathIdFixture.Create(sourcePath);
        FileSystemPathId destinationPathId = _fileSystemPathIdFixture.Create(destinationPath);
        FileSystemPathId movedFilePathId = _fileSystemPathIdFixture.Create(_pathDestinationFile);

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
        Result<File> result = _sut.MoveFile(sourcePath, destinationPath, overrideExisting);

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.Equal("file.txt", result.Value.Name);
        Assert.Equal(FileSystemItemStatus.Accessible, result.Value.Status);
    }

    [Fact]
    public void MoveFile_WhenSourceDoesNotExist_ShouldReturnError()
    {
        // Arrange
        string sourcePath = _pathSourceNonExistingFile;
        string destinationPath = _pathDestination2;
        bool overrideExisting = true;
        FileSystemPathId sourcePathId = _fileSystemPathIdFixture.Create(sourcePath);

        _mockPlatformContext.PathStrategy.PathSeparator.Returns(_dirSeparator);
        _mockEnvironmentContext.FileProviderService.FileExists(sourcePathId).Returns(false);

        // Act
        Result<File> result = _sut.MoveFile(sourcePath, destinationPath, overrideExisting);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.FileSystemManagement.FileNotFound, result.FirstError);
    }

    [Fact]
    public void MoveFile_WhenFileProviderServiceReturnsError_ShouldPropagateError()
    {
        // Arrange
        string sourcePath = _pathSourceFile;
        string destinationPath = _pathDestination1;
        bool overrideExisting = true;
        FileSystemPathId sourcePathId = _fileSystemPathIdFixture.Create(sourcePath);
        FileSystemPathId destinationPathId = _fileSystemPathIdFixture.Create(destinationPath);

        _mockPlatformContext.PathStrategy.PathSeparator.Returns(_dirSeparator);
        _mockEnvironmentContext.FileProviderService.FileExists(sourcePathId).Returns(true);
        _mockEnvironmentContext.FileProviderService.MoveFile(sourcePathId, destinationPathId, overrideExisting)
            .Returns(Errors.Permission.UnauthorizedAccess);

        // Act
        Result<File> result = _sut.MoveFile(sourcePath, destinationPath, overrideExisting);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Permission.UnauthorizedAccess, result.FirstError);
    }

    [Fact]
    public void MoveFile_WhenRetrievingFileInfoFails_ShouldReturnInaccessibleFile()
    {
        // Arrange
        string sourcePath = _pathSourceFile;
        string destinationPath = _pathDestination1;
        bool overrideExisting = true;
        FileSystemPathId sourcePathId = _fileSystemPathIdFixture.Create(sourcePath);
        FileSystemPathId destinationPathId = _fileSystemPathIdFixture.Create(destinationPath);
        FileSystemPathId movedFilePathId = _fileSystemPathIdFixture.Create(_pathDestinationFile);

        _mockPlatformContext.PathStrategy.PathSeparator.Returns(_dirSeparator);
        _mockPlatformContext.PathStrategy.CombinePath(Arg.Any<FileSystemPathId>(), Arg.Any<string>())
            .Returns(movedFilePathId);
        _mockEnvironmentContext.FileProviderService.FileExists(sourcePathId).Returns(true);
        _mockEnvironmentContext.FileProviderService.MoveFile(sourcePathId, destinationPathId, overrideExisting)
            .Returns(movedFilePathId);

        _mockEnvironmentContext.FileProviderService.GetFileName(movedFilePathId).Returns(Errors.Permission.UnauthorizedAccess);
        _mockEnvironmentContext.FileProviderService.GetLastWriteTime(movedFilePathId).Returns(Errors.Permission.UnauthorizedAccess);
        _mockEnvironmentContext.FileProviderService.GetCreationTime(movedFilePathId).Returns(Errors.Permission.UnauthorizedAccess);
        _mockEnvironmentContext.FileProviderService.GetSize(movedFilePathId).Returns(Result.From((long?)null));

        // Act
        Result<File> result = _sut.MoveFile(sourcePath, destinationPath, overrideExisting);

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.Equal(FileSystemItemStatus.Inaccessible, result.Value.Status);
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
        Result<File> result = _sut.MoveFile(invalidSourcePath, destinationPath, overrideExisting);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.FileSystemManagement.InvalidPath, result.FirstError);
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
        Result<File> result = _sut.MoveFile(sourcePath, invalidDestinationPath, overrideExisting);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.FileSystemManagement.InvalidPath, result.FirstError);
    }

    [Fact]
    public void MoveFile_WithValidFileSystemPathIds_ShouldReturnMovedFile()
    {
        // Arrange
        FileSystemPathId sourcePathId = _fileSystemPathIdFixture.Create(_pathSourceFile);
        FileSystemPathId destinationPathId = _fileSystemPathIdFixture.Create(_pathDestination2);
        FileSystemPathId movedFilePathId = _fileSystemPathIdFixture.Create(_pathDestinationFile);
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
        Result<File> result = _sut.MoveFile(sourcePathId, destinationPathId, overrideExisting);

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.Equal("file.txt", result.Value.Name);
        Assert.Equal(FileSystemItemStatus.Accessible, result.Value.Status);
    }

    [Fact]
    public void MoveFileWithFileSystemPathIds_WhenSourceDoesNotExist_ShouldReturnError()
    {
        // Arrange
        FileSystemPathId sourcePathId = _fileSystemPathIdFixture.Create(
            s_isUnix ? "/Source/nonexistent.txt" : _pathSourceNonExistingFile
        );
        FileSystemPathId destinationPathId = _fileSystemPathIdFixture.Create(_pathDestination2);
        bool overrideExisting = true;

        _mockEnvironmentContext.FileProviderService.FileExists(sourcePathId).Returns(false);

        // Act
        Result<File> result = _sut.MoveFile(sourcePathId, destinationPathId, overrideExisting);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.FileSystemManagement.FileNotFound, result.FirstError);
    }

    [Fact]
    public void MoveFileWithFileSystemPathIds_WhenFileProviderServiceReturnsError_ShouldPropagateError()
    {
        // Arrange
        FileSystemPathId sourcePathId = _fileSystemPathIdFixture.Create(_pathSourceFile);
        FileSystemPathId destinationPathId = _fileSystemPathIdFixture.Create(_pathDestination1);
        bool overrideExisting = true;

        _mockEnvironmentContext.FileProviderService.FileExists(sourcePathId).Returns(true);
        _mockEnvironmentContext.FileProviderService.MoveFile(sourcePathId, destinationPathId, overrideExisting)
            .Returns(Errors.Permission.UnauthorizedAccess);

        // Act
        Result<File> result = _sut.MoveFile(sourcePathId, destinationPathId, overrideExisting);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Permission.UnauthorizedAccess, result.FirstError);
    }

    [Fact]
    public void MoveFileWithFileSystemPathIds_WhenRetrievingFileInfoFails_ShouldReturnInaccessibleFile()
    {
        // Arrange
        FileSystemPathId sourcePathId = _fileSystemPathIdFixture.Create(_pathSourceFile);
        FileSystemPathId destinationPathId = _fileSystemPathIdFixture.Create(_pathDestination1);
        FileSystemPathId movedFilePathId = _fileSystemPathIdFixture.Create(_pathDestinationFile);
        bool overrideExisting = true;

        _mockEnvironmentContext.FileProviderService.FileExists(sourcePathId).Returns(true);
        _mockEnvironmentContext.FileProviderService.MoveFile(sourcePathId, destinationPathId, overrideExisting)
            .Returns(movedFilePathId);

        _mockEnvironmentContext.FileProviderService.GetFileName(movedFilePathId).Returns(Errors.Permission.UnauthorizedAccess);
        _mockEnvironmentContext.FileProviderService.GetLastWriteTime(movedFilePathId).Returns(Errors.Permission.UnauthorizedAccess);
        _mockEnvironmentContext.FileProviderService.GetCreationTime(movedFilePathId).Returns(Errors.Permission.UnauthorizedAccess);
        _mockEnvironmentContext.FileProviderService.GetSize(movedFilePathId).Returns(Result.From((long?)null));

        // Act
        Result<File> result = _sut.MoveFile(sourcePathId, destinationPathId, overrideExisting);

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.Equal(FileSystemItemStatus.Inaccessible, result.Value.Status);
    }

    [Fact]
    public void MoveFileWithFileSystemPathIds_WhenSizeRetrievalFails_ShouldReturnInaccessibleFileWithZeroSize()
    {
        // Arrange
        FileSystemPathId sourcePathId = _fileSystemPathIdFixture.Create(_pathSourceFile);
        FileSystemPathId destinationPathId = _fileSystemPathIdFixture.Create(_pathDestination1);
        FileSystemPathId movedFilePathId = _fileSystemPathIdFixture.Create(_pathDestinationFile);
        bool overrideExisting = true;

        _mockEnvironmentContext.FileProviderService.FileExists(sourcePathId).Returns(true);
        _mockEnvironmentContext.FileProviderService.MoveFile(sourcePathId, destinationPathId, overrideExisting)
            .Returns(movedFilePathId);
        _mockEnvironmentContext.FileProviderService.GetFileName(movedFilePathId).Returns("file.txt");
        _mockEnvironmentContext.FileProviderService.GetLastWriteTime(movedFilePathId)
            .Returns(Optional<DateTime>.FromNullable(DateTime.Now));
        _mockEnvironmentContext.FileProviderService.GetCreationTime(movedFilePathId)
            .Returns(Optional<DateTime>.FromNullable(DateTime.Now.AddDays(-1)));
        _mockEnvironmentContext.FileProviderService.GetSize(movedFilePathId)
            .Returns(Errors.Permission.UnauthorizedAccess);

        // Act
        Result<File> result = _sut.MoveFile(sourcePathId, destinationPathId, overrideExisting);

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.Equal(FileSystemItemStatus.Inaccessible, result.Value.Status);
        Assert.Equal(0, result.Value.Size);
    }

    [Fact]
    public void MoveFile_WhenFileExistsReturnsError_ShouldPropagateError()
    {
        // Arrange
        string sourcePath = _pathSourceFile;
        string destinationPath = _pathDestination1;
        bool overrideExisting = true;
        FileSystemPathId sourcePathId = _fileSystemPathIdFixture.Create(sourcePath);

        _mockPlatformContext.PathStrategy.PathSeparator.Returns(_dirSeparator);
        _mockEnvironmentContext.FileProviderService.FileExists(sourcePathId)
            .Returns(Errors.Permission.UnauthorizedAccess);

        // Act
        Result<File> result = _sut.MoveFile(sourcePath, destinationPath, overrideExisting);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Permission.UnauthorizedAccess, result.FirstError);
    }

    [Fact]
    public void RenameFile_WithValidPathAndName_ShouldReturnRenamedFile()
    {
        // Arrange
        string path = _pathSourceOldFile;
        string newName = "newfile.txt";
        FileSystemPathId pathId = _fileSystemPathIdFixture.Create(path);
        FileSystemPathId newPathId = _fileSystemPathIdFixture.Create(_pathSourceNewFile);

        _mockPlatformContext.PathStrategy.PathSeparator.Returns(_dirSeparator);
        _mockPlatformContext.PathStrategy.CombinePath(Arg.Any<FileSystemPathId>(), newName)
            .Returns(Result.From(newPathId));
        _mockEnvironmentContext.FileProviderService.FileExists(newPathId).Returns(false);
        _mockEnvironmentContext.FileProviderService.RenameFile(pathId, newName)
            .Returns(Result.From(newPathId));
        _mockEnvironmentContext.FileProviderService.GetFileName(newPathId).Returns(Result.From(newName));
        _mockEnvironmentContext.FileProviderService.GetLastWriteTime(newPathId)
            .Returns(Result.From(Optional<DateTime>.FromNullable(DateTime.Now)));
        _mockEnvironmentContext.FileProviderService.GetCreationTime(newPathId)
            .Returns(Result.From(Optional<DateTime>.FromNullable(DateTime.Now.AddDays(-1))));
        _mockEnvironmentContext.FileProviderService.GetSize(newPathId).Returns(Result.From((long?)1024));

        // Act
        Result<File> result = _sut.RenameFile(path, newName);

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.Equal(newName, result.Value.Name);
        Assert.Equal(FileSystemItemStatus.Accessible, result.Value.Status);
    }

    [Fact]
    public void RenameFile_WhenNewNameAlreadyExists_ShouldReturnError()
    {
        // Arrange
        string path = _pathSourceOldFile;
        string newName = "existingfile.txt";
        FileSystemPathId newPathId = _fileSystemPathIdFixture.Create(_pathSourceExistingFile);

        _mockPlatformContext.PathStrategy.PathSeparator.Returns(_dirSeparator);
        _mockPlatformContext.PathStrategy.CombinePath(Arg.Any<FileSystemPathId>(), newName)
            .Returns(Result.From(newPathId));
        _mockEnvironmentContext.FileProviderService.FileExists(newPathId).Returns(true);

        // Act
        Result<File> result = _sut.RenameFile(path, newName);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.FileSystemManagement.FileAlreadyExists, result.FirstError);
    }

    [Fact]
    public void RenameFile_WhenFileProviderServiceReturnsError_ShouldPropagateError()
    {
        // Arrange
        string path = _pathSourceOldFile;
        string newName = "newfile.txt";
        FileSystemPathId pathId = _fileSystemPathIdFixture.Create(path);
        FileSystemPathId newPathId = _fileSystemPathIdFixture.Create(_pathSourceNewFile);

        _mockPlatformContext.PathStrategy.PathSeparator.Returns(_dirSeparator);
        _mockPlatformContext.PathStrategy.CombinePath(Arg.Any<FileSystemPathId>(), newName)
            .Returns(Result.From(newPathId));
        _mockEnvironmentContext.FileProviderService.FileExists(newPathId).Returns(false);
        _mockEnvironmentContext.FileProviderService.RenameFile(pathId, newName)
            .Returns(Errors.Permission.UnauthorizedAccess);

        // Act
        Result<File> result = _sut.RenameFile(path, newName);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Permission.UnauthorizedAccess, result.FirstError);
    }

    [Fact]
    public void RenameFile_WhenRetrievingFileInfoFails_ShouldReturnInaccessibleFile()
    {
        // Arrange
        string path = _pathSourceOldFile;
        string newName = "newfile.txt";
        FileSystemPathId pathId = _fileSystemPathIdFixture.Create(path);
        FileSystemPathId newPathId = _fileSystemPathIdFixture.Create(_pathSourceNewFile);

        _mockPlatformContext.PathStrategy.PathSeparator.Returns(_dirSeparator);
        _mockPlatformContext.PathStrategy.CombinePath(Arg.Any<FileSystemPathId>(), newName)
            .Returns(Result.From(newPathId));
        _mockEnvironmentContext.FileProviderService.FileExists(newPathId).Returns(false);
        _mockEnvironmentContext.FileProviderService.RenameFile(pathId, newName)
            .Returns(Result.From(newPathId));

        _mockEnvironmentContext.FileProviderService.GetFileName(newPathId).Returns(Errors.Permission.UnauthorizedAccess);
        _mockEnvironmentContext.FileProviderService.GetLastWriteTime(newPathId).Returns(Errors.Permission.UnauthorizedAccess);
        _mockEnvironmentContext.FileProviderService.GetCreationTime(newPathId).Returns(Errors.Permission.UnauthorizedAccess);
        _mockEnvironmentContext.FileProviderService.GetSize(newPathId).Returns(Result.From((long?)null));

        // Act
        Result<File> result = _sut.RenameFile(path, newName);

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.Equal(FileSystemItemStatus.Inaccessible, result.Value.Status);
    }

    [Fact]
    public void RenameFile_WithInvalidPath_ShouldReturnError()
    {
        // Arrange
        string invalidPath = string.Empty;
        string newName = "newfile.txt";

        _mockPlatformContext.PathStrategy.PathSeparator.Returns(_dirSeparator);

        // Act
        Result<File> result = _sut.RenameFile(invalidPath, newName);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.FileSystemManagement.InvalidPath, result.FirstError);
    }

    [Fact]
    public void RenameFileWithValidFileSystemPathIdAndName_ShouldReturnRenamedFile()
    {
        // Arrange
        FileSystemPathId pathId = _fileSystemPathIdFixture.Create(_pathSourceOldFile);
        string newName = "newfile.txt";
        FileSystemPathId newPathId = _fileSystemPathIdFixture.Create(_pathSourceNewFile);

        _mockPlatformContext.PathStrategy.CombinePath(Arg.Any<FileSystemPathId>(), newName)
            .Returns(Result.From(newPathId));
        _mockEnvironmentContext.FileProviderService.FileExists(newPathId).Returns(false);
        _mockEnvironmentContext.FileProviderService.RenameFile(pathId, newName)
            .Returns(Result.From(newPathId));
        _mockEnvironmentContext.FileProviderService.GetFileName(newPathId).Returns(Result.From(newName));
        _mockEnvironmentContext.FileProviderService.GetLastWriteTime(newPathId)
            .Returns(Result.From(Optional<DateTime>.FromNullable(DateTime.Now)));
        _mockEnvironmentContext.FileProviderService.GetCreationTime(newPathId)
            .Returns(Result.From(Optional<DateTime>.FromNullable(DateTime.Now.AddDays(-1))));
        _mockEnvironmentContext.FileProviderService.GetSize(newPathId).Returns(Result.From((long?)1024));

        // Act
        Result<File> result = _sut.RenameFile(pathId, newName);

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.Equal(newName, result.Value.Name);
        Assert.Equal(FileSystemItemStatus.Accessible, result.Value.Status);
    }

    [Fact]
    public void RenameFileWithFileSystemPathId_WhenNewNameAlreadyExists_ShouldReturnError()
    {
        // Arrange
        FileSystemPathId pathId = _fileSystemPathIdFixture.Create(_pathSourceOldFile);
        string newName = "existingfile.txt";
        FileSystemPathId newPathId = _fileSystemPathIdFixture.Create(_pathSourceExistingFile);

        _mockPlatformContext.PathStrategy.CombinePath(Arg.Any<FileSystemPathId>(), newName)
            .Returns(Result.From(newPathId));
        _mockEnvironmentContext.FileProviderService.FileExists(newPathId).Returns(true);

        // Act
        Result<File> result = _sut.RenameFile(pathId, newName);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.FileSystemManagement.FileAlreadyExists, result.FirstError);
    }

    [Fact]
    public void RenameFileWithFileSystemPathId_WhenFileProviderServiceReturnsError_ShouldPropagateError()
    {
        // Arrange
        FileSystemPathId pathId = _fileSystemPathIdFixture.Create(_pathSourceOldFile);
        string newName = "newfile.txt";
        FileSystemPathId newPathId = _fileSystemPathIdFixture.Create(_pathSourceNewFile);

        _mockPlatformContext.PathStrategy.CombinePath(Arg.Any<FileSystemPathId>(), newName)
            .Returns(Result.From(newPathId));
        _mockEnvironmentContext.FileProviderService.FileExists(newPathId).Returns(false);
        _mockEnvironmentContext.FileProviderService.RenameFile(pathId, newName)
            .Returns(Errors.Permission.UnauthorizedAccess);

        // Act
        Result<File> result = _sut.RenameFile(pathId, newName);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Permission.UnauthorizedAccess, result.FirstError);
    }

    [Fact]
    public void RenameFileWithFileSystemPathId_WhenRetrievingFileInfoFails_ShouldReturnInaccessibleFile()
    {
        // Arrange
        FileSystemPathId pathId = _fileSystemPathIdFixture.Create(_pathSourceOldFile);
        string newName = "newfile.txt";
        FileSystemPathId newPathId = _fileSystemPathIdFixture.Create(_pathSourceNewFile);

        _mockPlatformContext.PathStrategy.CombinePath(Arg.Any<FileSystemPathId>(), newName)
            .Returns(Result.From(newPathId));
        _mockEnvironmentContext.FileProviderService.FileExists(newPathId).Returns(false);
        _mockEnvironmentContext.FileProviderService.RenameFile(pathId, newName)
            .Returns(Result.From(newPathId));

        _mockEnvironmentContext.FileProviderService.GetFileName(newPathId).Returns(Errors.Permission.UnauthorizedAccess);
        _mockEnvironmentContext.FileProviderService.GetLastWriteTime(newPathId).Returns(Errors.Permission.UnauthorizedAccess);
        _mockEnvironmentContext.FileProviderService.GetCreationTime(newPathId).Returns(Errors.Permission.UnauthorizedAccess);
        _mockEnvironmentContext.FileProviderService.GetSize(newPathId).Returns(Result.From((long?)null));

        // Act
        Result<File> result = _sut.RenameFile(pathId, newName);

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.Equal(FileSystemItemStatus.Inaccessible, result.Value.Status);
    }

    [Fact]
    public void RenameFileWithFileSystemPathId_WhenSizeRetrievalFails_ShouldReturnInaccessibleFileWithZeroSize()
    {
        // Arrange
        FileSystemPathId pathId = _fileSystemPathIdFixture.Create(_pathSourceOldFile);
        string newName = "newfile.txt";
        FileSystemPathId newPathId = _fileSystemPathIdFixture.Create(_pathSourceNewFile);

        _mockPlatformContext.PathStrategy.CombinePath(Arg.Any<FileSystemPathId>(), newName)
            .Returns(Result.From(newPathId));
        _mockEnvironmentContext.FileProviderService.FileExists(newPathId).Returns(false);
        _mockEnvironmentContext.FileProviderService.RenameFile(pathId, newName)
            .Returns(Result.From(newPathId));
        _mockEnvironmentContext.FileProviderService.GetFileName(newPathId).Returns(Result.From(newName));
        _mockEnvironmentContext.FileProviderService.GetLastWriteTime(newPathId)
            .Returns(Result.From(Optional<DateTime>.FromNullable(DateTime.Now)));
        _mockEnvironmentContext.FileProviderService.GetCreationTime(newPathId)
            .Returns(Result.From(Optional<DateTime>.FromNullable(DateTime.Now.AddDays(-1))));
        _mockEnvironmentContext.FileProviderService.GetSize(newPathId)
            .Returns(Errors.Permission.UnauthorizedAccess);

        // Act
        Result<File> result = _sut.RenameFile(pathId, newName);

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.Equal(FileSystemItemStatus.Inaccessible, result.Value.Status);
        Assert.Equal(0, result.Value.Size);
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
        Result<File> result = _sut.RenameFile(path, newName);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.FileSystemManagement.InvalidPath, result.FirstError);
    }

    [Fact]
    public void RenameFile_WhenFileExistsReturnsError_ShouldPropagateError()
    {
        // Arrange
        string path = _pathSourceOldFile;
        string newName = "newfile.txt";
        FileSystemPathId newPathId = _fileSystemPathIdFixture.Create(_pathSourceNewFile);

        _mockPlatformContext.PathStrategy.PathSeparator.Returns(_dirSeparator);
        _mockPlatformContext.PathStrategy.CombinePath(Arg.Any<FileSystemPathId>(), newName)
            .Returns(Result.From(newPathId));
        _mockEnvironmentContext.FileProviderService.FileExists(newPathId)
            .Returns(Errors.Permission.UnauthorizedAccess);

        // Act
        Result<File> result = _sut.RenameFile(path, newName);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Permission.UnauthorizedAccess, result.FirstError);
    }

    [Fact]
    public void RenameFileWithFileSystemPathId_WhenCombinePathReturnsError_ShouldPropagateError()
    {
        // Arrange
        FileSystemPathId pathId = _fileSystemPathIdFixture.Create(_pathSourceOldFile);
        string newName = "newfile.txt";

        _mockPlatformContext.PathStrategy.CombinePath(Arg.Any<FileSystemPathId>(), newName)
            .Returns(Errors.FileSystemManagement.InvalidPath);

        // Act
        Result<File> result = _sut.RenameFile(pathId, newName);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.FileSystemManagement.InvalidPath, result.FirstError);
    }

    [Fact]
    public void RenameFileWithFileSystemPathId_WhenFileExistsReturnsError_ShouldPropagateError()
    {
        // Arrange
        FileSystemPathId pathId = _fileSystemPathIdFixture.Create(_pathSourceOldFile);
        string newName = "newfile.txt";
        FileSystemPathId newPathId = _fileSystemPathIdFixture.Create(_pathSourceNewFile);

        _mockPlatformContext.PathStrategy.CombinePath(Arg.Any<FileSystemPathId>(), newName)
            .Returns(Result.From(newPathId));
        _mockEnvironmentContext.FileProviderService.FileExists(newPathId)
            .Returns(Errors.Permission.UnauthorizedAccess);

        // Act
        Result<File> result = _sut.RenameFile(pathId, newName);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Permission.UnauthorizedAccess, result.FirstError);
    }

    [Fact]
    public void DeleteFile_WithValidPath_ShouldReturnDeleted()
    {
        // Arrange
        string path = _pathSourceFile;
        FileSystemPathId pathId = _fileSystemPathIdFixture.Create(path);

        _mockEnvironmentContext.FileProviderService.DeleteFile(pathId)
            .Returns(Result.Deleted);

        // Act
        Result<Deleted> result = _sut.DeleteFile(path);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Deleted, result.Value);
    }

    [Fact]
    public void DeleteFile_WithInvalidPath_ShouldReturnError()
    {
        // Arrange
        string invalidPath = string.Empty;

        // Act
        Result<Deleted> result = _sut.DeleteFile(invalidPath);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.FileSystemManagement.InvalidPath, result.FirstError);
    }

    [Fact]
    public void DeleteFile_WhenFileProviderServiceReturnsError_ShouldPropagateError()
    {
        // Arrange
        string path = _pathSourceFile;
        FileSystemPathId pathId = _fileSystemPathIdFixture.Create(path);

        _mockEnvironmentContext.FileProviderService.DeleteFile(pathId)
            .Returns(Errors.Permission.UnauthorizedAccess);

        // Act
        Result<Deleted> result = _sut.DeleteFile(path);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Permission.UnauthorizedAccess, result.FirstError);
    }

    [Fact]
    public void DeleteFile_WithValidFileSystemPathId_ShouldReturnDeleted()
    {
        // Arrange
        FileSystemPathId pathId = _fileSystemPathIdFixture.Create(_pathSourceFile);

        _mockEnvironmentContext.FileProviderService.DeleteFile(pathId)
            .Returns(Result.Deleted);

        // Act
        Result<Deleted> result = _sut.DeleteFile(pathId);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Deleted, result.Value);
    }

    [Fact]
    public void DeleteFileWithFileSystemPathId_WhenFileProviderServiceReturnsError_ShouldPropagateError()
    {
        // Arrange
        FileSystemPathId pathId = _fileSystemPathIdFixture.Create(_pathSourceFile);

        _mockEnvironmentContext.FileProviderService.DeleteFile(pathId)
            .Returns(Errors.Permission.UnauthorizedAccess);

        // Act
        Result<Deleted> result = _sut.DeleteFile(pathId);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Permission.UnauthorizedAccess, result.FirstError);
    }

    [Fact]
    public void ReadFile_WithFileSystemPathId_ShouldReturnTrue()
    {
        // Arrange
        FileSystemPathId pathId = _fileSystemPathIdFixture.Create(_pathSourceFile);

        // Act
        Result<bool> result = _sut.ReadFile(pathId);

        // Assert
        Assert.False(result.IsFailure);
        Assert.True(result.Value);
    }
}
