#region ========================================================================= USING =====================================================================================
using FluentAssertions;
using Lumina.Application.Common.Mapping.FileSystemManagement.FileSystem;
using Lumina.Application.UnitTests.Common.Mapping.FileSystemManagement.FileSystem.Fixtures;
using Lumina.Application.UnitTests.Core.FileSystemManagement.Directories.Fixtures;
using Lumina.Application.UnitTests.Core.FileSystemManagement.Files.Fixtures;
using Lumina.Application.UnitTests.Core.FileSystemManagement.FileSystem.Fixtures;
using Lumina.Application.UnitTests.Core.FileSystemManagement.Fixtures;
using Lumina.Contracts.Entities.FileSystemManagement;
using Lumina.Domain.Common.Enums.FileSystem;
using Lumina.Contracts.Responses.FileSystemManagement.Common;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Entities;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
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
        FileSystemItemEntity result = domainModel.ToFileSystemItemEntity();

        // Assert
        result.Should().NotBeNull();
        result.Path.Should().Be(domainModel.Id.Path);
        result.Name.Should().Be(domainModel.Name);
        result.DateCreated.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
        result.DateModified.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void ToTreeNodeResponse_WhenMappingDirectory_ShouldMapCorrectly()
    {
        // Arrange
        Directory directory = _directoryFixture.CreateDirectory();

        // Act
        FileSystemTreeNodeResponse result = directory.ToTreeNodeResponse();

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(directory.Name);
        result.Path.Should().Be(directory.Id.Path);
        result.ItemType.Should().Be(directory.Type);
    }

    [Fact]
    public void ToTreeNodeResponse_WhenMappingFile_ShouldMapCorrectly()
    {
        // Arrange
        File file = _fileFixture.CreateFile();

        // Act
        FileSystemTreeNodeResponse result = file.ToTreeNodeResponse();

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(file.Name);
        result.Path.Should().Be(file.Id.Path);
        result.ItemType.Should().Be(file.Type);
    }

    [Fact]
    public void ToTreeNodeResponse_WhenMappingWindowsRootItem_ShouldMapCorrectly()
    {
        // Arrange
        WindowsRootItem windowsRootItem = _windowsRootItemFixture.Create();

        // Act
        FileSystemTreeNodeResponse result = windowsRootItem.ToTreeNodeResponse();

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(windowsRootItem.Name);
        result.Path.Should().Be(windowsRootItem.Id.Path);
        result.ItemType.Should().Be(windowsRootItem.Type);
        result.IsExpanded.Should().BeFalse();
        result.ChildrenLoaded.Should().BeFalse();
        result.Children.Should().BeEmpty();
    }

    [Fact]
    public void ToTreeNodeResponse_WhenMappingUnixRootItem_ShouldMapCorrectly()
    {
        // Arrange
        UnixRootItem unixRootItem = _unixRootItemFixture.Create();

        // Act
        FileSystemTreeNodeResponse result = unixRootItem.ToTreeNodeResponse();

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(unixRootItem.Name);
        result.Path.Should().Be(unixRootItem.Id.Path);
        result.ItemType.Should().Be(unixRootItem.Type);
        result.IsExpanded.Should().BeFalse();
        result.ChildrenLoaded.Should().BeFalse();
        result.Children.Should().BeEmpty();
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
        FileSystemItemEntity result = domainModel.ToFileSystemItemEntity();

        // Assert
        result.Should().NotBeNull();
        result.Path.Should().Be(domainModel.Id.Path);
        result.Name.Should().Be(domainModel.Name);
        result.DateCreated.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
        result.DateModified.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
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
        result.Should().NotBeNull();
        result.Should().HaveCount(domainModels.Count);

        List<FileSystemTreeNodeResponse> resultList = result.ToList();
        for (int i = 0; i < resultList.Count; i++)
        {
            resultList[i].Name.Should().Be(domainModels[i].Name);
            resultList[i].Path.Should().Be(domainModels[i].Id.Path);
            resultList[i].ItemType.Should().Be(domainModels[i].Type);
            resultList[i].IsExpanded.Should().BeFalse();
            resultList[i].ChildrenLoaded.Should().BeFalse();
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
        invalidItem.Invoking(item => item.ToTreeNodeResponse())
            .Should().Throw<ArgumentException>()
            .WithMessage("Invalid FileSystemItem");
    }

    [Fact]
    public void ToTreeNodeResponses_WhenMappingMultipleFileSystemItems_ShouldMapCorrectly()
    {
        // Arrange
        IEnumerable<FileSystemItem> domainModels = _directoryFixture.CreateMany();

        // Act
        IEnumerable<FileSystemTreeNodeResponse> result = domainModels.ToTreeNodeResponses();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(domainModels.Count());

        List<FileSystemTreeNodeResponse> resultList = result.ToList();
        List<FileSystemItem> domainModelsList = domainModels.ToList();
        for (int i = 0; i < resultList.Count; i++)
        {
            resultList[i].Name.Should().Be(domainModelsList[i].Name);
            resultList[i].Path.Should().Be(domainModelsList[i].Id.Path);
            resultList[i].ItemType.Should().Be(domainModelsList[i].Type);
            resultList[i].IsExpanded.Should().BeFalse();
            resultList[i].ChildrenLoaded.Should().BeFalse();
            resultList[i].Children.Should().BeEmpty();
        }
    }
}
