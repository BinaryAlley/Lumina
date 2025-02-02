#region ========================================================================= USING =====================================================================================
using ErrorOr;
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
        Assert.False(createResult.IsError);
        WindowsRootItem domainModel = createResult.Value;

        // Act
        FileSystemTreeNodeResponse result = domainModel.ToTreeNodeResponse();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(domainModel.Id.Path, result.Path);
        Assert.Equal(domainModel.Name, result.Name);
        Assert.Equal(FileSystemItemType.Root, result.ItemType);
        Assert.False(result.IsExpanded);
        Assert.False(result.ChildrenLoaded);
        Assert.Empty(result.Children);
    }

    [Fact]
    public void ToTreeNodeResponse_WhenMappingWindowsRootItemWithCustomStatus_ShouldMapCorrectly()
    {
        // Arrange
        string path = "D:\\";
        string name = "D:";
        FileSystemItemStatus customStatus = FileSystemItemStatus.Inaccessible;
        ErrorOr<WindowsRootItem> createResult = WindowsRootItem.Create(path, name, customStatus);
        Assert.False(createResult.IsError);
        WindowsRootItem domainModel = createResult.Value;

        // Act
        FileSystemTreeNodeResponse result = domainModel.ToTreeNodeResponse();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(domainModel.Id.Path, result.Path);
        Assert.Equal(domainModel.Name, result.Name);
        Assert.Equal(FileSystemItemType.Root, result.ItemType);
        Assert.False(result.IsExpanded);
        Assert.False(result.ChildrenLoaded);
        Assert.Empty(result.Children);
    }
}
