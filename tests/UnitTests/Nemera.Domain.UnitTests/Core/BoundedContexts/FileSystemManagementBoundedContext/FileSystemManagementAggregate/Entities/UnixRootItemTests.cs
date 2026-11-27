#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Entities;
using Lumina.Domain.Fixtures.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Entities;
using Lumina.Domain.SharedKernel.Common.Enums.FileSystem;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Entities;

/// <summary>
/// Contains unit tests for the <see cref="UnixRootItem"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UnixRootItemTests
{
    private readonly UnixRootItemFixture _unixRootItemFixture = new();

    [Fact]
    public void Create_WhenCalledWithDefaultStatus_ShouldReturnSuccessfulResult()
    {
        // Arrange & Act
        Result<UnixRootItem> result = UnixRootItem.Create();

        // Assert
        Assert.False(result.IsFailure);
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
        Result<UnixRootItem> result = UnixRootItem.Create(customStatus);

        // Assert
        Assert.False(result.IsFailure);
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
        UnixRootItem unixRootItem = _unixRootItemFixture.Create();

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
        UnixRootItem unixRootItem = _unixRootItemFixture.Create();
        FileSystemItemStatus newStatus = FileSystemItemStatus.Accessible;

        // Act
        Result<Updated> result = unixRootItem.SetStatus(newStatus);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(newStatus, unixRootItem.Status);
    }

    [Fact]
    public void SetParent_WhenCalledWithNullParent_ShouldReturnError()
    {
        // Arrange
        UnixRootItem unixRootItem = _unixRootItemFixture.Create();

        // Act
        Result<Updated> result = unixRootItem.SetParent(null!);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.FileSystemManagement.ParentNodeCannotBeNull, result.FirstError);
        Assert.False(unixRootItem.Parent.HasValue);
    }
}
