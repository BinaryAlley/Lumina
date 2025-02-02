#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Strategies.Path;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.ValueObjects;
using Lumina.Domain.UnitTests.Core.Aggregates.FileSystemManagementAggregate.ValueObjects.Fixtures;
using NSubstitute;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
using System.Linq;
#endregion

namespace Lumina.Domain.UnitTests.Core.Aggregates.FileSystemManagementAggregate.Strategies.Path;

/// <summary>
/// Contains unit tests for the <see cref="UnixPathStrategy"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UnixPathStrategyTests
{
    private readonly IFileSystem _mockFileSystem;
    private readonly UnixPathStrategy _sut;
    private readonly FileSystemPathIdFixture _fileSystemPathIdFixture;
    private readonly PathSegmentFixture _pathSegmentFixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="UnixPathStrategyTests"/> class.
    /// </summary>
    public UnixPathStrategyTests()
    {
        _mockFileSystem = Substitute.For<IFileSystem>();
        _sut = new UnixPathStrategy(_mockFileSystem);
        _fileSystemPathIdFixture = new FileSystemPathIdFixture();
        _pathSegmentFixture = new PathSegmentFixture();
    }

    [Theory]
    [InlineData("/home/user")]
    [InlineData("/var/log/")]
    [InlineData("/etc/config.conf")]
    [InlineData("/")]
    [InlineData("/usr/local/bin")]
    public void IsValidPath_WithValidUnixPaths_ShouldReturnTrue(string path)
    {
        // Arrange
        FileSystemPathId pathId = _fileSystemPathIdFixture.CreateFileSystemPathId(path);

        // Act
        bool result = _sut.IsValidPath(pathId);

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData("C:\\Windows\\System32")]
    [InlineData("relative/path")]
    [InlineData("./relative/path")]
    [InlineData("../parent/path")]
    public void IsValidPath_WithInvalidUnixPaths_ShouldReturnFalse(string path)
    {
        // Arrange
        FileSystemPathId pathId = _fileSystemPathIdFixture.CreateFileSystemPathId(path);

        // Act
        bool result = _sut.IsValidPath(pathId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValidPath_WithPathContainingInvalidCharacters_ShouldReturnFalse()
    {
        // Arrange
        string invalidPath = "/home/user/file\0name";
        FileSystemPathId pathId = _fileSystemPathIdFixture.CreateFileSystemPathId(invalidPath);

        // Act
        bool result = _sut.IsValidPath(pathId);

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData("/home/user/file with spaces")]
    [InlineData("/home/user/file-with-dashes")]
    [InlineData("/home/user/file_with_underscores")]
    [InlineData("/home/user/file.with.dots")]
    [InlineData("/home/user/file~with~tilde")]
    [InlineData("/home/user/file!with!exclamation")]
    [InlineData("/home/user/file$with$dollar")]
    [InlineData("/home/user/file&with&ampersand")]
    [InlineData("/home/user/file'with'singlequote")]
    [InlineData("/home/user/file(with)parentheses")]
    public void IsValidPath_WithValidSpecialCharacters_ShouldReturnTrue(string path)
    {
        // Arrange
        FileSystemPathId pathId = _fileSystemPathIdFixture.CreateFileSystemPathId(path);

        // Act
        bool result = _sut.IsValidPath(pathId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsValidPath_WithVeryLongPath_ShouldReturnTrue()
    {
        // Arrange
        string longPath = "/" + string.Join("/", Enumerable.Repeat("a", 100));
        FileSystemPathId pathId = _fileSystemPathIdFixture.CreateFileSystemPathId(longPath);

        // Act
        bool result = _sut.IsValidPath(pathId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Exists_WhenPathExistsAndIsHiddenAndIncludeHiddenElementsIsTrue_ShouldReturnTrue()
    {
        // Arrange
        string existingPath = "/home/user/existing_file.txt";
        FileSystemPathId pathId = _fileSystemPathIdFixture.CreateFileSystemPathId(existingPath);

        _mockFileSystem.Path.Exists(existingPath).Returns(true);
        _mockFileSystem.File.Exists(existingPath).Returns(true);

        // Act
        bool result = _sut.Exists(pathId, true);

        // Assert
        Assert.True(result);
        _mockFileSystem.Path.Received(1).Exists(existingPath);
    }

    [Fact]
    public void Exists_WhenPathExistsAndIsHiddenAndIncludeHiddenElementsIsFalse_ShouldReturnFalse()
    {
        // Arrange
        string existingPath = "/home/user/existing_file.txt";
        FileSystemPathId pathId = _fileSystemPathIdFixture.CreateFileSystemPathId(existingPath);
        _mockFileSystem.Path.Exists(existingPath).Returns(true);
        _mockFileSystem.File.Exists(existingPath).Returns(true);
        IFileInfo mockFileInfo = Substitute.For<IFileInfo>();
        mockFileInfo.Attributes.Returns(System.IO.FileAttributes.Hidden);
        _mockFileSystem.FileInfo.New(Arg.Any<string>()).Returns(mockFileInfo);

        // Act
        bool result = _sut.Exists(pathId, false);

        // Assert
        Assert.False(result);
        _mockFileSystem.Path.Received(1).Exists(existingPath);
    }

    [Fact]
    public void Exists_WhenPathDoesNotExist_ShouldReturnFalse()
    {
        // Arrange
        string nonExistingPath = "/home/user/non_existing_file.txt";
        FileSystemPathId pathId = _fileSystemPathIdFixture.CreateFileSystemPathId(nonExistingPath);
        _mockFileSystem.Path.Exists(nonExistingPath).Returns(false);

        // Act
        bool result = _sut.Exists(pathId);

        // Assert
        Assert.False(result);
        _mockFileSystem.Path.Received(1).Exists(nonExistingPath);
    }

    [Fact]
    public void Exists_WithRootPath_ShouldCheckExistence()
    {
        // Arrange
        string rootPath = "/";
        FileSystemPathId pathId = _fileSystemPathIdFixture.CreateFileSystemPathId(rootPath);
        _mockFileSystem.Path.Exists(rootPath).Returns(true);

        // Act
        bool result = _sut.Exists(pathId, true);

        // Assert
        Assert.True(result);
        _mockFileSystem.Path.Received(1).Exists(rootPath);
    }

    [Fact]
    public void Exists_WithDirectoryPath_ShouldCheckExistence()
    {
        // Arrange
        string directoryPath = "/home/user/documents/";
        FileSystemPathId pathId = _fileSystemPathIdFixture.CreateFileSystemPathId(directoryPath);
        _mockFileSystem.Path.Exists(directoryPath).Returns(true);
        _mockFileSystem.Directory.Exists(directoryPath).Returns(true);

        // Act
        bool result = _sut.Exists(pathId, true);

        // Assert
        Assert.True(result);
        _mockFileSystem.Path.Received(1).Exists(directoryPath);
    }

    [Fact]
    public void Exists_WithFilePath_ShouldCheckExistence()
    {
        // Arrange
        string filePath = "/home/user/documents/file.txt";
        FileSystemPathId pathId = _fileSystemPathIdFixture.CreateFileSystemPathId(filePath);
        _mockFileSystem.Path.Exists(filePath).Returns(true);
        _mockFileSystem.File.Exists(filePath).Returns(true);

        // Act
        bool result = _sut.Exists(pathId, true);

        // Assert
        Assert.True(result);
        _mockFileSystem.Path.Received(1).Exists(filePath);
    }

    [Fact]
    public void CombinePath_WithValidPathAndName_ShouldReturnCombinedPath()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId("/home/user");
        string name = "documents";

        // Act
        ErrorOr<FileSystemPathId> result = _sut.CombinePath(path, name);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal("/home/user/documents/", result.Value.Path);
    }

    [Fact]
    public void CombinePath_WithTrailingSlashInPath_ShouldReturnCorrectlyCombinedPath()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId("/home/user/");
        string name = "documents";

        // Act
        ErrorOr<FileSystemPathId> result = _sut.CombinePath(path, name);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal("/home/user/documents/", result.Value.Path);
    }

    [Fact]
    public void CombinePath_WithLeadingSlashInName_ShouldReturnCorrectlyCombinedPath()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId("/home/user");
        string name = "/documents";

        // Act
        ErrorOr<FileSystemPathId> result = _sut.CombinePath(path, name);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal("/home/user/documents/", result.Value.Path);
    }

    [Fact]
    public void CombinePath_WithEmptyName_ShouldReturnError()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId("/home/user");
        string name = "";

        // Act
        ErrorOr<FileSystemPathId> result = _sut.CombinePath(path, name);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(Errors.FileSystemManagement.NameCannotBeEmpty, result.FirstError);
    }

    [Fact]
    public void CombinePath_WithNullName_ShouldReturnError()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId("/home/user");
        string name = null!;

        // Act
        ErrorOr<FileSystemPathId> result = _sut.CombinePath(path, name);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(Errors.FileSystemManagement.NameCannotBeEmpty, result.FirstError);
    }

    [Fact]
    public void CombinePath_WithRootPath_ShouldReturnCorrectlyCombinedPath()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId("/");
        string name = "home";

        // Act
        ErrorOr<FileSystemPathId> result = _sut.CombinePath(path, name);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal("/home/", result.Value.Path);
    }

    [Fact]
    public void ParsePath_WithValidUnixPath_ShouldReturnCorrectPathSegments()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId("/home/user/documents/file.txt");

        // Act
        ErrorOr<IEnumerable<PathSegment>> result = _sut.ParsePath(path);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(5, result.Value.Count());
        Assert.Equal(_pathSegmentFixture.CreatePathSegment("/", false, true), result.Value.ElementAt(0));
        Assert.Equal(_pathSegmentFixture.CreatePathSegment("home", true, false), result.Value.ElementAt(1));
        Assert.Equal(_pathSegmentFixture.CreatePathSegment("user", true, false), result.Value.ElementAt(2));
        Assert.Equal(_pathSegmentFixture.CreatePathSegment("documents", true, false), result.Value.ElementAt(3));
        Assert.Equal(_pathSegmentFixture.CreatePathSegment("file.txt", false, false), result.Value.ElementAt(4));
    }

    [Fact]
    public void ParsePath_WithRootPath_ShouldReturnSingleRootSegment()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId("/");

        // Act
        ErrorOr<IEnumerable<PathSegment>> result = _sut.ParsePath(path);

        // Assert
        Assert.False(result.IsError);
        Assert.Single(result.Value);
        Assert.Equal(_pathSegmentFixture.CreatePathSegment("/", false, true), result.Value.Single());
    }

    [Fact]
    public void ParsePath_WithTrailingSlash_ShouldTreatLastSegmentAsDirectory()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId("/home/user/documents/");

        // Act
        ErrorOr<IEnumerable<PathSegment>> result = _sut.ParsePath(path);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(4, result.Value.Count());
        Assert.Equal(_pathSegmentFixture.CreatePathSegment("documents", true, false), result.Value.Last());
    }

    [Fact]
    public void ParsePath_WithRelativePath_ShouldReturnError()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId("home/user");

        // Act
        ErrorOr<IEnumerable<PathSegment>> result = _sut.ParsePath(path);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(Errors.FileSystemManagement.InvalidPath, result.FirstError);
    }

    [Fact]
    public void ParsePath_WithPathContainingDots_ShouldParseCorrectly()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId("/home/user/file.with.dots.txt");

        // Act
        ErrorOr<IEnumerable<PathSegment>> result = _sut.ParsePath(path);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(4, result.Value.Count());
        Assert.Equal(_pathSegmentFixture.CreatePathSegment("file.with.dots.txt", false, false), result.Value.Last());
    }

    [Fact]
    public void GoUpOneLevel_WithValidPath_ShouldReturnParentPath()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId("/home/user/documents");

        // Act
        ErrorOr<IEnumerable<PathSegment>> result = _sut.GoUpOneLevel(path);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(3, result.Value.Count());
        Assert.Equal(_pathSegmentFixture.CreatePathSegment("/", false, true), result.Value.ElementAt(0));
        Assert.Equal(_pathSegmentFixture.CreatePathSegment("home", true, false), result.Value.ElementAt(1));
        Assert.Equal(_pathSegmentFixture.CreatePathSegment("user", true, false), result.Value.ElementAt(2));
    }

    [Fact]
    public void GoUpOneLevel_WithRootPath_ShouldReturnError()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId("/");

        // Act
        ErrorOr<IEnumerable<PathSegment>> result = _sut.GoUpOneLevel(path);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(Errors.FileSystemManagement.CannotNavigateUp, result.FirstError);
    }

    [Fact]
    public void GoUpOneLevel_WithTrailingSlash_ShouldReturnCorrectParentPath()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId("/home/user/");

        // Act
        ErrorOr<IEnumerable<PathSegment>> result = _sut.GoUpOneLevel(path);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(2, result.Value.Count());
        Assert.Equal(_pathSegmentFixture.CreatePathSegment("/", false, true), result.Value.ElementAt(0));
        Assert.Equal(_pathSegmentFixture.CreatePathSegment("home", true, false), result.Value.ElementAt(1));
    }

    [Fact]
    public void GoUpOneLevel_WithInvalidPath_ShouldReturnError()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId("invalid/path");

        // Act
        ErrorOr<IEnumerable<PathSegment>> result = _sut.GoUpOneLevel(path);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(Errors.FileSystemManagement.InvalidPath, result.FirstError);
    }

    [Fact]
    public void GoUpOneLevel_WithSingleLevelPath_ShouldReturnRootPath()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId("/home");

        // Act
        ErrorOr<IEnumerable<PathSegment>> result = _sut.GoUpOneLevel(path);

        // Assert
        Assert.False(result.IsError);
        Assert.Single(result.Value);
        Assert.Equal(_pathSegmentFixture.CreatePathSegment("/", false, true), result.Value.Single());
    }

    [Fact]
    public void GetInvalidPathCharsForPlatform_WhenCalled_ShouldReturnOnlyNullCharacter()
    {
        // Act
        char[] result = _sut.GetInvalidPathCharsForPlatform();

        // Assert
        Assert.Single(result);
        Assert.Contains('\0', result);
    }

    [Fact]
    public void GetInvalidPathCharsForPlatform_WhenCalled_ShouldNotContainOtherCharacters()
    {
        // Act
        char[] result = _sut.GetInvalidPathCharsForPlatform();

        // Assert
        Assert.DoesNotContain('/', result);
        Assert.DoesNotContain('\\', result);
        Assert.DoesNotContain(':', result);
        Assert.DoesNotContain('*', result);
        Assert.DoesNotContain('?', result);
        Assert.DoesNotContain('"', result);
        Assert.DoesNotContain('<', result);
        Assert.DoesNotContain('>', result);
        Assert.DoesNotContain('|', result);
    }

    [Fact]
    public void GetInvalidPathCharsForPlatform_WhenCalled_ShouldReturnSameResultOnMultipleCalls()
    {
        // Act
        char[] result1 = _sut.GetInvalidPathCharsForPlatform();
        char[] result2 = _sut.GetInvalidPathCharsForPlatform();

        // Assert
        Assert.Equal(result1, result2);
    }

    [Fact]
    public void GetPathRoot_WithValidUnixPath_ShouldReturnRootSegment()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId("/home/user/documents");

        // Act
        ErrorOr<PathSegment> result = _sut.GetPathRoot(path);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(_pathSegmentFixture.CreatePathSegment("/", false, true), result.Value);
    }

    [Fact]
    public void GetPathRoot_WithRootPath_ShouldReturnRootSegment()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId("/");

        // Act
        ErrorOr<PathSegment> result = _sut.GetPathRoot(path);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(_pathSegmentFixture.CreatePathSegment("/", false, true), result.Value);
    }

    [Fact]
    public void GetPathRoot_WithInvalidPath_ShouldReturnError()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId("home/user");

        // Act
        ErrorOr<PathSegment> result = _sut.GetPathRoot(path);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(Errors.FileSystemManagement.InvalidPath, result.FirstError);
    }
}
