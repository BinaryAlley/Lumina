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

namespace Lumina.Domain.UnitTests.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Strategies.Path;

/// <summary>
/// Contains unit tests for the <see cref="WindowsPathStrategy"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class WindowsPathStrategyTests
{
    private readonly IFileSystem _mockFileSystem;
    private readonly WindowsPathStrategy _sut;
    private readonly FileSystemPathIdFixture _fileSystemPathIdFixture = new();
    private readonly PathSegmentFixture _pathSegmentFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="WindowsPathStrategyTests"/> class.
    /// </summary>
    public WindowsPathStrategyTests()
    {
        _mockFileSystem = Substitute.For<IFileSystem>();
        _sut = new WindowsPathStrategy(_mockFileSystem);
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
        FileSystemPathId pathId = _fileSystemPathIdFixture.Create(path);

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
        string invalidPath = @"C:\Users\User\file<name>";
        FileSystemPathId pathId = _fileSystemPathIdFixture.Create(invalidPath);

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
        string longPath = @"C:\" + string.Join("\\", Enumerable.Repeat("a", 100));
        FileSystemPathId pathId = _fileSystemPathIdFixture.Create(longPath);

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
        FileSystemPathId pathId = _fileSystemPathIdFixture.Create(path);

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
        FileSystemPathId pathId = _fileSystemPathIdFixture.Create(path);

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
        string existingPath = @"C:\Users\User\existing_file.txt";
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
        string nonExistingPath = @"C:\Users\User\non_existing_file.txt";
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
        string rootPath = @"C:\";
        FileSystemPathId pathId = _fileSystemPathIdFixture.Create(rootPath);
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
        string filePath = @"C:\Users\User\Documents\file.txt";
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
    public void Exists_WithUNCPath_ShouldCheckExistence()
    {
        // Arrange
        string uncPath = @"\\server\share\folder\file.txt";
        FileSystemPathId pathId = _fileSystemPathIdFixture.Create(uncPath);
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
        FileSystemPathId path = _fileSystemPathIdFixture.Create(@"C:\Users\User");
        string name = "Documents";

        // Act
        Result<FileSystemPathId> result = _sut.CombinePath(path, name);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(@"C:\Users\User\Documents\", result.Value.Path);
    }

    [Fact]
    public void CombinePath_WithTrailingBackslashInPath_ShouldReturnCorrectlyCombinedPath()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.Create(@"C:\Users\User\");
        string name = "Documents";

        // Act
        Result<FileSystemPathId> result = _sut.CombinePath(path, name);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(@"C:\Users\User\Documents\", result.Value.Path);
    }

    [Fact]
    public void CombinePath_WithLeadingBackslashInName_ShouldReturnCorrectlyCombinedPath()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.Create(@"C:\Users\User");
        string name = @"\Documents";

        // Act
        Result<FileSystemPathId> result = _sut.CombinePath(path, name);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(@"C:\Users\User\Documents\", result.Value.Path);
    }

    [Fact]
    public void CombinePath_WithNullName_ShouldReturnError()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.Create(@"C:\Users\User");
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
        FileSystemPathId path = _fileSystemPathIdFixture.Create(@"C:\");
        string name = "Users";

        // Act
        Result<FileSystemPathId> result = _sut.CombinePath(path, name);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(@"C:\Users\", result.Value.Path);
    }

    [Fact]
    public void CombinePath_WithUNCPath_ShouldReturnCorrectlyCombinedPath()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.Create(@"\\server\share");
        string name = "folder";

        // Act
        Result<FileSystemPathId> result = _sut.CombinePath(path, name);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(@"\\server\share\folder\", result.Value.Path);
    }

    [Fact]
    public void ParsePath_WithValidWindowsPath_ShouldReturnCorrectPathSegments()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.Create(@"C:\Users\User\Documents\file.txt");

        // Act
        Result<IEnumerable<PathSegment>> result = _sut.ParsePath(path);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(5, result.Value.Count());
        Assert.Equivalent(_pathSegmentFixture.Create("C:", false, true), result.Value.ElementAt(0));
        Assert.Equivalent(_pathSegmentFixture.Create("Users", true, false), result.Value.ElementAt(1));
        Assert.Equivalent(_pathSegmentFixture.Create("User", true, false), result.Value.ElementAt(2));
        Assert.Equivalent(_pathSegmentFixture.Create("Documents", true, false), result.Value.ElementAt(3));
        Assert.Equivalent(_pathSegmentFixture.Create("file.txt", false, false), result.Value.ElementAt(4));
    }

    [Fact]
    public void ParsePath_WithRootPath_ShouldReturnSingleRootSegment()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.Create(@"C:\");

        // Act
        Result<IEnumerable<PathSegment>> result = _sut.ParsePath(path);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Single(result.Value);
        Assert.Equivalent(_pathSegmentFixture.Create("C:", false, true), result.Value.Single());
    }

    [Fact]
    public void ParsePath_WithTrailingBackslash_ShouldTreatLastSegmentAsDirectory()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.Create(@"C:\Users\User\Documents\");

        // Act
        Result<IEnumerable<PathSegment>> result = _sut.ParsePath(path);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(4, result.Value.Count());
        Assert.Equivalent(_pathSegmentFixture.Create("Documents", true, false), result.Value.Last());
    }

    [Fact]
    public void ParsePath_WithRelativePath_ShouldReturnError()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.Create(@"Users\User");

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
        FileSystemPathId path = _fileSystemPathIdFixture.Create(@"C:\Users\User\file.with.dots.txt");

        // Act
        Result<IEnumerable<PathSegment>> result = _sut.ParsePath(path);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(4, result.Value.Count());
        Assert.Equivalent(_pathSegmentFixture.Create("file.with.dots.txt", false, false), result.Value.Last());
    }

    [Fact]
    public void ParsePath_WithUNCPath_ShouldReturnCorrectPathSegments()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.Create(@"\\server\share\folder\file.txt");

        // Act
        Result<IEnumerable<PathSegment>> result = _sut.ParsePath(path);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(5, result.Value.Count());
        Assert.Equivalent(_pathSegmentFixture.Create(@"\\", false, true), result.Value.ElementAt(0));
        Assert.Equivalent(_pathSegmentFixture.Create("server", true, false), result.Value.ElementAt(1));
        Assert.Equivalent(_pathSegmentFixture.Create("share", true, false), result.Value.ElementAt(2));
        Assert.Equivalent(_pathSegmentFixture.Create("folder", true, false), result.Value.ElementAt(3));
        Assert.Equivalent(_pathSegmentFixture.Create("file.txt", false, false), result.Value.ElementAt(4));
    }

    [Fact]
    public void GoUpOneLevel_WithValidPath_ShouldReturnParentPath()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.Create(@"C:\Users\User\Documents");

        // Act
        Result<IEnumerable<PathSegment>> result = _sut.GoUpOneLevel(path);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(3, result.Value.Count());
        Assert.Equivalent(_pathSegmentFixture.Create("C:", false, true), result.Value.ElementAt(0));
        Assert.Equivalent(_pathSegmentFixture.Create("Users", true, false), result.Value.ElementAt(1));
        Assert.Equivalent(_pathSegmentFixture.Create("User", true, false), result.Value.ElementAt(2));
    }

    [Fact]
    public void GoUpOneLevel_WithRootPath_ShouldReturnError()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.Create(@"C:\");

        // Act
        Result<IEnumerable<PathSegment>> result = _sut.GoUpOneLevel(path);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.FileSystemManagement.CannotNavigateUp, result.FirstError);
    }

    [Fact]
    public void GoUpOneLevel_WithTrailingBackslash_ShouldReturnCorrectParentPath()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.Create(@"C:\Users\User\");

        // Act
        Result<IEnumerable<PathSegment>> result = _sut.GoUpOneLevel(path);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(2, result.Value.Count());
        Assert.Equivalent(_pathSegmentFixture.Create("C:", false, true), result.Value.ElementAt(0));
        Assert.Equivalent(_pathSegmentFixture.Create("Users", true, false), result.Value.ElementAt(1));
    }

    [Fact]
    public void GoUpOneLevel_WithInvalidPath_ShouldReturnError()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.Create(@"invalid\path");

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
        FileSystemPathId path = _fileSystemPathIdFixture.Create(@"C:\Users");

        // Act
        Result<IEnumerable<PathSegment>> result = _sut.GoUpOneLevel(path);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Single(result.Value);
        Assert.Equivalent(_pathSegmentFixture.Create("C:", false, true), result.Value.Single());
    }

    [Fact]
    public void GoUpOneLevel_WithUNCPath_ShouldReturnParentPath()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.Create(@"\\server\share\folder\subfolder");

        // Act
        Result<IEnumerable<PathSegment>> result = _sut.GoUpOneLevel(path);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(4, result.Value.Count());
        Assert.Equivalent(_pathSegmentFixture.Create(@"\\", false, true), result.Value.ElementAt(0));
        Assert.Equivalent(_pathSegmentFixture.Create("server", true, false), result.Value.ElementAt(1));
        Assert.Equivalent(_pathSegmentFixture.Create("share", true, false), result.Value.ElementAt(2));
        Assert.Equivalent(_pathSegmentFixture.Create("folder", true, false), result.Value.ElementAt(3));
    }

    [Fact]
    public void GoUpOneLevel_WithUNCRootPath_ShouldReturnError()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.Create(@"\\server\share");

        // Act
        Result<IEnumerable<PathSegment>> result = _sut.GoUpOneLevel(path);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.FileSystemManagement.CannotNavigateUp, result.FirstError);
    }

    [Fact]
    public void GoUpOneLevel_WithUNCPathWithOneFolder_ShouldReturnUNCRoot()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.Create(@"\\server\share\folder");

        // Act
        Result<IEnumerable<PathSegment>> result = _sut.GoUpOneLevel(path);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(3, result.Value.Count());
        Assert.Equivalent(_pathSegmentFixture.Create(@"\\", false, true), result.Value.ElementAt(0));
        Assert.Equivalent(_pathSegmentFixture.Create("server", true, false), result.Value.ElementAt(1));
        Assert.Equivalent(_pathSegmentFixture.Create("share", true, false), result.Value.ElementAt(2));
    }

    [Fact]
    public void GoUpOneLevel_WithDriveRootAndBackslash_ShouldReturnError()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.Create(@"C:\");

        // Act
        Result<IEnumerable<PathSegment>> result = _sut.GoUpOneLevel(path);

        // Assert
        Assert.True(result.IsFailure);
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
        FileSystemPathId path = _fileSystemPathIdFixture.Create(@"C:\Users\User\Documents");

        // Act
        Result<PathSegment> result = _sut.GetPathRoot(path);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equivalent(_pathSegmentFixture.Create("C:\\", true, true), result.Value);
    }

    [Fact]
    public void GetPathRoot_WithRootPath_ShouldReturnRootSegment()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.Create(@"C:\");

        // Act
        Result<PathSegment> result = _sut.GetPathRoot(path);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equivalent(_pathSegmentFixture.Create("C:\\", true, true), result.Value);
    }

    [Fact]
    public void GetPathRoot_WithInvalidPath_ShouldReturnError()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.Create(@"InvalidFolder");

        // Act
        Result<PathSegment> result = _sut.GetPathRoot(path);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.FileSystemManagement.InvalidPath, result.FirstError);
    }

    [Fact]
    public void GetPathRoot_WithUNCPath_ShouldReturnUNCRootSegment()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.Create(@"\\server\share\folder");

        // Act
        Result<PathSegment> result = _sut.GetPathRoot(path);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equivalent(_pathSegmentFixture.Create(@"\\server\share\", true, false), result.Value);
    }

    [Fact]
    public void GetPathRoot_WithInvalidUNCPath_ShouldReturnError()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.Create(@"\\");

        // Act
        Result<PathSegment> result = _sut.GetPathRoot(path);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.FileSystemManagement.InvalidPath, result.FirstError);
    }

    [Fact]
    public void GetPathRoot_WithUNCPathWithoutShare_ShouldReturnError()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.Create(@"\\server");

        // Act
        Result<PathSegment> result = _sut.GetPathRoot(path);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.FileSystemManagement.InvalidPath, result.FirstError);
    }

    [Fact]
    public void GetPathRoot_WithNonDriveNonUNCPath_ShouldReturnError()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.Create(@"InvalidPath");

        // Act
        Result<PathSegment> result = _sut.GetPathRoot(path);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.FileSystemManagement.InvalidPath, result.FirstError);
    }

    [Fact]
    public void GetPathRoot_WithUNCPathMissingServerName_ShouldReturnError()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.Create(@"\\");

        // Act
        Result<PathSegment> result = _sut.GetPathRoot(path);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.FileSystemManagement.InvalidPath, result.FirstError);
    }

    [Fact]
    public void GetPathRoot_WithUNCPathWithoutThirdBackslash_ShouldReturnServerAndShare()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.Create(@"\\server\share");

        // Act
        Result<PathSegment> result = _sut.GetPathRoot(path);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equivalent(_pathSegmentFixture.Create(@"\\server\share\", true, false), result.Value);
    }

    [Fact]
    public void GetPathRoot_WithNeitherUNCNorDrivePath_ShouldReturnError()
    {
        // Arrange
        FileSystemPathId path = _fileSystemPathIdFixture.Create(@"invalid\path");

        // Act
        Result<PathSegment> result = _sut.GetPathRoot(path);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.FileSystemManagement.InvalidPath, result.FirstError);
    }
}
