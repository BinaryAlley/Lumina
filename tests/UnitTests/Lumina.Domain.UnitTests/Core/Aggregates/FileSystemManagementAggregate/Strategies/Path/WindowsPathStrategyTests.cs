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
/// Contains unit tests for the <see cref="WindowsPathStrategy"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class WindowsPathStrategyTests
{
    private readonly IFileSystem _mockFileSystem;
    private readonly WindowsPathStrategy _sut;
    private readonly FileSystemPathIdFixture _fileSystemPathIdFixture;
    private readonly PathSegmentFixture _pathSegmentFixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="WindowsPathStrategyTests"/> class.
    /// </summary>
    public WindowsPathStrategyTests()
    {
        _mockFileSystem = Substitute.For<IFileSystem>();
        _sut = new WindowsPathStrategy(_mockFileSystem);
        _fileSystemPathIdFixture = new FileSystemPathIdFixture();
        _pathSegmentFixture = new PathSegmentFixture();
    }

    [Theory]
    [InlineData(@"C:\Users\User")]
    [InlineData(@"D:\Program Files\")]
    [InlineData(@"E:\Documents\file.txt")]
    [InlineData(@"F:\")]
    [InlineData(@"G:\Projects\Visual Studio 2022\")]
    public void IsValidPath_WithValidWindowsPaths_ShouldReturnTrue(string path)
    {
        // Arrange
        FileSystemPathId pathId = _fileSystemPathIdFixture.CreateFileSystemPathId(path);

        // Act
        bool result = _sut.IsValidPath(pathId);

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData("/home/user")]
    [InlineData("relative\\path")]
    [InlineData(".\\relative\\path")]
    [InlineData("..\\parent\\path")]
    [InlineData("C:invalid_path")]
    public void IsValidPath_WithInvalidWindowsPaths_ShouldReturnFalse(string path)
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
        string invalidPath = @"C:\Users\User\file<name>";
        FileSystemPathId pathId = _fileSystemPathIdFixture.CreateFileSystemPathId(invalidPath);

        // Act
        bool result = _sut.IsValidPath(pathId);

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData(@"C:\Users\User\file with spaces")]
    [InlineData(@"D:\Projects\file-with-dashes")]
    [InlineData(@"E:\Documents\file_with_underscores")]
    [InlineData(@"F:\Data\file.with.dots")]
    [InlineData(@"G:\Backups\file(with)parentheses")]
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
        string longPath = @"C:\" + string.Join("\\", Enumerable.Repeat("a", 100));
        FileSystemPathId pathId = _fileSystemPathIdFixture.CreateFileSystemPathId(longPath);

        // Act
        bool result = _sut.IsValidPath(pathId);

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData(@"\\server\share")]
    [InlineData(@"\\server\share\folder")]
    [InlineData(@"\\server\share\folder\file.txt")]
    public void IsValidPath_WithValidUNCPaths_ShouldReturnTrue(string path)
    {
        // Arrange
        FileSystemPathId pathId = _fileSystemPathIdFixture.CreateFileSystemPathId(path);

        // Act
        bool result = _sut.IsValidPath(pathId);

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData(@"\\")]
    [InlineData(@"\\server")]
    [InlineData(@"\\\server\share")]
    public void IsValidPath_WithInvalidUNCPaths_ShouldReturnFalse(string path)
    {
        // Arrange
        FileSystemPathId pathId = _fileSystemPathIdFixture.CreateFileSystemPathId(path);

        // Act
        bool result = _sut.IsValidPath(pathId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Exists_WhenPathExistsAndIsHiddenAndIncludeHiddenElementsIsTrue_ShouldReturnTrue()
    {
        // Arrange
        string existingPath = @"C:\Users\User\existing_file.txt";
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
        string existingPath = @"C:\Users\User\existing_file.txt";
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
        string nonExistingPath = @"C:\Users\User\non_existing_file.txt";
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
        string rootPath = @"C:\";
        FileSystemPathId pathId = _fileSystemPathIdFixture.CreateFileSystemPathId(rootPath);
        _mockFileSystem.Path.Exists(rootPath).Returns(true);

        // Act
        bool result = _sut.Exists(pathId);

        // Assert
        Assert.True(result);
        _mockFileSystem.Path.Received(1).Exists(rootPath);
    }

    [Fact]
    public void Exists_WithDirectoryPath_ShouldCheckExistence()
    {
        // Arrange
        string directoryPath = @"C:\Users\User\Documents\";
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
        string filePath = @"C:\Users\User\Documents\file.txt";
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
    public void Exists_WithUNCPath_ShouldCheckExistence()
    {
        // Arrange
        string uncPath = @"\\server\share\folder\file.txt";
        FileSystemPathId pathId = _fileSystemPathIdFixture.CreateFileSystemPathId(uncPath);
        _mockFileSystem.Path.Exists(uncPath).Returns(true);

        // Act
        bool result = _sut.Exists(pathId, true);

        // Assert
        Assert.True(result);
        _mockFileSystem.Path.Received(1).Exists(uncPath);
    }

    [Fact]
    public void CombinePath_WithValidPathAndName_ShouldReturnCombinedPath()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId(@"C:\Users\User");
        string name = "Documents";

        // Act
        ErrorOr<FileSystemPathId> result = _sut.CombinePath(path, name);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(@"C:\Users\User\Documents\", result.Value.Path);
    }

    [Fact]
    public void CombinePath_WithTrailingBackslashInPath_ShouldReturnCorrectlyCombinedPath()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId(@"C:\Users\User\");
        string name = "Documents";

        // Act
        ErrorOr<FileSystemPathId> result = _sut.CombinePath(path, name);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(@"C:\Users\User\Documents\", result.Value.Path);
    }

    [Fact]
    public void CombinePath_WithLeadingBackslashInName_ShouldReturnCorrectlyCombinedPath()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId(@"C:\Users\User");
        string name = @"\Documents";

        // Act
        ErrorOr<FileSystemPathId> result = _sut.CombinePath(path, name);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(@"C:\Users\User\Documents\", result.Value.Path);
    }

    [Fact]
    public void CombinePath_WithNullName_ShouldReturnError()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId(@"C:\Users\User");
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
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId(@"C:\");
        string name = "Users";

        // Act
        ErrorOr<FileSystemPathId> result = _sut.CombinePath(path, name);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(@"C:\Users\", result.Value.Path);
    }

    [Fact]
    public void CombinePath_WithUNCPath_ShouldReturnCorrectlyCombinedPath()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId(@"\\server\share");
        string name = "folder";

        // Act
        ErrorOr<FileSystemPathId> result = _sut.CombinePath(path, name);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(@"\\server\share\folder\", result.Value.Path);
    }

    [Fact]
    public void ParsePath_WithValidWindowsPath_ShouldReturnCorrectPathSegments()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId(@"C:\Users\User\Documents\file.txt");

        // Act
        ErrorOr<IEnumerable<PathSegment>> result = _sut.ParsePath(path);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(5, result.Value.Count());
        Assert.Equivalent(_pathSegmentFixture.CreatePathSegment("C:", false, true), result.Value.ElementAt(0));
        Assert.Equivalent(_pathSegmentFixture.CreatePathSegment("Users", true, false), result.Value.ElementAt(1));
        Assert.Equivalent(_pathSegmentFixture.CreatePathSegment("User", true, false), result.Value.ElementAt(2));
        Assert.Equivalent(_pathSegmentFixture.CreatePathSegment("Documents", true, false), result.Value.ElementAt(3));
        Assert.Equivalent(_pathSegmentFixture.CreatePathSegment("file.txt", false, false), result.Value.ElementAt(4));
    }

    [Fact]
    public void ParsePath_WithRootPath_ShouldReturnSingleRootSegment()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId(@"C:\");

        // Act
        ErrorOr<IEnumerable<PathSegment>> result = _sut.ParsePath(path);

        // Assert
        Assert.False(result.IsError);
        Assert.Single(result.Value);
        Assert.Equivalent(_pathSegmentFixture.CreatePathSegment("C:", false, true), result.Value.Single());
    }

    [Fact]
    public void ParsePath_WithTrailingBackslash_ShouldTreatLastSegmentAsDirectory()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId(@"C:\Users\User\Documents\");

        // Act
        ErrorOr<IEnumerable<PathSegment>> result = _sut.ParsePath(path);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(4, result.Value.Count());
        Assert.Equivalent(_pathSegmentFixture.CreatePathSegment("Documents", true, false), result.Value.Last());
    }

    [Fact]
    public void ParsePath_WithRelativePath_ShouldReturnError()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId(@"Users\User");

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
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId(@"C:\Users\User\file.with.dots.txt");

        // Act
        ErrorOr<IEnumerable<PathSegment>> result = _sut.ParsePath(path);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(4, result.Value.Count());
        Assert.Equivalent(_pathSegmentFixture.CreatePathSegment("file.with.dots.txt", false, false), result.Value.Last());
    }

    [Fact]
    public void ParsePath_WithUNCPath_ShouldReturnCorrectPathSegments()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId(@"\\server\share\folder\file.txt");

        // Act
        ErrorOr<IEnumerable<PathSegment>> result = _sut.ParsePath(path);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(5, result.Value.Count());
        Assert.Equivalent(_pathSegmentFixture.CreatePathSegment(@"\\", false, true), result.Value.ElementAt(0));
        Assert.Equivalent(_pathSegmentFixture.CreatePathSegment("server", true, false), result.Value.ElementAt(1));
        Assert.Equivalent(_pathSegmentFixture.CreatePathSegment("share", true, false), result.Value.ElementAt(2));
        Assert.Equivalent(_pathSegmentFixture.CreatePathSegment("folder", true, false), result.Value.ElementAt(3));
        Assert.Equivalent(_pathSegmentFixture.CreatePathSegment("file.txt", false, false), result.Value.ElementAt(4));
    }

    [Fact]
    public void GoUpOneLevel_WithValidPath_ShouldReturnParentPath()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId(@"C:\Users\User\Documents");

        // Act
        ErrorOr<IEnumerable<PathSegment>> result = _sut.GoUpOneLevel(path);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(3, result.Value.Count());
        Assert.Equivalent(_pathSegmentFixture.CreatePathSegment("C:", false, true), result.Value.ElementAt(0));
        Assert.Equivalent(_pathSegmentFixture.CreatePathSegment("Users", true, false), result.Value.ElementAt(1));
        Assert.Equivalent(_pathSegmentFixture.CreatePathSegment("User", true, false), result.Value.ElementAt(2));
    }

    [Fact]
    public void GoUpOneLevel_WithRootPath_ShouldReturnError()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId(@"C:\");

        // Act
        ErrorOr<IEnumerable<PathSegment>> result = _sut.GoUpOneLevel(path);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(Errors.FileSystemManagement.CannotNavigateUp, result.FirstError);
    }

    [Fact]
    public void GoUpOneLevel_WithTrailingBackslash_ShouldReturnCorrectParentPath()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId(@"C:\Users\User\");

        // Act
        ErrorOr<IEnumerable<PathSegment>> result = _sut.GoUpOneLevel(path);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(2, result.Value.Count());
        Assert.Equivalent(_pathSegmentFixture.CreatePathSegment("C:", false, true), result.Value.ElementAt(0));
        Assert.Equivalent(_pathSegmentFixture.CreatePathSegment("Users", true, false), result.Value.ElementAt(1));
    }

    [Fact]
    public void GoUpOneLevel_WithInvalidPath_ShouldReturnError()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId(@"invalid\path");

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
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId(@"C:\Users");

        // Act
        ErrorOr<IEnumerable<PathSegment>> result = _sut.GoUpOneLevel(path);

        // Assert
        Assert.False(result.IsError);
        Assert.Single(result.Value);
        Assert.Equivalent(_pathSegmentFixture.CreatePathSegment("C:", false, true), result.Value.Single());
    }

    [Fact]
    public void GoUpOneLevel_WithUNCPath_ShouldReturnParentPath()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId(@"\\server\share\folder\subfolder");

        // Act
        ErrorOr<IEnumerable<PathSegment>> result = _sut.GoUpOneLevel(path);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(4, result.Value.Count());
        Assert.Equivalent(_pathSegmentFixture.CreatePathSegment(@"\\", false, true), result.Value.ElementAt(0));
        Assert.Equivalent(_pathSegmentFixture.CreatePathSegment("server", true, false), result.Value.ElementAt(1));
        Assert.Equivalent(_pathSegmentFixture.CreatePathSegment("share", true, false), result.Value.ElementAt(2));
        Assert.Equivalent(_pathSegmentFixture.CreatePathSegment("folder", true, false), result.Value.ElementAt(3));
    }

    [Fact]
    public void GoUpOneLevel_WithUNCRootPath_ShouldReturnError()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId(@"\\server\share");

        // Act
        ErrorOr<IEnumerable<PathSegment>> result = _sut.GoUpOneLevel(path);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(Errors.FileSystemManagement.CannotNavigateUp, result.FirstError);
    }

    [Fact]
    public void GoUpOneLevel_WithUNCPathWithOneFolder_ShouldReturnUNCRoot()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId(@"\\server\share\folder");

        // Act
        ErrorOr<IEnumerable<PathSegment>> result = _sut.GoUpOneLevel(path);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(3, result.Value.Count());
        Assert.Equivalent(_pathSegmentFixture.CreatePathSegment(@"\\", false, true), result.Value.ElementAt(0));
        Assert.Equivalent(_pathSegmentFixture.CreatePathSegment("server", true, false), result.Value.ElementAt(1));
        Assert.Equivalent(_pathSegmentFixture.CreatePathSegment("share", true, false), result.Value.ElementAt(2));
    }

    [Fact]
    public void GoUpOneLevel_WithDriveRootAndBackslash_ShouldReturnError()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId(@"C:\");

        // Act
        ErrorOr<IEnumerable<PathSegment>> result = _sut.GoUpOneLevel(path);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(Errors.FileSystemManagement.CannotNavigateUp, result.FirstError);
    }

    [Fact]
    public void GetInvalidPathCharsForPlatform_WhenCalled_ShouldReturnExpectedCharacters()
    {
        // Act
        char[] result = _sut.GetInvalidPathCharsForPlatform();

        // Assert
        Assert.Equal(['<', '>', '"', '/', '|', '?', '*'], result);
    }

    [Fact]
    public void GetInvalidPathCharsForPlatform_WhenCalled_ShouldNotContainCertainCharacters()
    {
        // Act
        char[] result = _sut.GetInvalidPathCharsForPlatform();

        // Assert
        Assert.DoesNotContain('\\', result);
        Assert.DoesNotContain(':', result);
        Assert.DoesNotContain('\0', result);
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
    public void GetPathRoot_WithValidWindowsPath_ShouldReturnRootSegment()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId(@"C:\Users\User\Documents");

        // Act
        ErrorOr<PathSegment> result = _sut.GetPathRoot(path);

        // Assert
        Assert.False(result.IsError);
        Assert.Equivalent(_pathSegmentFixture.CreatePathSegment("C:\\", true, true), result.Value);
    }

    [Fact]
    public void GetPathRoot_WithRootPath_ShouldReturnRootSegment()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId(@"C:\");

        // Act
        ErrorOr<PathSegment> result = _sut.GetPathRoot(path);

        // Assert
        Assert.False(result.IsError);
        Assert.Equivalent(_pathSegmentFixture.CreatePathSegment("C:\\", true, true), result.Value);
    }

    [Fact]
    public void GetPathRoot_WithInvalidPath_ShouldReturnError()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId(@"InvalidFolder");

        // Act
        ErrorOr<PathSegment> result = _sut.GetPathRoot(path);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(Errors.FileSystemManagement.InvalidPath, result.FirstError);
    }

    [Fact]
    public void GetPathRoot_WithUNCPath_ShouldReturnUNCRootSegment()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId(@"\\server\share\folder");

        // Act
        ErrorOr<PathSegment> result = _sut.GetPathRoot(path);

        // Assert
        Assert.False(result.IsError);
        Assert.Equivalent(_pathSegmentFixture.CreatePathSegment(@"\\server\share\", true, false), result.Value);
    }

    [Fact]
    public void GetPathRoot_WithInvalidUNCPath_ShouldReturnError()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId(@"\\");

        // Act
        ErrorOr<PathSegment> result = _sut.GetPathRoot(path);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(Errors.FileSystemManagement.InvalidPath, result.FirstError);
    }

    [Fact]
    public void GetPathRoot_WithUNCPathWithoutShare_ShouldReturnError()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId(@"\\server");

        // Act
        ErrorOr<PathSegment> result = _sut.GetPathRoot(path);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(Errors.FileSystemManagement.InvalidPath, result.FirstError);
    }

    [Fact]
    public void GetPathRoot_WithNonDriveNonUNCPath_ShouldReturnError()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId(@"InvalidPath");

        // Act
        ErrorOr<PathSegment> result = _sut.GetPathRoot(path);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(Errors.FileSystemManagement.InvalidPath, result.FirstError);
    }

    [Fact]
    public void GetPathRoot_WithUNCPathMissingServerName_ShouldReturnError()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId(@"\\");

        // Act
        ErrorOr<PathSegment> result = _sut.GetPathRoot(path);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(Errors.FileSystemManagement.InvalidPath, result.FirstError);
    }

    [Fact]
    public void GetPathRoot_WithUNCPathWithoutThirdBackslash_ShouldReturnServerAndShare()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId(@"\\server\share");

        // Act
        ErrorOr<PathSegment> result = _sut.GetPathRoot(path);

        // Assert
        Assert.False(result.IsError);
        Assert.Equivalent(_pathSegmentFixture.CreatePathSegment(@"\\server\share\", true, false), result.Value);
    }

    [Fact]
    public void GetPathRoot_WithNeitherUNCNorDrivePath_ShouldReturnError()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.CreateFileSystemPathId(@"invalid\path");

        // Act
        ErrorOr<PathSegment> result = _sut.GetPathRoot(path);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(Errors.FileSystemManagement.InvalidPath, result.FirstError);
    }
}
