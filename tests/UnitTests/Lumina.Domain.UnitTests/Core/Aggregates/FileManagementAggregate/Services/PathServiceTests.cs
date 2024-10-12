#region ========================================================================= USING =====================================================================================
using ErrorOr;
using FluentAssertions;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.Services;
using Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.Strategies.Path;
using Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.Strategies.Platform;
using Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.ValueObjects;
using Lumina.Domain.UnitTests.Core.Aggregates.FileManagementAggregate.ValueObjects.Fixtures;
using NSubstitute;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.UnitTests.Core.Aggregates.FileManagementAggregate.Services;

/// <summary>
/// Contains unit tests for the <see cref="PathService"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class PathServiceTests
{
    private readonly IPlatformContextManager _mockPlatformContextManager;
    private readonly IPlatformContext _mockPlatformContext;
    private readonly IPathStrategy _mockPathStrategy;
    private readonly PathService _sut;
    private readonly FileSystemPathIdFixture _fileSystemPathIdFixture;
    private readonly PathSegmentFixture _pathSegmentFixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="PathServiceTests"/> class.
    /// </summary>
    public PathServiceTests()
    {
        _mockPlatformContextManager = Substitute.For<IPlatformContextManager>();
        _mockPlatformContext = Substitute.For<IPlatformContext>();
        _mockPathStrategy = Substitute.For<IPathStrategy>();
        _mockPlatformContext.PathStrategy.Returns(_mockPathStrategy);
        _mockPlatformContextManager.GetCurrentContext().Returns(_mockPlatformContext);
        _sut = new PathService(_mockPlatformContextManager);
        _fileSystemPathIdFixture = new FileSystemPathIdFixture();
        _pathSegmentFixture = new PathSegmentFixture();
    }

    [Fact]
    public void IsValidPath_WithValidPath_ShouldReturnTrue()
    {
        // Arrange
        string validPath = @"C:\ValidPath";
        FileSystemPathId pathId = _fileSystemPathIdFixture.CreateFileSystemPathId(validPath);
        _mockPathStrategy.IsValidPath(pathId).Returns(true);

        // Act
        bool result = _sut.IsValidPath(validPath);

        // Assert
        result.Should().BeTrue();
        _mockPathStrategy.Received(1).IsValidPath(Arg.Is<FileSystemPathId>(id => id.Path == validPath));
    }

    [Fact]
    public void IsValidPath_WithInvalidPath_ShouldReturnFalse()
    {
        // Arrange
        string invalidPath = @"C:\Invalid\*Path";
        FileSystemPathId pathId = _fileSystemPathIdFixture.CreateFileSystemPathId(invalidPath);
        _mockPathStrategy.IsValidPath(pathId).Returns(false);

        // Act
        bool result = _sut.IsValidPath(invalidPath);

        // Assert
        result.Should().BeFalse();
        _mockPathStrategy.Received(1).IsValidPath(Arg.Is<FileSystemPathId>(id => id.Path == invalidPath));
    }

    [Fact]
    public void IsValidPath_WithEmptyPath_ShouldReturnFalse()
    {
        // Arrange
        string emptyPath = string.Empty;

        // Act
        bool result = _sut.IsValidPath(emptyPath);

        // Assert
        result.Should().BeFalse();
        _mockPathStrategy.DidNotReceive().IsValidPath(Arg.Any<FileSystemPathId>());
    }

    [Fact]
    public void IsValidPath_WithNullPath_ShouldReturnFalse()
    {
        // Arrange
        string? nullPath = null;

        // Act
        bool result = _sut.IsValidPath(nullPath!);

        // Assert
        result.Should().BeFalse();
        _mockPathStrategy.DidNotReceive().IsValidPath(Arg.Any<FileSystemPathId>());
    }

    [Fact]
    public void Exists_WithValidExistingPath_ShouldReturnTrue()
    {
        // Arrange
        string existingPath = @"C:\ExistingPath";
        FileSystemPathId pathId = _fileSystemPathIdFixture.CreateFileSystemPathId(existingPath);
        _mockPathStrategy.Exists(pathId).Returns(true);

        // Act
        bool result = _sut.Exists(existingPath);

        // Assert
        result.Should().BeTrue();
        _mockPathStrategy.Received(1).Exists(Arg.Is<FileSystemPathId>(id => id.Path == existingPath));
    }

    [Fact]
    public void Exists_WhenPathIsHiddenAndIncludeHiddenElementsIsTrue_ShouldReturnTrue()
    {
        // Arrange
        string existingPath = @"C:\ExistingPath";
        FileSystemPathId pathId = _fileSystemPathIdFixture.CreateFileSystemPathId(existingPath);
        _mockPathStrategy.Exists(pathId, true).Returns(true);

        // Act
        bool result = _sut.Exists(existingPath, true);

        // Assert
        result.Should().BeTrue();
        _mockPathStrategy.Received(1).Exists(Arg.Is<FileSystemPathId>(id => id.Path == existingPath));
    }

    [Fact]
    public void Exists_WhenPathIsHiddenAndIncludeHiddenElementsIsFalse_ShouldReturnFalse()
    {
        // Arrange
        string existingPath = @"C:\ExistingPath";
        FileSystemPathId pathId = _fileSystemPathIdFixture.CreateFileSystemPathId(existingPath);
        _mockPathStrategy.Exists(pathId, false).Returns(false);

        // Act
        bool result = _sut.Exists(existingPath, false);

        // Assert
        result.Should().BeFalse();
        _mockPathStrategy.Received(1).Exists(Arg.Is<FileSystemPathId>(id => id.Path == existingPath), false);
    }

    [Fact]
    public void Exists_WithValidNonExistingPath_ShouldReturnFalse()
    {
        // Arrange
        string nonExistingPath = @"C:\NonExistingPath";
        FileSystemPathId pathId = _fileSystemPathIdFixture.CreateFileSystemPathId(nonExistingPath);
        _mockPathStrategy.Exists(pathId).Returns(false);

        // Act
        bool result = _sut.Exists(nonExistingPath);

        // Assert
        result.Should().BeFalse();
        _mockPathStrategy.Received(1).Exists(Arg.Is<FileSystemPathId>(id => id.Path == nonExistingPath));
    }

    [Fact]
    public void Exists_WithEmptyPath_ShouldReturnFalse()
    {
        // Arrange
        string emptyPath = string.Empty;

        // Act
        bool result = _sut.Exists(emptyPath);

        // Assert
        result.Should().BeFalse();
        _mockPathStrategy.DidNotReceive().Exists(Arg.Any<FileSystemPathId>());
    }

    [Fact]
    public void Exists_WithNullPath_ShouldReturnFalse()
    {
        // Arrange
        string? nullPath = null;

        // Act
        bool result = _sut.Exists(nullPath!);

        // Assert
        result.Should().BeFalse();
        _mockPathStrategy.DidNotReceive().Exists(Arg.Any<FileSystemPathId>());
    }

    [Fact]
    public void CombinePath_WithValidPathAndName_ShouldReturnCombinedPath()
    {
        // Arrange
        string path = @"C:\BaseDir";
        string name = "SubDir";
        FileSystemPathId pathId = _fileSystemPathIdFixture.CreateFileSystemPathId(path);
        FileSystemPathId combinedPathId = _fileSystemPathIdFixture.CreateFileSystemPathId(@"C:\BaseDir\SubDir");
        _mockPathStrategy.CombinePath(pathId, name).Returns(ErrorOrFactory.From(combinedPathId));

        // Act
        ErrorOr<string> result = _sut.CombinePath(path, name);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(@"C:\BaseDir\SubDir");
        _mockPathStrategy.Received(1).CombinePath(Arg.Is<FileSystemPathId>(id => id.Path == path), name);
    }

    [Fact]
    public void CombinePath_WithInvalidPath_ShouldReturnError()
    {
        // Arrange
        string invalidPath = string.Empty;
        string name = "SubDir";

        // Act
        ErrorOr<string> result = _sut.CombinePath(invalidPath, name);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.FileManagement.InvalidPath);
        _mockPathStrategy.DidNotReceive().CombinePath(Arg.Any<FileSystemPathId>(), Arg.Any<string>());
    }

    [Fact]
    public void CombinePath_WhenPathStrategyReturnsError_ShouldPropagateError()
    {
        // Arrange
        string path = @"C:\BaseDir";
        string name = "Invalid*Name";
        FileSystemPathId pathId = _fileSystemPathIdFixture.CreateFileSystemPathId(path);
        _mockPathStrategy.CombinePath(pathId, name).Returns(Errors.FileManagement.InvalidPath);

        // Act
        ErrorOr<string> result = _sut.CombinePath(path, name);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.FileManagement.InvalidPath);
        _mockPathStrategy.Received(1).CombinePath(Arg.Is<FileSystemPathId>(id => id.Path == path), name);
    }

    [Fact]
    public void CombinePath_WithNullName_ShouldReturnError()
    {
        // Arrange
        string path = @"C:\BaseDir";
        string? name = null;

        // Act
        ErrorOr<string> result = _sut.CombinePath(path, name!);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.FileManagement.InvalidPath);
        _mockPathStrategy.DidNotReceive().CombinePath(Arg.Any<FileSystemPathId>(), Arg.Any<string>());
    }

    [Fact]
    public void ParsePath_WithValidPath_ShouldReturnPathSegments()
    {
        // Arrange
        string validPath = @"C:\ValidPath\SubDir";
        FileSystemPathId pathId = _fileSystemPathIdFixture.CreateFileSystemPathId(validPath);
        List<PathSegment> expectedSegments = _pathSegmentFixture.CreateMany();
        _mockPathStrategy.ParsePath(pathId).Returns(ErrorOrFactory.From(expectedSegments.AsEnumerable()));

        // Act
        ErrorOr<IEnumerable<PathSegment>> result = _sut.ParsePath(validPath);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeEquivalentTo(expectedSegments);
        _mockPathStrategy.Received(1).ParsePath(Arg.Is<FileSystemPathId>(id => id.Path == validPath));
    }

    [Fact]
    public void ParsePath_WithInvalidPath_ShouldReturnError()
    {
        // Arrange
        string invalidPath = string.Empty;

        // Act
        ErrorOr<IEnumerable<PathSegment>> result = _sut.ParsePath(invalidPath);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.FileManagement.InvalidPath);
        _mockPathStrategy.DidNotReceive().ParsePath(Arg.Any<FileSystemPathId>());
    }

    [Fact]
    public void ParsePath_WhenPathStrategyReturnsError_ShouldPropagateError()
    {
        // Arrange
        string path = @"C:\InvalidPath\*SubDir";
        FileSystemPathId pathId = _fileSystemPathIdFixture.CreateFileSystemPathId(path);
        _mockPathStrategy.ParsePath(pathId).Returns(Errors.FileManagement.InvalidPath);

        // Act
        ErrorOr<IEnumerable<PathSegment>> result = _sut.ParsePath(path);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.FileManagement.InvalidPath);
        _mockPathStrategy.Received(1).ParsePath(Arg.Is<FileSystemPathId>(id => id.Path == path));
    }

    [Fact]
    public void ParsePath_WithNullPath_ShouldReturnError()
    {
        // Arrange
        string? nullPath = null;

        // Act
        ErrorOr<IEnumerable<PathSegment>> result = _sut.ParsePath(nullPath!);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.FileManagement.InvalidPath);
        _mockPathStrategy.DidNotReceive().ParsePath(Arg.Any<FileSystemPathId>());
    }

    [Fact]
    public void GoUpOneLevel_WithValidPath_ShouldReturnParentPathSegments()
    {
        // Arrange
        string validPath = @"C:\ValidPath\SubDir";
        FileSystemPathId pathId = _fileSystemPathIdFixture.CreateFileSystemPathId(validPath);
        List<PathSegment> expectedSegments = _pathSegmentFixture.CreateMany(2); // Assuming parent path has 2 segments
        _mockPathStrategy.GoUpOneLevel(pathId).Returns(ErrorOrFactory.From(expectedSegments.AsEnumerable()));

        // Act
        ErrorOr<IEnumerable<PathSegment>> result = _sut.GoUpOneLevel(validPath);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeEquivalentTo(expectedSegments);
        _mockPathStrategy.Received(1).GoUpOneLevel(Arg.Is<FileSystemPathId>(id => id.Path == validPath));
    }

    [Fact]
    public void GoUpOneLevel_WithRootPath_ShouldReturnEmptySegmentList()
    {
        // Arrange
        string rootPath = @"C:\";
        FileSystemPathId pathId = _fileSystemPathIdFixture.CreateFileSystemPathId(rootPath);
        List<PathSegment> emptySegmentList = [];
        _mockPathStrategy.GoUpOneLevel(pathId).Returns(ErrorOrFactory.From(emptySegmentList.AsEnumerable()));

        // Act
        ErrorOr<IEnumerable<PathSegment>> result = _sut.GoUpOneLevel(rootPath);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeEmpty();
        _mockPathStrategy.Received(1).GoUpOneLevel(Arg.Is<FileSystemPathId>(id => id.Path == rootPath));
    }

    [Fact]
    public void GoUpOneLevel_WithInvalidPath_ShouldReturnError()
    {
        // Arrange
        string invalidPath = string.Empty;

        // Act
        ErrorOr<IEnumerable<PathSegment>> result = _sut.GoUpOneLevel(invalidPath);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.FileManagement.InvalidPath);
        _mockPathStrategy.DidNotReceive().GoUpOneLevel(Arg.Any<FileSystemPathId>());
    }

    [Fact]
    public void GoUpOneLevel_WhenPathStrategyReturnsError_ShouldPropagateError()
    {
        // Arrange
        string path = @"C:\InvalidPath\*SubDir";
        FileSystemPathId pathId = _fileSystemPathIdFixture.CreateFileSystemPathId(path);
        _mockPathStrategy.GoUpOneLevel(pathId).Returns(Errors.FileManagement.InvalidPath);

        // Act
        ErrorOr<IEnumerable<PathSegment>> result = _sut.GoUpOneLevel(path);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.FileManagement.InvalidPath);
        _mockPathStrategy.Received(1).GoUpOneLevel(Arg.Is<FileSystemPathId>(id => id.Path == path));
    }

    [Fact]
    public void GoUpOneLevel_WithNullPath_ShouldReturnError()
    {
        // Arrange
        string? nullPath = null;

        // Act
        ErrorOr<IEnumerable<PathSegment>> result = _sut.GoUpOneLevel(nullPath!);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.FileManagement.InvalidPath);
        _mockPathStrategy.DidNotReceive().GoUpOneLevel(Arg.Any<FileSystemPathId>());
    }

    [Fact]
    public void GetInvalidPathCharsForPlatform_WhenCalled_ShouldReturnInvalidCharsFromPathStrategy()
    {
        // Arrange
        char[] expectedInvalidChars = ['<', '>', ':', '"', '|', '?', '*'];
        _mockPathStrategy.GetInvalidPathCharsForPlatform().Returns(expectedInvalidChars);

        // Act
        char[] result = _sut.GetInvalidPathCharsForPlatform();

        // Assert
        result.Should().BeEquivalentTo(expectedInvalidChars);
        _mockPathStrategy.Received(1).GetInvalidPathCharsForPlatform();
    }

    [Fact]
    public void GetInvalidPathCharsForPlatform_WhenPathStrategyReturnsEmptyArray_ShouldReturnEmptyArray()
    {
        // Arrange
        char[] emptyArray = [];
        _mockPathStrategy.GetInvalidPathCharsForPlatform().Returns(emptyArray);

        // Act
        char[] result = _sut.GetInvalidPathCharsForPlatform();

        // Assert
        result.Should().BeEmpty();
        _mockPathStrategy.Received(1).GetInvalidPathCharsForPlatform();
    }

    [Fact]
    public void GetInvalidPathCharsForPlatform_WhenReturnedArrayIsModified_ShouldNotAffectSubsequentCalls()
    {
        // Arrange
        char[] originalInvalidChars = ['<', '>', ':', '"', '|', '?', '*'];
        _mockPathStrategy.GetInvalidPathCharsForPlatform().Returns(originalInvalidChars);

        // Act
        char[] result = _sut.GetInvalidPathCharsForPlatform();
        result[0] = 'X'; // Attempt to modify the returned array

        // Assert
        char[] secondResult = _sut.GetInvalidPathCharsForPlatform();
        secondResult.Should().BeEquivalentTo(originalInvalidChars);
        _mockPathStrategy.Received(2).GetInvalidPathCharsForPlatform();
    }

    [Fact]
    public void GetPathRoot_WhenCalledWithValidPath_ShouldReturnRootPathSegment()
    {
        // Arrange
        string validPath = @"C:\ValidPath\SubDir";
        FileSystemPathId pathId = _fileSystemPathIdFixture.CreateFileSystemPathId(validPath);
        PathSegment expectedRootSegment = _pathSegmentFixture.CreatePathSegment(name: "C:", isDirectory: false, isDrive: true);
        _mockPathStrategy.GetPathRoot(pathId).Returns(ErrorOrFactory.From(expectedRootSegment));

        // Act
        ErrorOr<PathSegment> result = _sut.GetPathRoot(validPath);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeEquivalentTo(expectedRootSegment);
        _mockPathStrategy.Received(1).GetPathRoot(Arg.Is<FileSystemPathId>(id => id.Path == validPath));
    }

    [Fact]
    public void GetPathRoot_WhenCalledWithInvalidPath_ShouldReturnError()
    {
        // Arrange
        string invalidPath = string.Empty;

        // Act
        ErrorOr<PathSegment> result = _sut.GetPathRoot(invalidPath);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.FileManagement.InvalidPath);
        _mockPathStrategy.DidNotReceive().GetPathRoot(Arg.Any<FileSystemPathId>());
    }

    [Fact]
    public void GetPathRoot_WhenPathStrategyReturnsError_ShouldPropagateError()
    {
        // Arrange
        string path = @"C:\InvalidPath\*SubDir";
        FileSystemPathId pathId = _fileSystemPathIdFixture.CreateFileSystemPathId(path);
        _mockPathStrategy.GetPathRoot(pathId).Returns(Errors.FileManagement.InvalidPath);

        // Act
        ErrorOr<PathSegment> result = _sut.GetPathRoot(path);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.FileManagement.InvalidPath);
        _mockPathStrategy.Received(1).GetPathRoot(Arg.Is<FileSystemPathId>(id => id.Path == path));
    }

    [Fact]
    public void GetPathRoot_WhenCalledWithNullPath_ShouldReturnError()
    {
        // Arrange
        string? nullPath = null;

        // Act
        ErrorOr<PathSegment> result = _sut.GetPathRoot(nullPath!);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.FileManagement.InvalidPath);
        _mockPathStrategy.DidNotReceive().GetPathRoot(Arg.Any<FileSystemPathId>());
    }
}
