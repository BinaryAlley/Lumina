#region ========================================================================= USING =====================================================================================
using ErrorOr;
using FluentAssertions;
using Lumina.Contracts.Enums.FileSystem;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate;
using Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.Entities;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Core.Aggregates.FileManagementAggregate.Entities;

/// <summary>
/// Contains unit tests for the <see cref="UnixRootItem"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UnixRootItemTests
{
    [Fact]
    public void Create_WhenCalledWithDefaultStatus_ShouldReturnSuccessfulResult()
    {
        // Arrange & Act
        ErrorOr<UnixRootItem> result = UnixRootItem.Create();

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be("/");
        result.Value.Id.Path.Should().Be("/");
        result.Value.Type.Should().Be(FileSystemItemType.Root);
        result.Value.Status.Should().Be(FileSystemItemStatus.Accessible);
        result.Value.Items.Should().BeEmpty();
    }

    [Fact]
    public void Create_WhenCalledWithCustomStatus_ShouldReturnSuccessfulResultWithSpecifiedStatus()
    {
        // Arrange
        FileSystemItemStatus customStatus = FileSystemItemStatus.Accessible;

        // Act
        ErrorOr<UnixRootItem> result = UnixRootItem.Create(customStatus);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be("/");
        result.Value.Id.Path.Should().Be("/");
        result.Value.Type.Should().Be(FileSystemItemType.Root);
        result.Value.Status.Should().Be(customStatus);
        result.Value.Items.Should().BeEmpty();
    }

    [Fact]
    public void Items_WhenAccessed_ShouldReturnEmptyReadOnlyCollection()
    {
        // Arrange
        ErrorOr<UnixRootItem> createResult = UnixRootItem.Create();
        createResult.IsError.Should().BeFalse();
        UnixRootItem unixRootItem = createResult.Value;

        // Act
        IReadOnlyCollection<FileSystemItem> items = unixRootItem.Items;

        // Assert
        items.Should().BeEmpty();
        items.Should().BeAssignableTo<IReadOnlyCollection<FileSystemItem>>();
    }

    [Fact]
    public void SetStatus_WhenCalledWithNewStatus_ShouldUpdateStatus()
    {
        // Arrange
        ErrorOr<UnixRootItem> createResult = UnixRootItem.Create();
        createResult.IsError.Should().BeFalse();
        UnixRootItem unixRootItem = createResult.Value;
        FileSystemItemStatus newStatus = FileSystemItemStatus.Accessible;

        // Act
        ErrorOr<Updated> result = unixRootItem.SetStatus(newStatus);

        // Assert
        result.IsError.Should().BeFalse();
        unixRootItem.Status.Should().Be(newStatus);
    }

    [Fact]
    public void SetParent_WhenCalledWithNullParent_ShouldReturnError()
    {
        // Arrange
        ErrorOr<UnixRootItem> createResult = UnixRootItem.Create();
        createResult.IsError.Should().BeFalse();
        UnixRootItem unixRootItem = createResult.Value;
        
        // Act
        ErrorOr<Updated> result = unixRootItem.SetParent(null!);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.FileManagement.ParentNodeCannotBeNull);
        unixRootItem.Parent.HasValue.Should().BeFalse();
    }
}
