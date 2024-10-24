#region ========================================================================= USING =====================================================================================
using FluentAssertions;
using Lumina.Application.Common.Mapping.FileSystemManagement.Directories;
using Lumina.Application.UnitTests.Core.FileSystemManagement.Directories.Fixtures;
using Lumina.Contracts.Responses.FileSystemManagement.Common;
using Lumina.Contracts.Responses.FileSystemManagement.Directories;
using Lumina.Domain.Core.Aggregates.FileSystemManagement.FileSystemManagementAggregate;
using Lumina.Domain.Core.Aggregates.FileSystemManagement.FileSystemManagementAggregate.Entities;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.FileSystemManagement.Directories;

/// <summary>
/// Contains unit tests for the <see cref="DirectoryMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class DirectoryMappingTests
{
    private readonly DirectoryFixture _directoryFixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="DirectoryMappingTests"/> class.
    /// </summary>
    public DirectoryMappingTests()
    {
        _directoryFixture = new DirectoryFixture();
    }

    [Fact]
    public void ToFileSystemTreeNodeResponse_WhenMappingDirectory_ShouldMapCorrectly()
    {
        // Arrange
        Directory directory = _directoryFixture.CreateDirectory();
        List<FileSystemItem> childItems =
        [
            _directoryFixture.CreateDirectory(),
            _directoryFixture.CreateDirectory()
        ];
        foreach (FileSystemItem item in childItems)
            directory.AddItem(item);

        // Act
        FileSystemTreeNodeResponse result = directory.ToFileSystemTreeNodeResponse();

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(directory.Name);
        result.Path.Should().Be(directory.Id.Path);
        result.ItemType.Should().Be(directory.Type);
        result.IsExpanded.Should().BeFalse();
        result.ChildrenLoaded.Should().BeFalse();
        result.Children.Should().NotBeNull();
        result.Children.Should().HaveCount(childItems.Count);

        for (int i = 0; i < childItems.Count; i++)
        {
            result.Children[i].Name.Should().Be(childItems[i].Name);
            result.Children[i].Path.Should().Be(childItems[i].Id.Path);
            result.Children[i].ItemType.Should().Be(childItems[i].Type);
        }
    }

    [Fact]
    public void ToFileSystemTreeNodeResponses_WhenMappingMultipleDirectories_ShouldMapCorrectly()
    {
        // Arrange
        List<Directory> directories = _directoryFixture.CreateMany(2);

        // Act
        IEnumerable<FileSystemTreeNodeResponse> result = directories.ToFileSystemTreeNodeResponses();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(directories.Count);

        List<FileSystemTreeNodeResponse> resultList = result.ToList();
        for (int i = 0; i < directories.Count; i++)
        {
            resultList[i].Name.Should().Be(directories[i].Name);
            resultList[i].Path.Should().Be(directories[i].Id.Path);
            resultList[i].ItemType.Should().Be(directories[i].Type);
        }
    }

    [Fact]
    public void ToResponse_WhenMappingDirectory_ShouldMapCorrectly()
    {
        // Arrange
        Directory directory = _directoryFixture.CreateDirectory();
        List<FileSystemItem> childItems =
        [
            _directoryFixture.CreateDirectory(),
            _directoryFixture.CreateDirectory()
        ];
        foreach (FileSystemItem item in childItems)
            directory.AddItem(item);

        // Act
        DirectoryResponse result = directory.ToResponse();

        // Assert
        result.Should().NotBeNull();
        result.Path.Should().Be(directory.Id.Path);
        result.Name.Should().Be(directory.Name);
        result.DateCreated.Should().Be(directory.DateCreated.Value);
        result.DateModified.Should().Be(directory.DateModified.Value);
        result.Items.Should().NotBeNull();
        result.Items.Should().HaveCount(childItems.Count);

        for (int i = 0; i < childItems.Count; i++)
        {
            result.Items[i].Name.Should().Be(childItems[i].Name);
            result.Items[i].Path.Should().Be(childItems[i].Id.Path);
        }
    }

    [Fact]
    public void ToResponses_WhenMappingMultipleDirectories_ShouldMapCorrectly()
    {
        // Arrange
        List<Directory> directories = _directoryFixture.CreateMany(2);

        // Act
        IEnumerable<DirectoryResponse> result = directories.ToResponses();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(directories.Count);

        List<DirectoryResponse> resultList = result.ToList();
        for (int i = 0; i < directories.Count; i++)
        {
            resultList[i].Path.Should().Be(directories[i].Id.Path);
            resultList[i].Name.Should().Be(directories[i].Name);
            resultList[i].DateCreated.Should().Be(directories[i].DateCreated.Value);
            resultList[i].DateModified.Should().Be(directories[i].DateModified.Value);
        }
    }
}
