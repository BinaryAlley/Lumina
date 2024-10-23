#region ========================================================================= USING =====================================================================================
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using ErrorOr;
using FluentAssertions;
using Lumina.Contracts.Enums.FileSystem;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.Aggregates.FileSystemManagement.FileSystemManagementAggregate;
using Lumina.Domain.Core.Aggregates.FileSystemManagement.FileSystemManagementAggregate.ValueObjects;
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
        item.Id.Should().Be(id);
        item.Name.Should().Be(name);
        item.Type.Should().Be(type);
        item.Status.Should().Be(FileSystemItemStatus.Accessible);
        item.Parent.Should().Be(Optional<FileSystemItem>.None());
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
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(Result.Updated);
        item.Status.Should().Be(newStatus);
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
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(Result.Updated);
        item.Parent.Should().Be(Optional<FileSystemItem>.Some(parent));
    }

    [Fact]
    public void SetParent_WithNullParent_ShouldReturnError()
    {
        // Arrange
        FileSystemItemFixture item = CreateFileSystemItemFixture();

        // Act
        ErrorOr<Updated> result = item.SetParent(null!);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.FileSystemManagement.ParentNodeCannotBeNull);
        item.Parent.Should().Be(Optional<FileSystemItem>.None());
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
        result.Should().BeTrue();
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
        result.Should().BeFalse();
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
        hashCode1.Should().Be(hashCode2);
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
        hashCode1.Should().NotBe(hashCode2);
    }

    private FileSystemItemFixture CreateFileSystemItemFixture(FileSystemPathId? id = null)
    {
        id ??= _fileSystemPathIdFixture.CreateFileSystemPathId();
        string name = _fixture.Create<string>();
        FileSystemItemType type = _fixture.Create<FileSystemItemType>();
        return new FileSystemItemFixture(id, name, type);
    }
}
