#region ========================================================================= USING =====================================================================================
using ErrorOr;
using FluentAssertions;
using Lumina.Contracts.Enums.FileSystem;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Core.Aggregates.FileSystemManagement.FileSystemManagementAggregate;
using Lumina.Domain.Core.Aggregates.FileSystemManagement.FileSystemManagementAggregate.Entities;
using Lumina.Domain.Core.Aggregates.FileSystemManagement.FileSystemManagementAggregate.ValueObjects;
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
        result.IsError.Should().BeFalse();
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be(name);
        result.Value.Id.Path.Should().Be(path);
        result.Value.Type.Should().Be(FileSystemItemType.Root);
        result.Value.Status.Should().Be(FileSystemItemStatus.Accessible);
        result.Value.Items.Should().BeEmpty();
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
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.FileSystemManagement.InvalidPath);
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
        result.IsError.Should().BeFalse();
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be(name);
        result.Value.Id.Path.Should().Be(path);
        result.Value.Type.Should().Be(FileSystemItemType.Root);
        result.Value.Status.Should().Be(customStatus);
    }

    [Fact]
    public void Create_WhenCalledWithFileSystemPathId_ShouldReturnSuccessfulResult()
    {
        // Arrange
        ErrorOr<FileSystemPathId> pathIdResult = FileSystemPathId.Create("E:\\");
        pathIdResult.IsError.Should().BeFalse();
        FileSystemPathId pathId = pathIdResult.Value;
        string name = "E:";

        // Act
        ErrorOr<WindowsRootItem> result = WindowsRootItem.Create(pathId, name);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().Be(pathId);
        result.Value.Name.Should().Be(name);
        result.Value.Type.Should().Be(FileSystemItemType.Root);
        result.Value.Status.Should().Be(FileSystemItemStatus.Accessible);
    }

    [Fact]
    public void Items_WhenAccessed_ShouldReturnEmptyReadOnlyCollection()
    {
        // Arrange
        ErrorOr<WindowsRootItem> createResult = WindowsRootItem.Create("F:\\", "F:");
        createResult.IsError.Should().BeFalse();
        WindowsRootItem windowsRootItem = createResult.Value;

        // Act
        IReadOnlyCollection<FileSystemItem> items = windowsRootItem.Items;

        // Assert
        items.Should().BeEmpty();
        items.Should().BeAssignableTo<IReadOnlyCollection<FileSystemItem>>();
    }

    [Fact]
    public void SetStatus_WhenCalledWithNewStatus_ShouldUpdateStatus()
    {
        // Arrange
        ErrorOr<WindowsRootItem> createResult = WindowsRootItem.Create("G:\\", "G:");
        createResult.IsError.Should().BeFalse();
        WindowsRootItem windowsRootItem = createResult.Value;
        FileSystemItemStatus newStatus = FileSystemItemStatus.Accessible;

        // Act
        ErrorOr<Updated> result = windowsRootItem.SetStatus(newStatus);

        // Assert
        result.IsError.Should().BeFalse();
        windowsRootItem.Status.Should().Be(newStatus);
    }

    [Fact]
    public void SetParent_WhenCalledWithNullParent_ShouldReturnError()
    {
        // Arrange
        ErrorOr<WindowsRootItem> createResult = WindowsRootItem.Create("H:\\", "H:");
        createResult.IsError.Should().BeFalse();
        WindowsRootItem windowsRootItem = createResult.Value;
        
        // Act
        ErrorOr<Updated> result = windowsRootItem.SetParent(null!);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.FileSystemManagement.ParentNodeCannotBeNull);
        windowsRootItem.Parent.HasValue.Should().BeFalse();
    }
}
