#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.FileSystemManagement.FileSystem;
using Lumina.Application.UnitTests.Common.Mapping.FileSystemManagement.FileSystem.Fixtures;
using Lumina.Application.UnitTests.Core.FileSystemManagement.Directories.Fixtures;
using Lumina.Application.UnitTests.Core.FileSystemManagement.Files.Fixtures;
using Lumina.Application.UnitTests.Core.FileSystemManagement.FileSystem.Fixtures;
using Lumina.Application.UnitTests.Core.FileSystemManagement.Fixtures;
using Lumina.Contracts.Responses.FileSystemManagement.Common;
using Lumina.Domain.Common.Enums.FileSystem;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Entities;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.ValueObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Lumina.Contracts.DTO.FileSystemManagement;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.FileSystemManagement.FileSystem;

/// <summary>
/// Contains unit tests for the <see cref="FileSystemItemMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class FileSystemItemMappingTests
{
    private readonly FileSystemPathIdFixture _fileSystemPathIdFixture;
    private readonly DirectoryFixture _directoryFixture;
    private readonly FileFixture _fileFixture;
    private readonly WindowsRootItemFixture _windowsRootItemFixture;
    private readonly UnixRootItemFixture _unixRootItemFixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileSystemItemMappingTests"/> class.
    /// </summary>
    public FileSystemItemMappingTests()
    {
        _fileSystemPathIdFixture = new FileSystemPathIdFixture();
        _directoryFixture = new DirectoryFixture();
        _fileFixture = new FileFixture();
        _windowsRootItemFixture = new WindowsRootItemFixture();
        _unixRootItemFixture = new UnixRootItemFixture();
    }

    [Fact]
    public void ToFileSystemItemModel_WhenMappingFileSystemItem_ShouldMapCorrectly()
    {
        // Arrange
        FileSystemPathId id = _fileSystemPathIdFixture.CreateFileSystemPathId();
        string name = "TestItem";
        FileSystemItemType type = FileSystemItemType.File;
        FileSystemItemFixture domainModel = new(id, name, type);

        // Arrange
       
        // Act
        FileSystemItemDto result = domainModel.ToFileSystemItemDto();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(domainModel.Id.Path, result.Path);
        Assert.Equal(domainModel.Name, result.Name);
        Assert.True((DateTime.Now - result.DateCreated) < TimeSpan.FromSeconds(1));
        Assert.True((DateTime.Now - result.DateModified) < TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void ToTreeNodeResponse_WhenMappingDirectory_ShouldMapCorrectly()
    {
        // Arrange
        Directory directory = _directoryFixture.CreateDirectory();

        // Act
        FileSystemTreeNodeResponse result = directory.ToTreeNodeResponse();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(directory.Name, result.Name);
        Assert.Equal(directory.Id.Path, result.Path);
        Assert.Equal(directory.Type, result.ItemType);
    }

    [Fact]
    public void ToTreeNodeResponse_WhenMappingFile_ShouldMapCorrectly()
    {
        // Arrange
        File file = _fileFixture.CreateFile();

        // Act
        FileSystemTreeNodeResponse result = file.ToTreeNodeResponse();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(file.Name, result.Name);
        Assert.Equal(file.Id.Path, result.Path);
        Assert.Equal(file.Type, result.ItemType);
    }

    [Fact]
    public void ToTreeNodeResponse_WhenMappingWindowsRootItem_ShouldMapCorrectly()
    {
        // Arrange
        WindowsRootItem windowsRootItem = _windowsRootItemFixture.Create();

        // Act
        FileSystemTreeNodeResponse result = windowsRootItem.ToTreeNodeResponse();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(windowsRootItem.Name, result.Name);
        Assert.Equal(windowsRootItem.Id.Path, result.Path);
        Assert.Equal(windowsRootItem.Type, result.ItemType);
        Assert.False(result.IsExpanded);
        Assert.False(result.ChildrenLoaded);
        Assert.Empty(result.Children);
    }

    [Fact]
    public void ToTreeNodeResponse_WhenMappingUnixRootItem_ShouldMapCorrectly()
    {
        // Arrange
        UnixRootItem unixRootItem = _unixRootItemFixture.Create();

        // Act
        FileSystemTreeNodeResponse result = unixRootItem.ToTreeNodeResponse();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(unixRootItem.Name, result.Name);
        Assert.Equal(unixRootItem.Id.Path, result.Path);
        Assert.Equal(unixRootItem.Type, result.ItemType);
        Assert.False(result.IsExpanded);
        Assert.False(result.ChildrenLoaded);
        Assert.Empty(result.Children);
    }

    [Theory]
    [InlineData("TestFile.txt", FileSystemItemType.File)]
    [InlineData("TestFolder", FileSystemItemType.Directory)]
    public void ToFileSystemItemModel_WhenMappingDifferentFileSystemItems_ShouldMapCorrectly(string name, FileSystemItemType type)
    {
        // Arrange
        FileSystemPathId id = _fileSystemPathIdFixture.CreateFileSystemPathId();
        FileSystemItemFixture domainModel = new(id, name, type);

        // Act
        FileSystemItemDto result = domainModel.ToFileSystemItemDto();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(domainModel.Id.Path, result.Path);
        Assert.Equal(domainModel.Name, result.Name);
        Assert.True((DateTime.Now - result.DateCreated) < TimeSpan.FromSeconds(1));
        Assert.True((DateTime.Now - result.DateModified) < TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void ToTreeNodeResponses_WhenMappingMixedFileSystemItems_ShouldMapCorrectly()
    {
        // Arrange
        List<FileSystemItem> domainModels =
        [
            _directoryFixture.CreateDirectory(),
            _fileFixture.CreateFile(),
            _windowsRootItemFixture.Create(),
            _unixRootItemFixture.Create()
        ];

        // Act
        IEnumerable<FileSystemTreeNodeResponse> result = domainModels.ToTreeNodeResponses();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(domainModels.Count, result.Count());

        List<FileSystemTreeNodeResponse> resultList = result.ToList();
        for (int i = 0; i < resultList.Count; i++)
        {
            Assert.Equal(domainModels[i].Name, resultList[i].Name);
            Assert.Equal(domainModels[i].Id.Path, resultList[i].Path);
            Assert.Equal(domainModels[i].Type, resultList[i].ItemType);
            Assert.False(resultList[i].IsExpanded);
            Assert.False(resultList[i].ChildrenLoaded);
        }
    }

    [Fact]
    public void ToTreeNodeResponse_WhenMappingInvalidFileSystemItem_ShouldThrowArgumentException()
    {
        // Arrange
        FileSystemPathId id = _fileSystemPathIdFixture.CreateFileSystemPathId();
        string name = "TestItem";
        FileSystemItemType type = FileSystemItemType.File;
        FileSystemItemFixture invalidItem = new(id, name, type);

        // Act & Assert
        ArgumentException exception = Assert.Throws<ArgumentException>(() => invalidItem.ToTreeNodeResponse());
        Assert.Equal("Invalid FileSystemItem", exception.Message);
    }

    [Fact]
    public void ToTreeNodeResponses_WhenMappingMultipleFileSystemItems_ShouldMapCorrectly()
    {
        // Arrange
        IEnumerable<FileSystemItem> domainModels = _directoryFixture.CreateMany();

        // Act
        IEnumerable<FileSystemTreeNodeResponse> result = domainModels.ToTreeNodeResponses();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(domainModels.Count(), result.Count());

        List<FileSystemTreeNodeResponse> resultList = result.ToList();
        List<FileSystemItem> domainModelsList = domainModels.ToList();
        for (int i = 0; i < resultList.Count; i++)
        {
            Assert.Equal(domainModelsList[i].Name, resultList[i].Name);
            Assert.Equal(domainModelsList[i].Id.Path, resultList[i].Path);
            Assert.Equal(domainModelsList[i].Type, resultList[i].ItemType);
            Assert.False(resultList[i].IsExpanded);
            Assert.False(resultList[i].ChildrenLoaded);
            Assert.Empty(resultList[i].Children);
        }
    }
}
