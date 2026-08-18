#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Entities;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Entities;
using Lumina.Domain.Fixtures.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.FileSystem;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Entities;

/// <summary>
/// Contains unit tests for the <see cref="WindowsRootItem"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class WindowsRootItemTests
{
    private readonly FileSystemPathIdFixture _fileSystemPathIdFixture = new();
    private readonly WindowsRootItemFixture _windowsRootItemFixture = new();

    [Fact]
    public void Create_WhenCalledWithValidParameters_ShouldReturnSuccessfulResult()
    {
        // Arrange
        string path = "C:\\";
        string name = "C:";

        // Act
        Result<WindowsRootItem> result = WindowsRootItem.Create(path, name);

        // Assert
        Assert.False(result.IsFailure);
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
        Result<WindowsRootItem> result = WindowsRootItem.Create(invalidPath, name);

        // Assert
        Assert.True(result.IsFailure);
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
        Result<WindowsRootItem> result = WindowsRootItem.Create(path, name, customStatus);

        // Assert
        Assert.False(result.IsFailure);
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
        FileSystemPathId pathId = _fileSystemPathIdFixture.Create(path: "E:\\");
        string name = "E:";

        // Act
        Result<WindowsRootItem> result = WindowsRootItem.Create(pathId, name);

        // Assert
        Assert.False(result.IsFailure);
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
        WindowsRootItem windowsRootItem = _windowsRootItemFixture.Create();

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
        WindowsRootItem windowsRootItem = _windowsRootItemFixture.Create();
        FileSystemItemStatus newStatus = FileSystemItemStatus.Accessible;

        // Act
        Result<Updated> result = windowsRootItem.SetStatus(newStatus);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(newStatus, windowsRootItem.Status);
    }

    [Fact]
    public void SetParent_WhenCalledWithNullParent_ShouldReturnError()
    {
        // Arrange
        WindowsRootItem windowsRootItem = _windowsRootItemFixture.Create();

        // Act
        Result<Updated> result = windowsRootItem.SetParent(null!);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.FileSystemManagement.ParentNodeCannotBeNull, result.FirstError);
        Assert.False(windowsRootItem.Parent.HasValue);
    }
}
