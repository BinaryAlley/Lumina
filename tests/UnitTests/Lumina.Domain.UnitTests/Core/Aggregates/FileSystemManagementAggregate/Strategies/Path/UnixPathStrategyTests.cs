#region ========================================================================= USING =====================================================================================
using ErrorOr;
using FluentAssertions;
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
        result.Should().BeTrue();
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
        result.Should().BeFalse();
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
        result.Should().BeFalse();
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
        result.Should().BeTrue();
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
        result.Should().BeTrue();
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
        result.Should().BeTrue();
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
        result.Should().BeFalse();
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
        result.Should().BeFalse();
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
        result.Should().BeTrue();
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
        result.Should().BeTrue();
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
        result.Should().BeTrue();
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
        result.IsError.Should().BeFalse();
        result.Value.Path.Should().Be("/home/user/documents/");
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
        result.IsError.Should().BeFalse();
        result.Value.Path.Should().Be("/home/user/documents/");
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
        result.IsError.Should().BeFalse();
        result.Value.Path.Should().Be("/home/user/documents/");
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
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.FileSystemManagement.NameCannotBeEmpty);
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
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.FileSystemManagement.NameCannotBeEmpty);
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
        result.IsError.Should().BeFalse();
        result.Value.Path.Should().Be("/home/");
    }

    [Fact]
    public void ParsePath_WithValidUnixPath_ShouldReturnCorrectPathSegments()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId("/home/user/documents/file.txt");

        // Act
        ErrorOr<IEnumerable<PathSegment>> result = _sut.ParsePath(path);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().HaveCount(5);
        result.Value.ElementAt(0).Should().BeEquivalentTo(_pathSegmentFixture.CreatePathSegment("/", false, true));
        result.Value.ElementAt(1).Should().BeEquivalentTo(_pathSegmentFixture.CreatePathSegment("home", true, false));
        result.Value.ElementAt(2).Should().BeEquivalentTo(_pathSegmentFixture.CreatePathSegment("user", true, false));
        result.Value.ElementAt(3).Should().BeEquivalentTo(_pathSegmentFixture.CreatePathSegment("documents", true, false));
        result.Value.ElementAt(4).Should().BeEquivalentTo(_pathSegmentFixture.CreatePathSegment("file.txt", false, false));
    }

    [Fact]
    public void ParsePath_WithRootPath_ShouldReturnSingleRootSegment()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId("/");

        // Act
        ErrorOr<IEnumerable<PathSegment>> result = _sut.ParsePath(path);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().HaveCount(1);
        result.Value.Single().Should().BeEquivalentTo(_pathSegmentFixture.CreatePathSegment("/", false, true));
    }

    [Fact]
    public void ParsePath_WithTrailingSlash_ShouldTreatLastSegmentAsDirectory()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId("/home/user/documents/");

        // Act
        ErrorOr<IEnumerable<PathSegment>> result = _sut.ParsePath(path);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().HaveCount(4);
        result.Value.Last().Should().BeEquivalentTo(_pathSegmentFixture.CreatePathSegment("documents", true, false));
    }

    [Fact]
    public void ParsePath_WithRelativePath_ShouldReturnError()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId("home/user");

        // Act
        ErrorOr<IEnumerable<PathSegment>> result = _sut.ParsePath(path);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.FileSystemManagement.InvalidPath);
    }

    [Fact]
    public void ParsePath_WithPathContainingDots_ShouldParseCorrectly()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId("/home/user/file.with.dots.txt");

        // Act
        ErrorOr<IEnumerable<PathSegment>> result = _sut.ParsePath(path);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().HaveCount(4);
        result.Value.Last().Should().BeEquivalentTo(_pathSegmentFixture.CreatePathSegment("file.with.dots.txt", false, false));
    }

    [Fact]
    public void GoUpOneLevel_WithValidPath_ShouldReturnParentPath()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId("/home/user/documents");

        // Act
        ErrorOr<IEnumerable<PathSegment>> result = _sut.GoUpOneLevel(path);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().HaveCount(3);
        result.Value.ElementAt(0).Should().BeEquivalentTo(_pathSegmentFixture.CreatePathSegment("/", false, true));
        result.Value.ElementAt(1).Should().BeEquivalentTo(_pathSegmentFixture.CreatePathSegment("home", true, false));
        result.Value.ElementAt(2).Should().BeEquivalentTo(_pathSegmentFixture.CreatePathSegment("user", true, false));
    }

    [Fact]
    public void GoUpOneLevel_WithRootPath_ShouldReturnError()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId("/");

        // Act
        ErrorOr<IEnumerable<PathSegment>> result = _sut.GoUpOneLevel(path);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.FileSystemManagement.CannotNavigateUp);
    }

    [Fact]
    public void GoUpOneLevel_WithTrailingSlash_ShouldReturnCorrectParentPath()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId("/home/user/");

        // Act
        ErrorOr<IEnumerable<PathSegment>> result = _sut.GoUpOneLevel(path);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().HaveCount(2);
        result.Value.ElementAt(0).Should().BeEquivalentTo(_pathSegmentFixture.CreatePathSegment("/", false, true));
        result.Value.ElementAt(1).Should().BeEquivalentTo(_pathSegmentFixture.CreatePathSegment("home", true, false));
    }

    [Fact]
    public void GoUpOneLevel_WithInvalidPath_ShouldReturnError()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId("invalid/path");

        // Act
        ErrorOr<IEnumerable<PathSegment>> result = _sut.GoUpOneLevel(path);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.FileSystemManagement.InvalidPath);
    }

    [Fact]
    public void GoUpOneLevel_WithSingleLevelPath_ShouldReturnRootPath()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId("/home");

        // Act
        ErrorOr<IEnumerable<PathSegment>> result = _sut.GoUpOneLevel(path);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().HaveCount(1);
        result.Value.Single().Should().BeEquivalentTo(_pathSegmentFixture.CreatePathSegment("/", false, true));
    }

    [Fact]
    public void GetInvalidPathCharsForPlatform_WhenCalled_ShouldReturnOnlyNullCharacter()
    {
        // Act
        char[] result = _sut.GetInvalidPathCharsForPlatform();

        // Assert
        result.Should().HaveCount(1);
        result.Should().Contain('\0');
    }

    [Fact]
    public void GetInvalidPathCharsForPlatform_WhenCalled_ShouldNotContainOtherCharacters()
    {
        // Act
        char[] result = _sut.GetInvalidPathCharsForPlatform();

        // Assert
        result.Should().NotContain('/');
        result.Should().NotContain('\\');
        result.Should().NotContain(':');
        result.Should().NotContain('*');
        result.Should().NotContain('?');
        result.Should().NotContain('"');
        result.Should().NotContain('<');
        result.Should().NotContain('>');
        result.Should().NotContain('|');
    }

    [Fact]
    public void GetInvalidPathCharsForPlatform_WhenCalled_ShouldReturnSameResultOnMultipleCalls()
    {
        // Act
        char[] result1 = _sut.GetInvalidPathCharsForPlatform();
        char[] result2 = _sut.GetInvalidPathCharsForPlatform();

        // Assert
        result1.Should().BeEquivalentTo(result2);
    }

    [Fact]
    public void GetPathRoot_WithValidUnixPath_ShouldReturnRootSegment()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId("/home/user/documents");

        // Act
        ErrorOr<PathSegment> result = _sut.GetPathRoot(path);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeEquivalentTo(_pathSegmentFixture.CreatePathSegment("/", false, true));
    }

    [Fact]
    public void GetPathRoot_WithRootPath_ShouldReturnRootSegment()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId("/");

        // Act
        ErrorOr<PathSegment> result = _sut.GetPathRoot(path);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeEquivalentTo(_pathSegmentFixture.CreatePathSegment("/", false, true));
    }

    [Fact]
    public void GetPathRoot_WithInvalidPath_ShouldReturnError()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId("home/user");

        // Act
        ErrorOr<PathSegment> result = _sut.GetPathRoot(path);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.FileSystemManagement.InvalidPath);
    }
}
