#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.Mapping.FileSystemManagement.FileSystem;
using Nemera.Contracts.Responses.FileSystemManagement.Common;
using Nemera.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Entities;
using Nemera.Domain.Fixtures.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Entities;
using Nemera.Domain.SharedKernel.Common.Enums.FileSystem;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Nemera.Application.UnitTests.Common.Mapping.FileSystemManagement.FileSystem;

/// <summary>
/// Contains unit tests for the <see cref="WindowsRootItemMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class WindowsRootItemMappingTests
{
    private readonly WindowsRootItemFixture _windowsRootItemFixture = new();

    [Fact]
    public void ToTreeNodeResponse_WhenMappingWindowsRootItem_ShouldMapCorrectly()
    {
        // Arrange
        WindowsRootItem domainModel = _windowsRootItemFixture.Create(path: "C:\\", name: "C:");

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
        WindowsRootItem domainModel = _windowsRootItemFixture.Create(path: "D:\\", name: "D:", status: FileSystemItemStatus.Inaccessible);

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
