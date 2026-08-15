#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Strategies.Path;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.ValueObjects;
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
    private readonly FileSystemPathIdFixture _fileSystemPathIdFixture = new();
    private readonly PathSegmentFixture _pathSegmentFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="UnixPathStrategyTests"/> class.
    /// </summary>
    public UnixPathStrategyTests()
    {
        _mockFileSystem = Substitute.For<IFileSystem>();
        _sut = new UnixPathStrategy(_mockFileSystem);
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
        FileSystemPathId pathId = _fileSystemPathIdFixture.Create(path);

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
        FileSystemPathId pathId = _fileSystemPathIdFixture.Create(path);

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
        FileSystemPathId pathId = _fileSystemPathIdFixture.Create(invalidPath);

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
        FileSystemPathId pathId = _fileSystemPathIdFixture.Create(path);

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
        FileSystemPathId pathId = _fileSystemPathIdFixture.Create(longPath);

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
        FileSystemPathId pathId = _fileSystemPathIdFixture.Create(existingPath);

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
        FileSystemPathId pathId = _fileSystemPathIdFixture.Create(existingPath);
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
        FileSystemPathId pathId = _fileSystemPathIdFixture.Create(nonExistingPath);
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
        FileSystemPathId pathId = _fileSystemPathIdFixture.Create(rootPath);
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
        FileSystemPathId pathId = _fileSystemPathIdFixture.Create(directoryPath);
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
        FileSystemPathId pathId = _fileSystemPathIdFixture.Create(filePath);
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
        FileSystemPathId path = _fileSystemPathIdFixture.Create("/home/user");
        string name = "documents";

        // Act
        Result<FileSystemPathId> result = _sut.CombinePath(path, name);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal("/home/user/documents/", result.Value.Path);
    }

    [Fact]
    public void CombinePath_WithTrailingSlashInPath_ShouldReturnCorrectlyCombinedPath()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.Create("/home/user/");
        string name = "documents";

        // Act
        Result<FileSystemPathId> result = _sut.CombinePath(path, name);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal("/home/user/documents/", result.Value.Path);
    }

    [Fact]
    public void CombinePath_WithLeadingSlashInName_ShouldReturnCorrectlyCombinedPath()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.Create("/home/user");
        string name = "/documents";

        // Act
        Result<FileSystemPathId> result = _sut.CombinePath(path, name);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal("/home/user/documents/", result.Value.Path);
    }

    [Fact]
    public void CombinePath_WithEmptyName_ShouldReturnError()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.Create("/home/user");
        string name = "";

        // Act
        Result<FileSystemPathId> result = _sut.CombinePath(path, name);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.FileSystemManagement.NameCannotBeEmpty, result.FirstError);
    }

    [Fact]
    public void CombinePath_WithNullName_ShouldReturnError()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.Create("/home/user");
        string name = null!;

        // Act
        Result<FileSystemPathId> result = _sut.CombinePath(path, name);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.FileSystemManagement.NameCannotBeEmpty, result.FirstError);
    }

    [Fact]
    public void CombinePath_WithRootPath_ShouldReturnCorrectlyCombinedPath()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.Create("/");
        string name = "home";

        // Act
        Result<FileSystemPathId> result = _sut.CombinePath(path, name);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal("/home/", result.Value.Path);
    }

    [Fact]
    public void ParsePath_WithValidUnixPath_ShouldReturnCorrectPathSegments()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.Create("/home/user/documents/file.txt");

        // Act
        Result<IEnumerable<PathSegment>> result = _sut.ParsePath(path);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(5, result.Value.Count());
        Assert.Equal(_pathSegmentFixture.Create("/", false, true), result.Value.ElementAt(0));
        Assert.Equal(_pathSegmentFixture.Create("home", true, false), result.Value.ElementAt(1));
        Assert.Equal(_pathSegmentFixture.Create("user", true, false), result.Value.ElementAt(2));
        Assert.Equal(_pathSegmentFixture.Create("documents", true, false), result.Value.ElementAt(3));
        Assert.Equal(_pathSegmentFixture.Create("file.txt", false, false), result.Value.ElementAt(4));
    }

    [Fact]
    public void ParsePath_WithRootPath_ShouldReturnSingleRootSegment()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.Create("/");

        // Act
        Result<IEnumerable<PathSegment>> result = _sut.ParsePath(path);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Single(result.Value);
        Assert.Equal(_pathSegmentFixture.Create("/", false, true), result.Value.Single());
    }

    [Fact]
    public void ParsePath_WithTrailingSlash_ShouldTreatLastSegmentAsDirectory()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.Create("/home/user/documents/");

        // Act
        Result<IEnumerable<PathSegment>> result = _sut.ParsePath(path);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(4, result.Value.Count());
        Assert.Equal(_pathSegmentFixture.Create("documents", true, false), result.Value.Last());
    }

    [Fact]
    public void ParsePath_WithRelativePath_ShouldReturnError()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.Create("home/user");

        // Act
        Result<IEnumerable<PathSegment>> result = _sut.ParsePath(path);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.FileSystemManagement.InvalidPath, result.FirstError);
    }

    [Fact]
    public void ParsePath_WithPathContainingDots_ShouldParseCorrectly()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.Create("/home/user/file.with.dots.txt");

        // Act
        Result<IEnumerable<PathSegment>> result = _sut.ParsePath(path);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(4, result.Value.Count());
        Assert.Equal(_pathSegmentFixture.Create("file.with.dots.txt", false, false), result.Value.Last());
    }

    [Fact]
    public void GoUpOneLevel_WithValidPath_ShouldReturnParentPath()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.Create("/home/user/documents");

        // Act
        Result<IEnumerable<PathSegment>> result = _sut.GoUpOneLevel(path);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(3, result.Value.Count());
        Assert.Equal(_pathSegmentFixture.Create("/", false, true), result.Value.ElementAt(0));
        Assert.Equal(_pathSegmentFixture.Create("home", true, false), result.Value.ElementAt(1));
        Assert.Equal(_pathSegmentFixture.Create("user", true, false), result.Value.ElementAt(2));
    }

    [Fact]
    public void GoUpOneLevel_WithRootPath_ShouldReturnError()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.Create("/");

        // Act
        Result<IEnumerable<PathSegment>> result = _sut.GoUpOneLevel(path);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.FileSystemManagement.CannotNavigateUp, result.FirstError);
    }

    [Fact]
    public void GoUpOneLevel_WithTrailingSlash_ShouldReturnCorrectParentPath()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.Create("/home/user/");

        // Act
        Result<IEnumerable<PathSegment>> result = _sut.GoUpOneLevel(path);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(2, result.Value.Count());
        Assert.Equal(_pathSegmentFixture.Create("/", false, true), result.Value.ElementAt(0));
        Assert.Equal(_pathSegmentFixture.Create("home", true, false), result.Value.ElementAt(1));
    }

    [Fact]
    public void GoUpOneLevel_WithInvalidPath_ShouldReturnError()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.Create("invalid/path");

        // Act
        Result<IEnumerable<PathSegment>> result = _sut.GoUpOneLevel(path);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.FileSystemManagement.InvalidPath, result.FirstError);
    }

    [Fact]
    public void GoUpOneLevel_WithSingleLevelPath_ShouldReturnRootPath()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.Create("/home");

        // Act
        Result<IEnumerable<PathSegment>> result = _sut.GoUpOneLevel(path);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Single(result.Value);
        Assert.Equal(_pathSegmentFixture.Create("/", false, true), result.Value.Single());
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
        FileSystemPathId path = _fileSystemPathIdFixture.Create("/home/user/documents");

        // Act
        Result<PathSegment> result = _sut.GetPathRoot(path);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(_pathSegmentFixture.Create("/", false, true), result.Value);
    }

    [Fact]
    public void GetPathRoot_WithRootPath_ShouldReturnRootSegment()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.Create("/");

        // Act
        Result<PathSegment> result = _sut.GetPathRoot(path);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(_pathSegmentFixture.Create("/", false, true), result.Value);
    }

    [Fact]
    public void GetPathRoot_WithInvalidPath_ShouldReturnError()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.Create("home/user");

        // Act
        Result<PathSegment> result = _sut.GetPathRoot(path);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.FileSystemManagement.InvalidPath, result.FirstError);
    }
}
