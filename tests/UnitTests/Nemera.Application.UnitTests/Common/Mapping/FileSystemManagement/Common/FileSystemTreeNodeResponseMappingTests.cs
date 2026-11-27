#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.Mapping.FileSystemManagement.Common;
using Nemera.Contracts.Fixtures.Core.Responses.FileSystemManagement.Common;
using Nemera.Contracts.Responses.FileSystemManagement.Common;
using Nemera.Domain.Common.Primitives;
using Nemera.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Entities;
using Nemera.Domain.SharedKernel.Common.Enums.FileSystem;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Nemera.Application.UnitTests.Common.Mapping.FileSystemManagement.Common;

/// <summary>
/// Contains unit tests for the <see cref="FileSystemTreeNodeResponseMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class FileSystemTreeNodeResponseMappingTests
{
    private readonly FileSystemTreeNodeResponseFixture _fileSystemTreeNodeResponseFixture = new();

    [Fact]
    public void ToWindowsRootItem_WhenMappingFileSystemTreeNodeResponse_ShouldMapCorrectly()
    {
        // Arrange
        FileSystemTreeNodeResponse response = _fileSystemTreeNodeResponseFixture.Create();

        // Act
        Result<WindowsRootItem> result = response.ToWindowsRootItem();

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.Equal(response.Path, result.Value.Id.Path);
        Assert.Equal(response.Name, result.Value.Name);
        Assert.Equal(FileSystemItemStatus.Accessible, result.Value.Status);
    }

    [Fact]
    public void ToUnixRootItem_WhenMappingFileSystemTreeNodeResponse_ShouldMapCorrectly()
    {
        // Arrange
        FileSystemTreeNodeResponse response = _fileSystemTreeNodeResponseFixture.Create();

        // Act
        Result<UnixRootItem> result = response.ToUnixRootItem();

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.Equal(FileSystemItemStatus.Accessible, result.Value.Status);
    }
}
