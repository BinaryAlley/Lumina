#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.FileSystemManagement.Common;
using Lumina.Application.UnitTests.Core.FileSystemManagement.Directories.Fixtures;
using Lumina.Contracts.Responses.FileSystemManagement.Common;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Entities;
using Lumina.Domain.SharedKernel.Common.Enums.FileSystem;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.FileSystemManagement.Common;

/// <summary>
/// Contains unit tests for the <see cref="FileSystemTreeNodeResponseMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class FileSystemTreeNodeResponseMappingTests
{
    private readonly FileSystemTreeNodeResponseFixture _fileSystemTreeNodeResponseFixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileSystemTreeNodeResponseMappingTests"/> class.
    /// </summary>
    public FileSystemTreeNodeResponseMappingTests()
    {
        _fileSystemTreeNodeResponseFixture = new();
    }

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
