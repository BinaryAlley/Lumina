#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Domain.Common.Enums.FileSystem;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Services;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Strategies.Environment;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Strategies.Platform;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Entities;
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
/// Contains unit tests for the <see cref="DirectoryService"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class DirectoryServiceTests
{
    private readonly IEnvironmentContext _mockEnvironmentContext;
    private readonly IPlatformContextManager _mockPlatformContextManager;
    private readonly IPlatformContext _mockPlatformContext;
    private readonly DirectoryService _sut;
    private readonly FileSystemPathIdFixture _fileSystemPathIdFixture;
    private readonly DirectoryFixture _directoryFixture;
    private static readonly bool s_isUnix = RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
    private readonly string _pathDirTest = s_isUnix ? "/TestDir" : @"C:\TestDir";
    private readonly string _pathDirTestSubDir1 = s_isUnix ? "/TestDir/Sub1" : @"C:\TestDir\Sub1";
    private readonly string _pathDirTestSubDir2 = s_isUnix ? "/TestDir/Sub2" : @"C:\TestDir\Sub2";

    /// <summary>
    /// Initializes a new instance of the <see cref="DirectoryServiceTests"/> class.
    /// </summary>
    public DirectoryServiceTests()
    {
        _mockEnvironmentContext = Substitute.For<IEnvironmentContext>();
        _mockPlatformContextManager = Substitute.For<IPlatformContextManager>();
        _mockPlatformContext = Substitute.For<IPlatformContext>();
        _mockPlatformContextManager.GetCurrentContext().Returns(_mockPlatformContext);
        _sut = new DirectoryService(_mockEnvironmentContext, _mockPlatformContextManager);
        _fileSystemPathIdFixture = new FileSystemPathIdFixture();
        _directoryFixture = new DirectoryFixture();
    }

    [Fact]
    public void GetSubdirectories_WithValidPath_ShouldReturnListOfDirectories()
    {
        // Arrange
        string path = _pathDirTest;
        bool includeHiddenElements = false;
        FileSystemPathId pathId = _fileSystemPathIdFixture.CreateFileSystemPathId(path);
        FileSystemPathId[] subPaths =
        [
            _fileSystemPathIdFixture.CreateFileSystemPathId(_pathDirTestSubDir1),
            _fileSystemPathIdFixture.CreateFileSystemPathId(_pathDirTestSubDir2)
        ];

        _mockEnvironmentContext.DirectoryProviderService.GetSubdirectoryPaths(pathId, includeHiddenElements)
            .Returns(ErrorOrFactory.From(subPaths.AsEnumerable()));

        foreach (FileSystemPathId subPath in subPaths)
        {
            _mockEnvironmentContext.DirectoryProviderService.GetFileName(subPath)
                .Returns(ErrorOrFactory.From(System.IO.Path.GetFileName(subPath.Path)));
            _mockEnvironmentContext.DirectoryProviderService.GetLastWriteTime(subPath)
                .Returns(ErrorOrFactory.From(Optional<DateTime>.FromNullable(DateTime.Now)));
            _mockEnvironmentContext.DirectoryProviderService.GetCreationTime(subPath)
                .Returns(ErrorOrFactory.From(Optional<DateTime>.FromNullable(DateTime.Now.AddDays(-1))));
        }

        // Act
        ErrorOr<IEnumerable<Directory>> result = _sut.GetSubdirectories(path, includeHiddenElements);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(2, result.Value.Count());
        Assert.Equal(["Sub1", "Sub2"], result.Value.Select(d => d.Name));
    }

    [Fact]
    public void GetSubdirectories_WithInvalidPath_ShouldReturnError()
    {
        // Arrange
        string invalidPath = string.Empty;
        bool includeHiddenElements = false;

        // Act
        ErrorOr<IEnumerable<Directory>> result = _sut.GetSubdirectories(invalidPath, includeHiddenElements);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(Errors.FileSystemManagement.InvalidPath, result.FirstError);
    }

    [Fact]
    public void GetSubdirectories_WhenDirectoryProviderServiceReturnsError_ShouldPropagateError()
    {
        // Arrange
        string path = _pathDirTest;
        bool includeHiddenElements = false;
        FileSystemPathId pathId = _fileSystemPathIdFixture.CreateFileSystemPathId(path);

        _mockEnvironmentContext.DirectoryProviderService.GetSubdirectoryPaths(pathId, includeHiddenElements)
            .Returns(Errors.Permission.UnauthorizedAccess);

        // Act
        ErrorOr<IEnumerable<Directory>> result = _sut.GetSubdirectories(path, includeHiddenElements);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(Errors.Permission.UnauthorizedAccess, result.FirstError);
    }

    [Fact]
    public void GetSubdirectories_WhenSubdirectoryDetailsAreInaccessible_ShouldReturnInaccessibleDirectory()
    {
        // Arrange
        string path = _pathDirTest;
        bool includeHiddenElements = false;
        FileSystemPathId pathId = _fileSystemPathIdFixture.CreateFileSystemPathId(path);
        FileSystemPathId subPath = _fileSystemPathIdFixture.CreateFileSystemPathId(
            s_isUnix ? "/TestDir/InaccessibleSub" : @"C:\TestDir\InaccessibleSub"
        );

        _mockEnvironmentContext.DirectoryProviderService.GetSubdirectoryPaths(pathId, includeHiddenElements)
            .Returns(ErrorOrFactory.From(new[] { subPath }.AsEnumerable()));

        _mockEnvironmentContext.DirectoryProviderService.GetFileName(subPath)
            .Returns(Errors.Permission.UnauthorizedAccess);
        _mockEnvironmentContext.DirectoryProviderService.GetLastWriteTime(subPath)
            .Returns(Errors.Permission.UnauthorizedAccess);
        _mockEnvironmentContext.DirectoryProviderService.GetCreationTime(subPath)
            .Returns(Errors.Permission.UnauthorizedAccess);

        // Act
        ErrorOr<IEnumerable<Directory>> result = _sut.GetSubdirectories(path, includeHiddenElements);

        // Assert
        Assert.False(result.IsError);
        Assert.Single(result.Value);
        Assert.Equal(FileSystemItemStatus.Inaccessible, result.Value.First().Status);
    }

    [Fact]
    public void GetSubdirectoriesOverload_WithValidDirectory_ShouldReturnListOfDirectories()
    {
        // Arrange
        Directory parentDirectory = _directoryFixture.CreateDirectory();
        bool includeHiddenElements = false;
        FileSystemPathId[] subPaths =
        [
            _fileSystemPathIdFixture.CreateFileSystemPathId(_pathDirTestSubDir1),
        _fileSystemPathIdFixture.CreateFileSystemPathId(_pathDirTestSubDir2)
        ];

        _mockEnvironmentContext.DirectoryProviderService.GetSubdirectoryPaths(parentDirectory.Id, includeHiddenElements)
            .Returns(ErrorOrFactory.From(subPaths.AsEnumerable()));

        foreach (FileSystemPathId subPath in subPaths)
        {
            _mockEnvironmentContext.DirectoryProviderService.GetFileName(subPath)
                .Returns(ErrorOrFactory.From(System.IO.Path.GetFileName(subPath.Path)));
            _mockEnvironmentContext.DirectoryProviderService.GetLastWriteTime(subPath)
                .Returns(ErrorOrFactory.From(Optional<DateTime>.FromNullable(DateTime.Now)));
            _mockEnvironmentContext.DirectoryProviderService.GetCreationTime(subPath)
                .Returns(ErrorOrFactory.From(Optional<DateTime>.FromNullable(DateTime.Now.AddDays(-1))));
        }

        // Act
        ErrorOr<IEnumerable<Directory>> result = _sut.GetSubdirectories(parentDirectory, includeHiddenElements);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(2, result.Value.Count());
        Assert.Equal(["Sub1", "Sub2"], result.Value.Select(d => d.Name));
    }

    [Fact]
    public void GetSubdirectoriesOverload_WhenDirectoryProviderServiceReturnsError_ShouldPropagateError()
    {
        // Arrange
        Directory parentDirectory = _directoryFixture.CreateDirectory();
        bool includeHiddenElements = false;

        _mockEnvironmentContext.DirectoryProviderService.GetSubdirectoryPaths(parentDirectory.Id, includeHiddenElements)
            .Returns(Errors.Permission.UnauthorizedAccess);

        // Act
        ErrorOr<IEnumerable<Directory>> result = _sut.GetSubdirectories(parentDirectory, includeHiddenElements);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(Errors.Permission.UnauthorizedAccess, result.FirstError);
    }

    [Fact]
    public void GetSubdirectoriesOverload_WhenSubdirectoryDetailsAreInaccessible_ShouldReturnInaccessibleDirectory()
    {
        // Arrange
        Directory parentDirectory = _directoryFixture.CreateDirectory();
        bool includeHiddenElements = false;
        FileSystemPathId subPath = _fileSystemPathIdFixture.CreateFileSystemPathId(
            s_isUnix ? "/TestDir/InaccessibleSub" : @"C:\TestDir\InaccessibleSub"
        );

        _mockEnvironmentContext.DirectoryProviderService.GetSubdirectoryPaths(parentDirectory.Id, includeHiddenElements)
            .Returns(ErrorOrFactory.From(new[] { subPath }.AsEnumerable()));

        _mockEnvironmentContext.DirectoryProviderService.GetFileName(subPath)
            .Returns(Errors.Permission.UnauthorizedAccess);
        _mockEnvironmentContext.DirectoryProviderService.GetLastWriteTime(subPath)
            .Returns(Errors.Permission.UnauthorizedAccess);
        _mockEnvironmentContext.DirectoryProviderService.GetCreationTime(subPath)
            .Returns(Errors.Permission.UnauthorizedAccess);

        // Act
        ErrorOr<IEnumerable<Directory>> result = _sut.GetSubdirectories(parentDirectory, includeHiddenElements);

        // Assert
        Assert.False(result.IsError);
        Assert.Single(result.Value);
        Assert.Equal(FileSystemItemStatus.Inaccessible, result.Value.First().Status);
    }

    [Fact]
    public void GetSubdirectories_WithValidFileSystemPathId_ShouldReturnListOfDirectories()
    {
        // Arrange
        FileSystemPathId pathId = _fileSystemPathIdFixture.CreateFileSystemPathId(_pathDirTest);
        bool includeHiddenElements = false;
        FileSystemPathId[] subPaths =
        [
            _fileSystemPathIdFixture.CreateFileSystemPathId(_pathDirTestSubDir1),
        _fileSystemPathIdFixture.CreateFileSystemPathId(_pathDirTestSubDir2)
        ];

        _mockEnvironmentContext.DirectoryProviderService.GetSubdirectoryPaths(pathId, includeHiddenElements)
            .Returns(ErrorOrFactory.From(subPaths.AsEnumerable()));

        foreach (FileSystemPathId subPath in subPaths)
        {
            _mockEnvironmentContext.DirectoryProviderService.GetFileName(subPath)
                .Returns(ErrorOrFactory.From(System.IO.Path.GetFileName(subPath.Path)));
            _mockEnvironmentContext.DirectoryProviderService.GetLastWriteTime(subPath)
                .Returns(ErrorOrFactory.From(Optional<DateTime>.FromNullable(DateTime.Now)));
            _mockEnvironmentContext.DirectoryProviderService.GetCreationTime(subPath)
                .Returns(ErrorOrFactory.From(Optional<DateTime>.FromNullable(DateTime.Now.AddDays(-1))));
        }

        // Act
        ErrorOr<IEnumerable<Directory>> result = _sut.GetSubdirectories(pathId, includeHiddenElements);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(2, result.Value.Count());
        Assert.Equal(["Sub1", "Sub2"], result.Value.Select(d => d.Name));
    }

    [Fact]
    public void GetSubdirectoriesWithFileSystemPathId_WhenDirectoryProviderServiceReturnsError_ShouldPropagateError()
    {
        // Arrange
        FileSystemPathId pathId = _fileSystemPathIdFixture.CreateFileSystemPathId(_pathDirTest);
        bool includeHiddenElements = false;

        _mockEnvironmentContext.DirectoryProviderService.GetSubdirectoryPaths(pathId, includeHiddenElements)
            .Returns(Errors.Permission.UnauthorizedAccess);

        // Act
        ErrorOr<IEnumerable<Directory>> result = _sut.GetSubdirectories(pathId, includeHiddenElements);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(Errors.Permission.UnauthorizedAccess, result.FirstError);
    }

    [Fact]
    public void GetSubdirectoriesWithFileSystemPathId_WhenSubdirectoryDetailsAreInaccessible_ShouldReturnInaccessibleDirectory()
    {
        // Arrange
        FileSystemPathId pathId = _fileSystemPathIdFixture.CreateFileSystemPathId(_pathDirTest);
        bool includeHiddenElements = false;
        FileSystemPathId subPath = _fileSystemPathIdFixture.CreateFileSystemPathId(
            s_isUnix ? "/TestDir/InaccessibleSub" : @"C:\TestDir\InaccessibleSub"
        );

        _mockEnvironmentContext.DirectoryProviderService.GetSubdirectoryPaths(pathId, includeHiddenElements)
            .Returns(ErrorOrFactory.From(new[] { subPath }.AsEnumerable()));

        _mockEnvironmentContext.DirectoryProviderService.GetFileName(subPath)
            .Returns(Errors.Permission.UnauthorizedAccess);
        _mockEnvironmentContext.DirectoryProviderService.GetLastWriteTime(subPath)
            .Returns(Errors.Permission.UnauthorizedAccess);
        _mockEnvironmentContext.DirectoryProviderService.GetCreationTime(subPath)
            .Returns(Errors.Permission.UnauthorizedAccess);

        // Act
        ErrorOr<IEnumerable<Directory>> result = _sut.GetSubdirectories(pathId, includeHiddenElements);

        // Assert
        Assert.False(result.IsError);
        Assert.Single(result.Value);
        Assert.Equal(FileSystemItemStatus.Inaccessible, result.Value.First().Status);
    }

    [Fact]
    public void CreateDirectory_WithValidStringPath_ShouldReturnNewDirectory()
    {
        // Arrange
        string parentPath = _pathDirTest;
        string newDirName = "NewDirectory";
        FileSystemPathId parentPathId = _fileSystemPathIdFixture.CreateFileSystemPathId(parentPath);
        FileSystemPathId newDirPathId = _fileSystemPathIdFixture.CreateFileSystemPathId(System.IO.Path.Combine(parentPath, newDirName));

        _mockEnvironmentContext.DirectoryProviderService.CreateDirectory(parentPathId, newDirName)
            .Returns(ErrorOrFactory.From(newDirPathId));
        _mockEnvironmentContext.DirectoryProviderService.GetFileName(newDirPathId)
            .Returns(ErrorOrFactory.From(newDirName));
        _mockEnvironmentContext.DirectoryProviderService.GetLastWriteTime(newDirPathId)
            .Returns(ErrorOrFactory.From(Optional<DateTime>.FromNullable(DateTime.Now)));
        _mockEnvironmentContext.DirectoryProviderService.GetCreationTime(newDirPathId)
            .Returns(ErrorOrFactory.From(Optional<DateTime>.FromNullable(DateTime.Now)));

        // Act
        ErrorOr<Directory> result = _sut.CreateDirectory(parentPath, newDirName);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(newDirPathId, result.Value.Id);
    }

    [Fact]
    public void CreateDirectory_WithInvalidStringPath_ShouldReturnError()
    {
        // Arrange
        string invalidPath = string.Empty;
        string newDirName = "NewDirectory";

        // Act
        ErrorOr<Directory> result = _sut.CreateDirectory(invalidPath, newDirName);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(Errors.FileSystemManagement.InvalidPath, result.FirstError);
    }

    [Fact]
    public void CreateDirectory_WithFileSystemPathId_ShouldReturnNewDirectory()
    {
        // Arrange
        FileSystemPathId parentPathId = _fileSystemPathIdFixture.CreateFileSystemPathId(_pathDirTest);
        string newDirName = "NewDirectory";
        FileSystemPathId newDirPathId = _fileSystemPathIdFixture.CreateFileSystemPathId(System.IO.Path.Combine(parentPathId.Path, newDirName));

        _mockEnvironmentContext.DirectoryProviderService.CreateDirectory(parentPathId, newDirName)
            .Returns(ErrorOrFactory.From(newDirPathId));
        _mockEnvironmentContext.DirectoryProviderService.GetFileName(newDirPathId)
            .Returns(ErrorOrFactory.From(newDirName));
        _mockEnvironmentContext.DirectoryProviderService.GetLastWriteTime(newDirPathId)
            .Returns(ErrorOrFactory.From(Optional<DateTime>.FromNullable(DateTime.Now)));
        _mockEnvironmentContext.DirectoryProviderService.GetCreationTime(newDirPathId)
            .Returns(ErrorOrFactory.From(Optional<DateTime>.FromNullable(DateTime.Now)));

        // Act
        ErrorOr<Directory> result = _sut.CreateDirectory(parentPathId, newDirName);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(newDirPathId, result.Value.Id);
    }

    [Fact]
    public void CreateDirectory_WhenDirectoryProviderServiceReturnsError_ShouldPropagateError()
    {
        // Arrange
        FileSystemPathId parentPathId = _fileSystemPathIdFixture.CreateFileSystemPathId(_pathDirTest);
        string newDirName = "NewDirectory";

        _mockEnvironmentContext.DirectoryProviderService.CreateDirectory(parentPathId, newDirName)
            .Returns(Errors.Permission.UnauthorizedAccess);

        // Act
        ErrorOr<Directory> result = _sut.CreateDirectory(parentPathId, newDirName);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(Errors.Permission.UnauthorizedAccess, result.FirstError);
    }

    [Fact]
    public void CreateDirectory_WhenDirectoryDetailsAreInaccessible_ShouldReturnInaccessibleDirectory()
    {
        // Arrange
        FileSystemPathId parentPathId = _fileSystemPathIdFixture.CreateFileSystemPathId(_pathDirTest);
        string newDirName = "NewDirectory";
        FileSystemPathId newDirPathId = _fileSystemPathIdFixture.CreateFileSystemPathId(System.IO.Path.Combine(parentPathId.Path, newDirName));

        _mockEnvironmentContext.DirectoryProviderService.CreateDirectory(parentPathId, newDirName)
            .Returns(ErrorOrFactory.From(newDirPathId));

        _mockEnvironmentContext.DirectoryProviderService.GetFileName(newDirPathId)
            .Returns(Errors.Permission.UnauthorizedAccess);
        _mockEnvironmentContext.DirectoryProviderService.GetLastWriteTime(newDirPathId)
            .Returns(Errors.Permission.UnauthorizedAccess);
        _mockEnvironmentContext.DirectoryProviderService.GetCreationTime(newDirPathId)
            .Returns(Errors.Permission.UnauthorizedAccess);

        // Act
        ErrorOr<Directory> result = _sut.CreateDirectory(parentPathId, newDirName);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(FileSystemItemStatus.Inaccessible, result.Value.Status);
        Assert.Equal(newDirPathId, result.Value.Id);
        Assert.False(result.Value.DateCreated.HasValue);
        Assert.False(result.Value.DateModified.HasValue);
    }

    [Fact]
    public void CreateDirectory_WhenDirectoryExistsReturnsError_ShouldPropagateError()
    {
        // Arrange
        FileSystemPathId parentPathId = _fileSystemPathIdFixture.CreateFileSystemPathId(_pathDirTest);
        string newDirName = "NewDirectory";
        FileSystemPathId newDirPathId = _fileSystemPathIdFixture.CreateFileSystemPathId(System.IO.Path.Combine(parentPathId.Path, newDirName));

        _mockPlatformContext.PathStrategy.CombinePath(parentPathId, newDirName).Returns(ErrorOrFactory.From(newDirPathId));
        _mockEnvironmentContext.DirectoryProviderService.DirectoryExists(newDirPathId)
            .Returns(Errors.Permission.UnauthorizedAccess);

        // Act
        ErrorOr<Directory> result = _sut.CreateDirectory(parentPathId, newDirName);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(Errors.Permission.UnauthorizedAccess, result.FirstError);
    }

    [Fact]
    public void CreateDirectory_WhenDirectoryAlreadyExists_ShouldReturnDirectoryAlreadyExistsError()
    {
        // Arrange
        FileSystemPathId parentPathId = _fileSystemPathIdFixture.CreateFileSystemPathId(_pathDirTest);
        string newDirName = "ExistingDirectory";
        FileSystemPathId newDirPathId = _fileSystemPathIdFixture.CreateFileSystemPathId(System.IO.Path.Combine(parentPathId.Path, newDirName));
        _mockPlatformContext.PathStrategy.CombinePath(parentPathId, newDirName).Returns(ErrorOrFactory.From(newDirPathId));

        _mockEnvironmentContext.DirectoryProviderService.DirectoryExists(newDirPathId).Returns(true);

        // Act
        ErrorOr<Directory> result = _sut.CreateDirectory(parentPathId, newDirName);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(Errors.FileSystemManagement.DirectoryAlreadyExists, result.FirstError);
    }

    [Fact]
    public void CreateDirectory_WhenCombinePathReturnsError_ShouldReturnError()
    {
        // Arrange
        FileSystemPathId parentPathId = _fileSystemPathIdFixture.CreateFileSystemPathId(_pathDirTest);
        string newDirName = "NewDirectory";

        _mockPlatformContext.PathStrategy.CombinePath(parentPathId, newDirName)
            .Returns(Errors.FileSystemManagement.InvalidPath);

        // Act
        ErrorOr<Directory> result = _sut.CreateDirectory(parentPathId, newDirName);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(Errors.FileSystemManagement.InvalidPath, result.FirstError);

        // Verify that DirectoryExists was not called
        _mockEnvironmentContext.DirectoryProviderService.DidNotReceive().DirectoryExists(Arg.Any<FileSystemPathId>());
        _mockEnvironmentContext.DirectoryProviderService.DidNotReceive().CreateDirectory(Arg.Any<FileSystemPathId>(), Arg.Any<string>());
    }

    [Fact]
    public void RenameDirectory_WithValidStringPath_ShouldReturnRenamedDirectory()
    {
        // Arrange
        string oldPath = s_isUnix ? "/TestDir/OldName" : @"C:\TestDir\OldName";
        string newName = "NewName";
        FileSystemPathId oldPathId = _fileSystemPathIdFixture.CreateFileSystemPathId(oldPath);
        FileSystemPathId newPathId = _fileSystemPathIdFixture.CreateFileSystemPathId(
            s_isUnix ? "/TestDir/NewName" : @"C:\TestDir\NewName"
        );

        _mockEnvironmentContext.DirectoryProviderService.RenameDirectory(oldPathId, newName)
            .Returns(ErrorOrFactory.From(newPathId));
        _mockEnvironmentContext.DirectoryProviderService.GetFileName(newPathId)
            .Returns(ErrorOrFactory.From(newName));
        _mockEnvironmentContext.DirectoryProviderService.GetLastWriteTime(newPathId)
            .Returns(ErrorOrFactory.From(Optional<DateTime>.FromNullable(DateTime.Now)));
        _mockEnvironmentContext.DirectoryProviderService.GetCreationTime(newPathId)
            .Returns(ErrorOrFactory.From(Optional<DateTime>.FromNullable(DateTime.Now)));

        // Act
        ErrorOr<Directory> result = _sut.RenameDirectory(oldPath, newName);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(newPathId, result.Value.Id);
        Assert.Equal(newName, result.Value.Name);
    }

    [Fact]
    public void RenameDirectory_WithInvalidStringPath_ShouldReturnError()
    {
        // Arrange
        string invalidPath = string.Empty;
        string newName = "NewName";

        // Act
        ErrorOr<Directory> result = _sut.RenameDirectory(invalidPath, newName);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(Errors.FileSystemManagement.InvalidPath, result.FirstError);
    }

    [Fact]
    public void RenameDirectory_WithFileSystemPathId_ShouldReturnRenamedDirectory()
    {
        // Arrange
        FileSystemPathId oldPathId = _fileSystemPathIdFixture.CreateFileSystemPathId(
            s_isUnix ? "/TestDir/OldName" : @"C:\TestDir\OldName"
        );
        string newName = "NewName";
        FileSystemPathId newPathId = _fileSystemPathIdFixture.CreateFileSystemPathId(
            s_isUnix ? "/TestDir/NewName" : @"C:\TestDir\NewName"
        );

        _mockEnvironmentContext.DirectoryProviderService.RenameDirectory(oldPathId, newName)
            .Returns(ErrorOrFactory.From(newPathId));
        _mockEnvironmentContext.DirectoryProviderService.GetFileName(newPathId)
            .Returns(ErrorOrFactory.From(newName));
        _mockEnvironmentContext.DirectoryProviderService.GetLastWriteTime(newPathId)
            .Returns(ErrorOrFactory.From(Optional<DateTime>.FromNullable(DateTime.Now)));
        _mockEnvironmentContext.DirectoryProviderService.GetCreationTime(newPathId)
            .Returns(ErrorOrFactory.From(Optional<DateTime>.FromNullable(DateTime.Now)));

        // Act
        ErrorOr<Directory> result = _sut.RenameDirectory(oldPathId, newName);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(newPathId, result.Value.Id);
        Assert.Equal(newName, result.Value.Name);
    }

    [Fact]
    public void RenameDirectory_WhenDirectoryProviderServiceReturnsError_ShouldPropagateError()
    {
        // Arrange
        FileSystemPathId oldPathId = _fileSystemPathIdFixture.CreateFileSystemPathId(
            s_isUnix ? "/TestDir/OldName" : @"C:\TestDir\OldName"
        );
        string newName = "NewName";

        _mockEnvironmentContext.DirectoryProviderService.RenameDirectory(oldPathId, newName)
            .Returns(Errors.Permission.UnauthorizedAccess);

        // Act
        ErrorOr<Directory> result = _sut.RenameDirectory(oldPathId, newName);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(Errors.Permission.UnauthorizedAccess, result.FirstError);
    }

    [Fact]
    public void RenameDirectory_WhenDirectoryDetailsAreInaccessible_ShouldReturnInaccessibleDirectory()
    {
        // Arrange
        FileSystemPathId oldPathId = _fileSystemPathIdFixture.CreateFileSystemPathId(
            s_isUnix ? "/TestDir/OldName" : @"C:\TestDir\OldName"
        );
        string newName = "NewName";
        FileSystemPathId newPathId = _fileSystemPathIdFixture.CreateFileSystemPathId(
            s_isUnix ? "/TestDir/NewName" : @"C:\TestDir\NewName"
        );

        _mockEnvironmentContext.DirectoryProviderService.RenameDirectory(oldPathId, newName)
            .Returns(ErrorOrFactory.From(newPathId));

        _mockEnvironmentContext.DirectoryProviderService.GetFileName(newPathId)
            .Returns(Errors.Permission.UnauthorizedAccess);
        _mockEnvironmentContext.DirectoryProviderService.GetLastWriteTime(newPathId)
            .Returns(Errors.Permission.UnauthorizedAccess);
        _mockEnvironmentContext.DirectoryProviderService.GetCreationTime(newPathId)
            .Returns(Errors.Permission.UnauthorizedAccess);

        // Act
        ErrorOr<Directory> result = _sut.RenameDirectory(oldPathId, newName);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(FileSystemItemStatus.Inaccessible, result.Value.Status);
        Assert.Equal(newPathId, result.Value.Id);
        Assert.False(result.Value.DateCreated.HasValue);
        Assert.False(result.Value.DateModified.HasValue);
    }

    [Fact]
    public void RenameDirectory_WhenDirectoryAlreadyExists_ShouldReturnDirectoryAlreadyExistsError()
    {
        // Arrange
        FileSystemPathId oldPathId = _fileSystemPathIdFixture.CreateFileSystemPathId(
            s_isUnix ? "/TestDir/OldName" : @"C:\TestDir\OldName"
        );
        string newName = "ExistingDirectory";
        FileSystemPathId newPathId = _fileSystemPathIdFixture.CreateFileSystemPathId(
            s_isUnix ? "/TestDir/ExistingDirectory" : @"C:\TestDir\ExistingDirectory"
        );

        _mockPlatformContext.PathStrategy.CombinePath(Arg.Any<FileSystemPathId>(), newName)
            .Returns(ErrorOrFactory.From(newPathId));
        _mockEnvironmentContext.DirectoryProviderService.DirectoryExists(newPathId)
            .Returns(ErrorOrFactory.From(true));

        // Act
        ErrorOr<Directory> result = _sut.RenameDirectory(oldPathId, newName);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(Errors.FileSystemManagement.DirectoryAlreadyExists, result.FirstError);
    }

    [Fact]
    public void RenameDirectory_WhenCombinePathReturnsError_ShouldReturnError()
    {
        // Arrange
        FileSystemPathId oldPathId = _fileSystemPathIdFixture.CreateFileSystemPathId(
            s_isUnix ? "/TestDir/OldName" : @"C:\TestDir\OldName"
        );
        string newName = "NewName";

        _mockPlatformContext.PathStrategy.CombinePath(Arg.Any<FileSystemPathId>(), newName)
            .Returns(Errors.FileSystemManagement.InvalidPath);

        // Act
        ErrorOr<Directory> result = _sut.RenameDirectory(oldPathId, newName);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(Errors.FileSystemManagement.InvalidPath, result.FirstError);

        // Verify that DirectoryExists and RenameDirectory were not called
        _mockEnvironmentContext.DirectoryProviderService.DidNotReceive().DirectoryExists(Arg.Any<FileSystemPathId>());
        _mockEnvironmentContext.DirectoryProviderService.DidNotReceive().RenameDirectory(Arg.Any<FileSystemPathId>(), Arg.Any<string>());
    }

    [Fact]
    public void RenameDirectory_WhenDirectoryExistsReturnsError_ShouldPropagateError()
    {
        // Arrange
        FileSystemPathId oldPathId = _fileSystemPathIdFixture.CreateFileSystemPathId(
            s_isUnix ? "/TestDir/OldName" : @"C:\TestDir\OldName"
        );
        string newName = "NewName";
        FileSystemPathId newPathId = _fileSystemPathIdFixture.CreateFileSystemPathId(
            s_isUnix ? "/TestDir/NewName" : @"C:\TestDir\NewName"
        );
        _mockPlatformContext.PathStrategy.CombinePath(Arg.Any<FileSystemPathId>(), newName)
            .Returns(ErrorOrFactory.From(newPathId));

        _mockEnvironmentContext.DirectoryProviderService.DirectoryExists(newPathId)
            .Returns(Errors.Permission.UnauthorizedAccess);

        // Act
        ErrorOr<Directory> result = _sut.RenameDirectory(oldPathId, newName);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(Errors.Permission.UnauthorizedAccess, result.FirstError);

        // Verify that RenameDirectory was not called
        _mockEnvironmentContext.DirectoryProviderService.DidNotReceive().RenameDirectory(Arg.Any<FileSystemPathId>(), Arg.Any<string>());
    }

    [Fact]
    public void DeleteDirectory_WithValidStringPath_ShouldReturnDeleted()
    {
        // Arrange
        string path = s_isUnix ? "/TestDir/ToDelete" : @"C:\TestDir\ToDelete";
        FileSystemPathId pathId = _fileSystemPathIdFixture.CreateFileSystemPathId(path);

        _mockEnvironmentContext.DirectoryProviderService.DeleteDirectory(pathId)
            .Returns(Result.Deleted);

        // Act
        ErrorOr<Deleted> result = _sut.DeleteDirectory(path);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(Result.Deleted, result.Value);
    }

    [Fact]
    public void DeleteDirectory_WithInvalidStringPath_ShouldReturnError()
    {
        // Arrange
        string invalidPath = string.Empty;

        // Act
        ErrorOr<Deleted> result = _sut.DeleteDirectory(invalidPath);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(Errors.FileSystemManagement.InvalidPath, result.FirstError);
    }

    [Fact]
    public void DeleteDirectory_WithFileSystemPathId_ShouldReturnDeleted()
    {
        // Arrange
        FileSystemPathId pathId = _fileSystemPathIdFixture.CreateFileSystemPathId(
            s_isUnix ? "/TestDir/ToDelete" : @"C:\TestDir\ToDelete"
        );

        _mockEnvironmentContext.DirectoryProviderService.DeleteDirectory(pathId)
            .Returns(Result.Deleted);

        // Act
        ErrorOr<Deleted> result = _sut.DeleteDirectory(pathId);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(Result.Deleted, result.Value);
    }

    [Fact]
    public void DeleteDirectory_WhenDirectoryProviderServiceReturnsError_ShouldPropagateError()
    {
        // Arrange
        FileSystemPathId pathId = _fileSystemPathIdFixture.CreateFileSystemPathId(
            s_isUnix ? "/TestDir/ToDelete" : @"C:\TestDir\ToDelete"
        );

        _mockEnvironmentContext.DirectoryProviderService.DeleteDirectory(pathId)
            .Returns(Errors.Permission.UnauthorizedAccess);

        // Act
        ErrorOr<Deleted> result = _sut.DeleteDirectory(pathId);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(Errors.Permission.UnauthorizedAccess, result.FirstError);
    }

    [Fact]
    public void DeleteDirectory_WhenDirectoryDoesNotExist_ShouldReturnDirectoryNotFoundError()
    {
        // Arrange
        FileSystemPathId pathId = _fileSystemPathIdFixture.CreateFileSystemPathId(
            s_isUnix ? "/TestDir/NonExistent" : @"C:\TestDir\NonExistent"
        );

        _mockEnvironmentContext.DirectoryProviderService.DeleteDirectory(pathId)
            .Returns(Errors.FileSystemManagement.DirectoryNotFound);

        // Act
        ErrorOr<Deleted> result = _sut.DeleteDirectory(pathId);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(Errors.FileSystemManagement.DirectoryNotFound, result.FirstError);
    }
}
