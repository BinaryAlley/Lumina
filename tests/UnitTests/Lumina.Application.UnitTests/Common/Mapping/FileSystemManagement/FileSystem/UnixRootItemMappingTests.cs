#region ========================================================================= USING =====================================================================================
using ErrorOr;
using FluentAssertions;
using Lumina.Application.Common.Mapping.FileSystemManagement.FileSystem;
using Lumina.Application.UnitTests.Core.FileSystemManagement.Directories.Fixtures;
using Lumina.Application.UnitTests.Core.FileSystemManagement.Files.Fixtures;
using Lumina.Contracts.Enums.FileSystem;
using Lumina.Contracts.Responses.FileSystemManagement.Common;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Entities;
using System.Collections.Generic;
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
        ErrorOr<UnixRootItem> createResult = UnixRootItem.Create();
        createResult.IsError.Should().BeFalse();
        UnixRootItem domainModel = createResult.Value;

        // Act
        FileSystemTreeNodeResponse result = domainModel.ToTreeNodeResponse();

        // Assert
        result.Should().NotBeNull();
        result.Path.Should().Be(domainModel.Id.Path);
        result.Name.Should().Be(domainModel.Name);
        result.ItemType.Should().Be(FileSystemItemType.Root);
        result.IsExpanded.Should().BeFalse();
        result.ChildrenLoaded.Should().BeFalse();
        result.Children.Should().NotBeNull();
        result.Children.Should().BeEmpty();
    }
}
