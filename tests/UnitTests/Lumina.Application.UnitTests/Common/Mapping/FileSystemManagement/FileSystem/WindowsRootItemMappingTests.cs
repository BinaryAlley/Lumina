#region ========================================================================= USING =====================================================================================
using ErrorOr;
using FluentAssertions;
using Lumina.Application.Common.Mapping.FileSystemManagement.FileSystem;
using Lumina.Domain.Common.Enums.FileSystem;
using Lumina.Contracts.Responses.FileSystemManagement.Common;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Entities;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.FileSystemManagement.FileSystem;

/// <summary>
/// Contains unit tests for the <see cref="WindowsRootItemMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class WindowsRootItemMappingTests
{
    [Fact]
    public void ToTreeNodeResponse_WhenMappingWindowsRootItem_ShouldMapCorrectly()
    {
        // Arrange
        string path = "C:\\";
        string name = "C:";
        ErrorOr<WindowsRootItem> createResult = WindowsRootItem.Create(path, name);
        createResult.IsError.Should().BeFalse();
        WindowsRootItem domainModel = createResult.Value;

        // Act
        FileSystemTreeNodeResponse result = domainModel.ToTreeNodeResponse();

        // Assert
        result.Should().NotBeNull();
        result.Path.Should().Be(domainModel.Id.Path);
        result.Name.Should().Be(domainModel.Name);
        result.ItemType.Should().Be(FileSystemItemType.Root);
        result.IsExpanded.Should().BeFalse();
        result.ChildrenLoaded.Should().BeFalse();
        result.Children.Should().BeEmpty();
    }

    [Fact]
    public void ToTreeNodeResponse_WhenMappingWindowsRootItemWithCustomStatus_ShouldMapCorrectly()
    {
        // Arrange
        string path = "D:\\";
        string name = "D:";
        FileSystemItemStatus customStatus = FileSystemItemStatus.Inaccessible;
        ErrorOr<WindowsRootItem> createResult = WindowsRootItem.Create(path, name, customStatus);
        createResult.IsError.Should().BeFalse();
        WindowsRootItem domainModel = createResult.Value;

        // Act
        FileSystemTreeNodeResponse result = domainModel.ToTreeNodeResponse();

        // Assert
        result.Should().NotBeNull();
        result.Path.Should().Be(domainModel.Id.Path);
        result.Name.Should().Be(domainModel.Name);
        result.ItemType.Should().Be(FileSystemItemType.Root);
        result.IsExpanded.Should().BeFalse();
        result.ChildrenLoaded.Should().BeFalse();
        result.Children.Should().BeEmpty();
    }
}
