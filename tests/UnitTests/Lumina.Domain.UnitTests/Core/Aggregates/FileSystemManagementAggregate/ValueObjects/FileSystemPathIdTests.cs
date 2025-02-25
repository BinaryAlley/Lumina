#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.ValueObjects;
using Lumina.Domain.UnitTests.Core.Aggregates.FileSystemManagementAggregate.ValueObjects.Fixtures;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Core.Aggregates.FileSystemManagementAggregate.ValueObjects;

/// <summary>
/// Contains unit tests for the <see cref="FileSystemPathId"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class FileSystemPathIdTests
{
    private readonly FileSystemPathIdFixture _fileSystemPathIdFixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileSystemPathIdTests"/> class.
    /// </summary>
    public FileSystemPathIdTests()
    {
        _fileSystemPathIdFixture = new FileSystemPathIdFixture();
    }

    [Fact]
    public void Create_WithValidPath_ShouldReturnFileSystemPathId()
    {
        // Arrange
        string validPath = @"C:\TestDir\file.txt";

        // Act
        ErrorOr<FileSystemPathId> result = FileSystemPathId.Create(validPath);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(validPath, result.Value.Path);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidPath_ShouldReturnError(string? invalidPath)
    {
        // Act
        ErrorOr<FileSystemPathId> result = FileSystemPathId.Create(invalidPath!);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(Errors.FileSystemManagement.InvalidPath, result.FirstError);
    }

    [Fact]
    public void Equals_WithSamePath_ShouldReturnTrue()
    {
        // Arrange
        string path = @"C:\TestDir\file.txt";
        FileSystemPathId id1 = _fileSystemPathIdFixture.CreateFileSystemPathId(path);
        FileSystemPathId id2 = _fileSystemPathIdFixture.CreateFileSystemPathId(path);

        // Act
        bool result = id1.Equals(id2);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Equals_WithDifferentPath_ShouldReturnFalse()
    {
        // Arrange
        FileSystemPathId id1 = _fileSystemPathIdFixture.CreateFileSystemPathId(@"C:\TestDir\file1.txt");
        FileSystemPathId id2 = _fileSystemPathIdFixture.CreateFileSystemPathId(@"C:\TestDir\file2.txt");

        // Act
        bool result = id1.Equals(id2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetHashCode_WithSamePath_ShouldReturnSameHashCode()
    {
        // Arrange
        string path = @"C:\TestDir\file.txt";
        FileSystemPathId id1 = _fileSystemPathIdFixture.CreateFileSystemPathId(path);
        FileSystemPathId id2 = _fileSystemPathIdFixture.CreateFileSystemPathId(path);

        // Act
        int hashCode1 = id1.GetHashCode();
        int hashCode2 = id2.GetHashCode();

        // Assert
        Assert.Equal(hashCode1, hashCode2);
    }

    [Fact]
    public void GetHashCode_WithDifferentPath_ShouldReturnDifferentHashCode()
    {
        // Arrange
        FileSystemPathId id1 = _fileSystemPathIdFixture.CreateFileSystemPathId(@"C:\TestDir\file1.txt");
        FileSystemPathId id2 = _fileSystemPathIdFixture.CreateFileSystemPathId(@"C:\TestDir\file2.txt");

        // Act
        int hashCode1 = id1.GetHashCode();
        int hashCode2 = id2.GetHashCode();

        // Assert
        Assert.NotEqual(hashCode1, hashCode2);
    }

    [Fact]
    public void EqualityOperator_WithSamePath_ShouldReturnTrue()
    {
        // Arrange
        string path = @"C:\TestDir\file.txt";
        FileSystemPathId id1 = _fileSystemPathIdFixture.CreateFileSystemPathId(path);
        FileSystemPathId id2 = _fileSystemPathIdFixture.CreateFileSystemPathId(path);

        // Act
        bool result = id1 == id2;

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void InequalityOperator_WithDifferentPath_ShouldReturnTrue()
    {
        // Arrange
        FileSystemPathId id1 = _fileSystemPathIdFixture.CreateFileSystemPathId(@"C:\TestDir\file1.txt");
        FileSystemPathId id2 = _fileSystemPathIdFixture.CreateFileSystemPathId(@"C:\TestDir\file2.txt");

        // Act
        bool result = id1 != id2;

        // Assert
        Assert.True(result);
    }
}
