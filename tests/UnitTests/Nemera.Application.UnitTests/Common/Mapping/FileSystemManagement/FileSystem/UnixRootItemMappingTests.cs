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
/// Contains unit tests for the <see cref="UnixRootItemMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UnixRootItemMappingTests
{
    private readonly UnixRootItemFixture _unixRootItemFixture = new();

    [Fact]
    public void ToTreeNodeResponse_WhenMappingUnixRootItem_ShouldMapCorrectly()
    {
        // Arrange
        UnixRootItem domainModel = _unixRootItemFixture.Create();

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
