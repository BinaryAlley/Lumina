#region ========================================================================= USING =====================================================================================
using FluentAssertions;
using Lumina.Application.Common.Mapping.FileManagement;
using Lumina.Application.UnitTests.Core.FileManagement.Directories.Fixtures;
using Lumina.Application.UnitTests.Core.FileManagement.Files.Fixtures;
using Lumina.Contracts.Enums.FileSystem;
using Lumina.Contracts.Responses.FileManagement;
using Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.Entities;
using Mapster;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.FileManagement;

/// <summary>
/// Contains unit tests for the <see cref="FileSystemTreeMappingConfig"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class FileSystemTreeMappingConfigTests
{
    #region ================================================================== FIELD MEMBERS ================================================================================
    private readonly TypeAdapterConfig _config;
    private readonly FileSystemTreeMappingConfig _fileSystemTreeMappingConfig;
    #endregion

    #region ====================================================================== CTOR =====================================================================================
    public FileSystemTreeMappingConfigTests()
    {
        _config = new TypeAdapterConfig();
        _fileSystemTreeMappingConfig = new FileSystemTreeMappingConfig();
        _fileSystemTreeMappingConfig.Register(_config);
    }
    #endregion

    #region ===================================================================== METHODS ===================================================================================
    [Fact]
    public void Register_WhenMappingWindowsRootItemToFileSystemTreeNodeResponse_ShouldMapCorrectly()
    {
        // Arrange
        WindowsRootItem windowsRoot = WindowsRootItem.Create("C:", "C:").Value;

        // Act
        FileSystemTreeNodeResponse result = windowsRoot.Adapt<FileSystemTreeNodeResponse>(_config);

        // Assert
        result.Should().NotBeNull();
        result.Path.Should().Be(windowsRoot.Id.Path);
        result.Name.Should().Be(windowsRoot.Name);
        result.ItemType.Should().Be(FileSystemItemType.Root);
        result.IsExpanded.Should().BeFalse();
        result.Children.Should().NotBeNull();
        result.Children.Should().BeEmpty();
    }

    [Fact]
    public void Register_WhenMappingFileSystemTreeNodeResponseToWindowsRootItem_ShouldMapCorrectly()
    {
        // Arrange
        FileSystemTreeNodeResponse response = new()
        {
            Path = "D:",
            Name = "D:",
            ItemType = FileSystemItemType.Root,
            IsExpanded = false
        };

        // Act
        WindowsRootItem result = response.Adapt<WindowsRootItem>(_config);

        // Assert
        result.Should().NotBeNull();
        result.Id.Path.Should().Be(response.Path);
        result.Name.Should().Be(response.Name);
        result.Type.Should().Be(FileSystemItemType.Root);
        result.Status.Should().Be(FileSystemItemStatus.Accessible);
    }

    [Fact]
    public void Register_WhenMappingUnixRootItemToFileSystemTreeNodeResponse_ShouldMapCorrectly()
    {
        // Arrange
        UnixRootItem unixRoot = UnixRootItem.Create().Value;

        // Act
        FileSystemTreeNodeResponse result = unixRoot.Adapt<FileSystemTreeNodeResponse>(_config);

        // Assert
        result.Should().NotBeNull();
        result.Path.Should().Be(unixRoot.Id.Path);
        result.Name.Should().Be(unixRoot.Name);
        result.ItemType.Should().Be(FileSystemItemType.Root);
        result.IsExpanded.Should().BeFalse();
        result.ChildrenLoaded.Should().BeFalse();
        result.Children.Should().NotBeNull();
        result.Children.Should().BeEmpty();
    }

    [Fact]
    public void Register_WhenMappingFileSystemTreeNodeResponseToUnixRootItem_ShouldMapCorrectly()
    {
        // Arrange
        FileSystemTreeNodeResponse response = new()
        {
            Path = "/",
            Name = "/",
            ItemType = FileSystemItemType.Root,
            IsExpanded = false
        };

        // Act
        UnixRootItem result = response.Adapt<UnixRootItem>(_config);

        // Assert
        result.Should().NotBeNull();
        result.Id.Path.Should().Be("/");
        result.Name.Should().Be("/");
        result.Type.Should().Be(FileSystemItemType.Root);
        result.Status.Should().Be(FileSystemItemStatus.Accessible);
    }

    [Fact]
    public void Register_WhenMappingDirectoryToFileSystemTreeNodeResponse_ShouldMapCorrectly()
    {
        // Arrange
        DirectoryFixture directoryFixture = new();
        Directory directory = directoryFixture.CreateDirectory();
        directory.AddItem(directoryFixture.CreateDirectory());
        directory.AddItem(directoryFixture.CreateDirectory());

        // Act
        FileSystemTreeNodeResponse result = directory.Adapt<FileSystemTreeNodeResponse>(_config);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(directory.Name);
        result.Path.Should().Be(directory.Id.Path);
        result.ItemType.Should().Be(directory.Type);
        result.IsExpanded.Should().BeFalse();
        result.ChildrenLoaded.Should().BeFalse();
        result.Children.Should().NotBeNull();
        result.Children.Should().HaveCount(2);
    }

    [Fact]
    public void Register_WhenMappingFileToFileSystemTreeNodeResponse_ShouldMapCorrectly()
    {
        // Arrange
        FileFixture fileFixture = new();
        File file = fileFixture.CreateFile();

        // Act
        FileSystemTreeNodeResponse result = file.Adapt<FileSystemTreeNodeResponse>(_config);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(file.Name);
        result.Path.Should().Be(file.Id.Path);
        result.ItemType.Should().Be(file.Type);
        result.IsExpanded.Should().BeFalse();
        result.ChildrenLoaded.Should().BeFalse();
        result.Children.Should().BeEmpty();
    }
    #endregion
}
