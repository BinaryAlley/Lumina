#region ========================================================================= USING =====================================================================================
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using ErrorOr;
using Lumina.Domain.Common.Enums.FileSystem;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.ValueObjects;
using Lumina.Domain.UnitTests.Core.Aggregates.FileSystemManagementAggregate.Fixtures;
using Lumina.Domain.UnitTests.Core.Aggregates.FileSystemManagementAggregate.ValueObjects.Fixtures;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Core.Aggregates.FileSystemManagementAggregate;

/// <summary>
/// Contains unit tests for the <see cref="FileSystemItem"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class FileSystemItemTests
{
    private readonly IFixture _fixture;
    private readonly FileSystemPathIdFixture _fileSystemPathIdFixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileSystemItemTests"/> class.
    /// </summary>
    public FileSystemItemTests()
    {
        _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
        _fileSystemPathIdFixture = new FileSystemPathIdFixture();
    }

    [Fact]
    public void Constructor_WhenCalled_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        FileSystemPathId id = _fileSystemPathIdFixture.CreateFileSystemPathId();
        string name = "TestItem";
        FileSystemItemType type = FileSystemItemType.File;

        // Act
        FileSystemItemFixture item = new(id, name, type);

        // Assert
        Assert.Equal(id, item.Id);
        Assert.Equal(name, item.Name);
        Assert.Equal(type, item.Type);
        Assert.Equal(FileSystemItemStatus.Accessible, item.Status);
        Assert.Equal(Optional<FileSystemItem>.None(), item.Parent);
    }

    [Fact]
    public void SetStatus_WhenCalled_ShouldUpdateStatusAndReturnUpdated()
    {
        // Arrange
        FileSystemItemFixture item = CreateFileSystemItemFixture();
        FileSystemItemStatus newStatus = FileSystemItemStatus.Inaccessible;

        // Act
        ErrorOr<Updated> result = item.SetStatus(newStatus);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(Result.Updated, result.Value);
        Assert.Equal(newStatus, item.Status);
    }

    [Fact]
    public void SetParent_WithValidParent_ShouldUpdateParentAndReturnUpdated()
    {
        // Arrange
        FileSystemItemFixture item = CreateFileSystemItemFixture();
        FileSystemItemFixture parent = CreateFileSystemItemFixture();

        // Act
        ErrorOr<Updated> result = item.SetParent(parent);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(Result.Updated, result.Value);
        Assert.Equal(Optional<FileSystemItem>.Some(parent), item.Parent);
    }

    [Fact]
    public void SetParent_WithNullParent_ShouldReturnError()
    {
        // Arrange
        FileSystemItemFixture item = CreateFileSystemItemFixture();

        // Act
        ErrorOr<Updated> result = item.SetParent(null!);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(Errors.FileSystemManagement.ParentNodeCannotBeNull, result.FirstError);
        Assert.Equal(Optional<FileSystemItem>.None(), item.Parent);
    }

    [Fact]
    public void Equals_WithSameId_ShouldReturnTrue()
    {
        // Arrange
        FileSystemPathId id = _fileSystemPathIdFixture.CreateFileSystemPathId();
        FileSystemItemFixture item1 = CreateFileSystemItemFixture(id);
        FileSystemItemFixture item2 = CreateFileSystemItemFixture(id);

        // Act
        bool result = item1.Equals(item2);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Equals_WithDifferentId_ShouldReturnFalse()
    {
        // Arrange
        FileSystemItemFixture item1 = CreateFileSystemItemFixture();
        FileSystemItemFixture item2 = CreateFileSystemItemFixture();

        // Act
        bool result = item1.Equals(item2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetHashCode_WithSameId_ShouldReturnSameHashCode()
    {
        // Arrange
        FileSystemPathId id = _fileSystemPathIdFixture.CreateFileSystemPathId();
        FileSystemItemFixture item1 = CreateFileSystemItemFixture(id);
        FileSystemItemFixture item2 = CreateFileSystemItemFixture(id);

        // Act
        int hashCode1 = item1.GetHashCode();
        int hashCode2 = item2.GetHashCode();

        // Assert
        Assert.Equal(hashCode1, hashCode2);
    }

    [Fact]
    public void GetHashCode_WithDifferentId_ShouldReturnDifferentHashCode()
    {
        // Arrange
        FileSystemItemFixture item1 = CreateFileSystemItemFixture();
        FileSystemItemFixture item2 = CreateFileSystemItemFixture();

        // Act
        int hashCode1 = item1.GetHashCode();
        int hashCode2 = item2.GetHashCode();

        // Assert
        Assert.NotEqual(hashCode1, hashCode2);
    }

    private FileSystemItemFixture CreateFileSystemItemFixture(FileSystemPathId? id = null)
    {
        id ??= _fileSystemPathIdFixture.CreateFileSystemPathId();
        string name = _fixture.Create<string>();
        FileSystemItemType type = _fixture.Create<FileSystemItemType>();
        return new FileSystemItemFixture(id, name, type);
    }
}
