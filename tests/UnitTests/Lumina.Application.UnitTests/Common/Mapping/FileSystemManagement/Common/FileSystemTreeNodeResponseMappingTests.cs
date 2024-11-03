#region ========================================================================= USING =====================================================================================
using ErrorOr;
using FluentAssertions;
using Lumina.Application.Common.Mapping.FileSystemManagement.Common;
using Lumina.Application.UnitTests.Core.FileSystemManagement.Directories.Fixtures;
using Lumina.Contracts.Enums.FileSystem;
using Lumina.Contracts.Responses.FileSystemManagement.Common;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Entities;
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
        ErrorOr<WindowsRootItem> result = response.ToWindowsRootItem();

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().NotBeNull();
        result.Value.Id.Path.Should().Be(response.Path);
        result.Value.Name.Should().Be(response.Name);
        result.Value.Status.Should().Be(FileSystemItemStatus.Accessible);
    }

    [Fact]
    public void ToUnixRootItem_WhenMappingFileSystemTreeNodeResponse_ShouldMapCorrectly()
    {
        // Arrange
        FileSystemTreeNodeResponse response = _fileSystemTreeNodeResponseFixture.Create();

        // Act
        ErrorOr<UnixRootItem> result = response.ToUnixRootItem();

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().NotBeNull();
        result.Value.Status.Should().Be(FileSystemItemStatus.Accessible);
    }
}
