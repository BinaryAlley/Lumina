#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Domain.Common.Enums.FileSystem;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Entities;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.ValueObjects;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Core.Aggregates.FileSystemManagementAggregate.Entities;

/// <summary>
/// Contains unit tests for the <see cref="WindowsRootItem"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class WindowsRootItemTests
{
    [Fact]
    public void Create_WhenCalledWithValidParameters_ShouldReturnSuccessfulResult()
    {
        // Arrange
        string path = "C:\\";
        string name = "C:";

        // Act
        ErrorOr<WindowsRootItem> result = WindowsRootItem.Create(path, name);

        // Assert
        Assert.False(result.IsError);
        Assert.NotNull(result.Value);
        Assert.Equal(name, result.Value.Name);
        Assert.Equal(path, result.Value.Id.Path);
        Assert.Equal(FileSystemItemType.Root, result.Value.Type);
        Assert.Equal(FileSystemItemStatus.Accessible, result.Value.Status);
        Assert.Empty(result.Value.Items);
    }

    [Fact]
    public void Create_WhenCalledWithInvalidPath_ShouldReturnError()
    {
        // Arrange
        string invalidPath = "";
        string name = "Invalid";

        // Act
        ErrorOr<WindowsRootItem> result = WindowsRootItem.Create(invalidPath, name);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(Errors.FileSystemManagement.InvalidPath, result.FirstError);
    }

    [Fact]
    public void Create_WhenCalledWithCustomStatus_ShouldReturnSuccessfulResultWithSpecifiedStatus()
    {
        // Arrange
        string path = "D:\\";
        string name = "D:";
        FileSystemItemStatus customStatus = FileSystemItemStatus.Accessible;

        // Act
        ErrorOr<WindowsRootItem> result = WindowsRootItem.Create(path, name, customStatus);

        // Assert
        Assert.False(result.IsError);
        Assert.NotNull(result.Value);
        Assert.Equal(name, result.Value.Name);
        Assert.Equal(path, result.Value.Id.Path);
        Assert.Equal(FileSystemItemType.Root, result.Value.Type);
        Assert.Equal(customStatus, result.Value.Status);
    }

    [Fact]
    public void Create_WhenCalledWithFileSystemPathId_ShouldReturnSuccessfulResult()
    {
        // Arrange
        ErrorOr<FileSystemPathId> pathIdResult = FileSystemPathId.Create("E:\\");
        Assert.False(pathIdResult.IsError);
        FileSystemPathId pathId = pathIdResult.Value;
        string name = "E:";

        // Act
        ErrorOr<WindowsRootItem> result = WindowsRootItem.Create(pathId, name);

        // Assert
        Assert.False(result.IsError);
        Assert.NotNull(result.Value);
        Assert.Equal(pathId, result.Value.Id);
        Assert.Equal(name, result.Value.Name);
        Assert.Equal(FileSystemItemType.Root, result.Value.Type);
        Assert.Equal(FileSystemItemStatus.Accessible, result.Value.Status);
    }

    [Fact]
    public void Items_WhenAccessed_ShouldReturnEmptyReadOnlyCollection()
    {
        // Arrange
        ErrorOr<WindowsRootItem> createResult = WindowsRootItem.Create("F:\\", "F:");
        Assert.False(createResult.IsError);
        WindowsRootItem windowsRootItem = createResult.Value;

        // Act
        IReadOnlyCollection<FileSystemItem> items = windowsRootItem.Items;

        // Assert
        Assert.Empty(items);
        Assert.IsAssignableFrom<IReadOnlyCollection<FileSystemItem>>(items);
    }

    [Fact]
    public void SetStatus_WhenCalledWithNewStatus_ShouldUpdateStatus()
    {
        // Arrange
        ErrorOr<WindowsRootItem> createResult = WindowsRootItem.Create("G:\\", "G:");
        Assert.False(createResult.IsError);
        WindowsRootItem windowsRootItem = createResult.Value;
        FileSystemItemStatus newStatus = FileSystemItemStatus.Accessible;

        // Act
        ErrorOr<Updated> result = windowsRootItem.SetStatus(newStatus);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(newStatus, windowsRootItem.Status);
    }

    [Fact]
    public void SetParent_WhenCalledWithNullParent_ShouldReturnError()
    {
        // Arrange
        ErrorOr<WindowsRootItem> createResult = WindowsRootItem.Create("H:\\", "H:");
        Assert.False(createResult.IsError);
        WindowsRootItem windowsRootItem = createResult.Value;
        
        // Act
        ErrorOr<Updated> result = windowsRootItem.SetParent(null!);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(Errors.FileSystemManagement.ParentNodeCannotBeNull, result.FirstError);
        Assert.False(windowsRootItem.Parent.HasValue);
    }
}
