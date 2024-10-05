#region ========================================================================= USING =====================================================================================
using FluentAssertions;
using Lumina.Application.Common.Mapping.FileManagement;
using Lumina.Application.UnitTests.Core.FileManagement.Directories.Fixtures;
using Lumina.Contracts.Models.FileManagement;
using Lumina.Contracts.Responses.FileManagement;
using Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate;
using Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.Entities;
using Mapster;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.FileManagement;

/// <summary>
/// Contains unit tests for the <see cref="DirectoryMappingConfig"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class DirectoryMappingConfigTests
{
    #region ================================================================== FIELD MEMBERS ================================================================================
    private readonly TypeAdapterConfig _config;
    private readonly DirectoryMappingConfig _directoryMappingConfig;
    private readonly FileSystemTreeMappingConfig _fileSystemTreeMappingConfig;
    #endregion

    #region ====================================================================== CTOR =====================================================================================
    public DirectoryMappingConfigTests()
    {
        _config = new TypeAdapterConfig();
        _directoryMappingConfig = new DirectoryMappingConfig();
        _fileSystemTreeMappingConfig = new FileSystemTreeMappingConfig();
        _directoryMappingConfig.Register(_config);
        _fileSystemTreeMappingConfig.Register(_config);
    }
    #endregion

    #region ===================================================================== METHODS ===================================================================================
    [Fact]
    public void Register_WhenMappingDirectoryToFileSystemTreeNodeResponse_ShouldMapCorrectly()
    {
        // Arrange
        DirectoryFixture directoryFixture = new();
        Directory directory = directoryFixture.CreateDirectory();
      
        // create some child items
        List<FileSystemItem> childItems =
        [
            directoryFixture.CreateDirectory(),
            directoryFixture.CreateDirectory()
        ];
        foreach (FileSystemItem item in childItems)
            directory.AddItem(item);

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
        result.Children.Should().HaveCount(childItems.Count);

        // Verify that each child item is correctly mapped
        for (int i = 0; i < childItems.Count; i++)
        {
            result.Children[i].Name.Should().Be(childItems[i].Name);
            result.Children[i].Path.Should().Be(childItems[i].Id.Path);
            result.Children[i].ItemType.Should().Be(childItems[i].Type);
        }
    }

    [Fact]
    public void Register_WhenMappingDirectoryToDirectoryResponse_ShouldMapCorrectly()
    {
        // Arrange
        DirectoryFixture directoryFixture = new();
        Directory directory = directoryFixture.CreateDirectory();

        // Act
        DirectoryResponse result = directory.Adapt<DirectoryResponse>(_config);

        // Assert
        result.Should().NotBeNull();
        result.Path.Should().Be(directory.Id.Path);
        result.Name.Should().Be(directory.Name);
        result.DateCreated.Should().Be(directory.DateCreated.Value);
        result.DateModified.Should().Be(directory.DateModified.Value);
        result.Items.Should().BeAssignableTo<List<FileSystemItemModel>>();
        result.Items.Should().HaveCount(directory.Items.Count);
    }

    [Fact]
    public void Register_WhenMappingDirectoryToFileSystemItemModel_ShouldMapCorrectly()
    {
        // Arrange
        DirectoryFixture directoryFixture = new();
        Directory directory = directoryFixture.CreateDirectory();
        DateTime beforeMapping = DateTime.Now;

        // Act
        FileSystemItemModel result = directory.Adapt<FileSystemItemModel>(_config);
        DateTime afterMapping = DateTime.Now;

        // Assert
        result.Should().NotBeNull();
        result.Path.Should().Be(directory.Id.Path);
        result.Name.Should().Be(directory.Name);
        result.DateCreated.Should().BeOnOrAfter(beforeMapping).And.BeOnOrBefore(afterMapping);
        result.DateModified.Should().BeOnOrAfter(beforeMapping).And.BeOnOrBefore(afterMapping);
    }
    #endregion
}
