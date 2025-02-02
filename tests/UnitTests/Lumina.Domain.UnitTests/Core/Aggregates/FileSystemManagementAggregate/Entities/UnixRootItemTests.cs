#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Domain.Common.Enums.FileSystem;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Entities;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Core.Aggregates.FileSystemManagementAggregate.Entities;

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
        Assert.False(result.IsError);
        Assert.NotNull(result.Value);
        Assert.Equal("/", result.Value.Name);
        Assert.Equal("/", result.Value.Id.Path);
        Assert.Equal(FileSystemItemType.Root, result.Value.Type);
        Assert.Equal(FileSystemItemStatus.Accessible, result.Value.Status);
        Assert.Empty(result.Value.Items);
    }

    [Fact]
    public void Create_WhenCalledWithCustomStatus_ShouldReturnSuccessfulResultWithSpecifiedStatus()
    {
        // Arrange
        FileSystemItemStatus customStatus = FileSystemItemStatus.Accessible;

        // Act
        ErrorOr<UnixRootItem> result = UnixRootItem.Create(customStatus);

        // Assert
        Assert.False(result.IsError);
        Assert.NotNull(result.Value);
        Assert.Equal("/", result.Value.Name);
        Assert.Equal("/", result.Value.Id.Path);
        Assert.Equal(FileSystemItemType.Root, result.Value.Type);
        Assert.Equal(customStatus, result.Value.Status);
        Assert.Empty(result.Value.Items);
    }

    [Fact]
    public void Items_WhenAccessed_ShouldReturnEmptyReadOnlyCollection()
    {
        // Arrange
        ErrorOr<UnixRootItem> createResult = UnixRootItem.Create();
        Assert.False(createResult.IsError);
        UnixRootItem unixRootItem = createResult.Value;

        // Act
        IReadOnlyCollection<FileSystemItem> items = unixRootItem.Items;

        // Assert
        Assert.Empty(items);
        Assert.IsAssignableFrom<IReadOnlyCollection<FileSystemItem>>(items);
    }

    [Fact]
    public void SetStatus_WhenCalledWithNewStatus_ShouldUpdateStatus()
    {
        // Arrange
        ErrorOr<UnixRootItem> createResult = UnixRootItem.Create();
        Assert.False(createResult.IsError);
        UnixRootItem unixRootItem = createResult.Value;
        FileSystemItemStatus newStatus = FileSystemItemStatus.Accessible;

        // Act
        ErrorOr<Updated> result = unixRootItem.SetStatus(newStatus);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(newStatus, unixRootItem.Status);
    }

    [Fact]
    public void SetParent_WhenCalledWithNullParent_ShouldReturnError()
    {
        // Arrange
        ErrorOr<UnixRootItem> createResult = UnixRootItem.Create();
        Assert.False(createResult.IsError);
        UnixRootItem unixRootItem = createResult.Value;

        // Act
        ErrorOr<Updated> result = unixRootItem.SetParent(null!);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(Errors.FileSystemManagement.ParentNodeCannotBeNull, result.FirstError);
        Assert.False(unixRootItem.Parent.HasValue);
    }
}
