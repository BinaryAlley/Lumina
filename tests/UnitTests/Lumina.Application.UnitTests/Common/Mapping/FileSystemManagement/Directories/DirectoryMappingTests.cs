#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.FileSystemManagement.Directories;
using Lumina.Contracts.Responses.FileSystemManagement.Common;
using Lumina.Contracts.Responses.FileSystemManagement.Directories;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Entities;
using Lumina.Domain.Fixtures.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Entities;
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
    private readonly DirectoryFixture _directoryFixture = new();

    [Theory]
    [InlineData(true)] // hidden directory
    [InlineData(false)] // visible directory
    public void ToFileSystemTreeNodeResponse_WhenMappingDirectory_ShouldMapCorrectly(bool isHidden)
    {
        // Arrange
        Directory directory = _directoryFixture.Create(isHidden: isHidden);
        List<FileSystemItem> childItems =
        [
            _directoryFixture.Create(isHidden: true),
            _directoryFixture.Create(isHidden: false)
        ];
        foreach (FileSystemItem item in childItems)
            directory.AddItem(item);

        // Act
        FileSystemTreeNodeResponse result = directory.ToFileSystemTreeNodeResponse();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(directory.Name, result.Name);
        Assert.Equal(directory.Id.Path, result.Path);
        Assert.Equal(directory.Type, result.ItemType);
        Assert.Equal(directory.IsHidden, result.IsHidden);
        Assert.False(result.IsExpanded);
        Assert.False(result.ChildrenLoaded);
        Assert.NotNull(result.Children);
        Assert.Equal(childItems.Count, result.Children.Count);

        for (int i = 0; i < childItems.Count; i++)
        {
            Assert.Equal(childItems[i].Name, result.Children[i].Name);
            Assert.Equal(childItems[i].Id.Path, result.Children[i].Path);
            Assert.Equal(childItems[i].Type, result.Children[i].ItemType);
            Assert.Equal(childItems[i].IsHidden, result.Children[i].IsHidden);
        }
    }

    [Fact]
    public void ToFileSystemTreeNodeResponses_WhenMappingMultipleDirectories_ShouldMapCorrectly()
    {
        // Arrange
        List<Directory> directories =
        [
            _directoryFixture.Create(isHidden: true),
            _directoryFixture.Create(isHidden: false)
        ];

        // Act
        IEnumerable<FileSystemTreeNodeResponse> result = directories.ToFileSystemTreeNodeResponses();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(directories.Count, result.Count());

        List<FileSystemTreeNodeResponse> resultList = result.ToList();
        for (int i = 0; i < directories.Count; i++)
        {
            Assert.Equal(directories[i].Name, resultList[i].Name);
            Assert.Equal(directories[i].Id.Path, resultList[i].Path);
            Assert.Equal(directories[i].Type, resultList[i].ItemType);
            Assert.Equal(directories[i].IsHidden, resultList[i].IsHidden);
        }
    }

    [Theory]
    [InlineData(true)] // hidden directory
    [InlineData(false)] // visible directory
    public void ToResponse_WhenMappingDirectory_ShouldMapCorrectly(bool isHidden)
    {
        // Arrange
        Directory directory = _directoryFixture.Create(isHidden: isHidden);
        List<FileSystemItem> childItems =
        [
            _directoryFixture.Create(isHidden: true),
            _directoryFixture.Create(isHidden: false)
        ];
        foreach (FileSystemItem item in childItems)
            directory.AddItem(item);

        // Act
        DirectoryResponse result = directory.ToResponse();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(directory.Id.Path, result.Path);
        Assert.Equal(directory.Name, result.Name);
        Assert.Equal(directory.DateCreated.Value, result.DateCreated);
        Assert.Equal(directory.DateModified.Value, result.DateModified);
        Assert.Equal(directory.IsHidden, result.IsHidden);
        Assert.NotNull(result.Items);
        Assert.Equal(childItems.Count, result.Items.Count);

        for (int i = 0; i < childItems.Count; i++)
        {
            Assert.Equal(childItems[i].Name, result.Items[i].Name);
            Assert.Equal(childItems[i].Id.Path, result.Items[i].Path);
            Assert.Equal(childItems[i].IsHidden, result.Items[i].IsHidden);
        }
    }

    [Fact]
    public void ToResponses_WhenMappingMultipleDirectories_ShouldMapCorrectly()
    {
        // Arrange
        List<Directory> directories =
        [
            _directoryFixture.Create(isHidden: true),
            _directoryFixture.Create(isHidden: false)
        ];

        // Act
        IEnumerable<DirectoryResponse> result = directories.ToResponses();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(directories.Count, result.Count());

        List<DirectoryResponse> resultList = result.ToList();
        for (int i = 0; i < directories.Count; i++)
        {
            Assert.Equal(directories[i].Id.Path, resultList[i].Path);
            Assert.Equal(directories[i].Name, resultList[i].Name);
            Assert.Equal(directories[i].DateCreated.Value, resultList[i].DateCreated);
            Assert.Equal(directories[i].DateModified.Value, resultList[i].DateModified);
            Assert.Equal(directories[i].IsHidden, resultList[i].IsHidden);
        }
    }
}
