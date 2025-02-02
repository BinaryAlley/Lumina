#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.FileSystemManagement.Files;
using Lumina.Application.UnitTests.Core.FileSystemManagement.Files.Fixtures;
using Lumina.Contracts.Responses.FileSystemManagement.Common;
using Lumina.Contracts.Responses.FileSystemManagement.Files;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.FileSystemManagement.Files;

/// <summary>
/// Contains unit tests for the <see cref="FileMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class FileMappingTests
{
    private readonly FileFixture _fileFixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileMappingTests"/> class.
    /// </summary>
    public FileMappingTests()
    {
        _fileFixture = new();
    }

    [Fact]
    public void ToFileSystemTreeNodeResponse_WhenMappingFile_ShouldMapCorrectly()
    {
        // Arrange
        File domainModel = _fileFixture.CreateFile();

        // Act
        FileSystemTreeNodeResponse result = domainModel.ToFileSystemTreeNodeResponse();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(domainModel.Name, result.Name);
        Assert.Equal(domainModel.Id.Path, result.Path);
        Assert.Equal(domainModel.Type, result.ItemType);
        Assert.False(result.IsExpanded);
        Assert.False(result.ChildrenLoaded);
    }

    [Fact]
    public void ToFileSystemTreeNodeResponses_WhenMappingMultipleFiles_ShouldMapCorrectly()
    {
        // Arrange
        IEnumerable<File> domainModels = _fileFixture.CreateMany();

        // Act
        IEnumerable<FileSystemTreeNodeResponse> result = domainModels.ToFileSystemTreeNodeResponses();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(domainModels.Count(), result.Count());

        List<FileSystemTreeNodeResponse> resultList = result.ToList();
        List<File> domainModelsList = domainModels.ToList();
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

    [Fact]
    public void ToResponse_WhenMappingFile_ShouldMapCorrectly()
    {
        // Arrange
        File domainModel = _fileFixture.CreateFile();

        // Act
        FileResponse result = domainModel.ToResponse();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(domainModel.Id.Path, result.Path);
        Assert.Equal(domainModel.Name, result.Name);
        Assert.Equal(domainModel.DateCreated.Value, result.DateCreated);
        Assert.Equal(domainModel.DateModified.Value, result.DateModified);
        Assert.Equal(domainModel.Size, result.Size);
    }

    [Fact]
    public void ToResponses_WhenMappingMultipleFiles_ShouldMapCorrectly()
    {
        // Arrange
        IEnumerable<File> domainModels = _fileFixture.CreateMany();

        // Act
        IEnumerable<FileResponse> result = domainModels.ToResponses();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(domainModels.Count(), result.Count());

        List<FileResponse> resultList = result.ToList();
        List<File> domainModelsList = domainModels.ToList();
        for (int i = 0; i < resultList.Count; i++)
        {
            Assert.Equal(domainModelsList[i].Name, resultList[i].Name);
            Assert.Equal(domainModelsList[i].Id.Path, resultList[i].Path);
            Assert.Equal(domainModelsList[i].Size, resultList[i].Size);
            Assert.Equal(domainModelsList[i].DateCreated.Value, resultList[i].DateCreated);
            Assert.Equal(domainModelsList[i].DateModified.Value, resultList[i].DateModified);
        }
    }
}
