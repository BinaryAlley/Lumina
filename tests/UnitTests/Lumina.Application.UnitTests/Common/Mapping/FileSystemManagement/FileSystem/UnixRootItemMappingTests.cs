#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.FileSystemManagement.FileSystem;
using Lumina.Contracts.Responses.FileSystemManagement.Common;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Entities;
using Lumina.Domain.SharedKernel.Common.Enums.FileSystem;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.FileSystemManagement.FileSystem;

/// <summary>
/// Contains unit tests for the <see cref="UnixRootItemMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UnixRootItemMappingTests
{
    [Fact]
    public void ToTreeNodeResponse_WhenMappingUnixRootItem_ShouldMapCorrectly()
    {
        // Arrange
        Result<UnixRootItem> createResult = UnixRootItem.Create();
        Assert.False(createResult.IsFailure);
        UnixRootItem domainModel = createResult.Value;

        // Act
        FileSystemTreeNodeResponse result = domainModel.ToTreeNodeResponse();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(domainModel.Id.Path, result.Path);
        Assert.Equal(domainModel.Name, result.Name);
        Assert.Equal(FileSystemItemType.Root, result.ItemType);
        Assert.False(result.IsExpanded);
        Assert.False(result.ChildrenLoaded);
        Assert.NotNull(result.Children);
        Assert.Empty(result.Children);
    }
}
