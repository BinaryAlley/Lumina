#region ========================================================================= USING =====================================================================================
using FluentAssertions;
using Lumina.Application.Common.Mapping.FileSystemManagement.Files;
using Lumina.Application.UnitTests.Core.FileSystemManagement.Files.Fixtures;
using Lumina.Contracts.Responses.FileSystemManagement.Common;
using Lumina.Contracts.Responses.FileSystemManagement.Files;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.Aggregates.FileSystemManagement.FileSystemManagementAggregate.Entities;
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
        result.Should().NotBeNull();
        result.Name.Should().Be(domainModel.Name);
        result.Path.Should().Be(domainModel.Id.Path);
        result.ItemType.Should().Be(domainModel.Type);
        result.IsExpanded.Should().BeFalse();
        result.ChildrenLoaded.Should().BeFalse();
    }

    [Fact]
    public void ToFileSystemTreeNodeResponses_WhenMappingMultipleFiles_ShouldMapCorrectly()
    {
        // Arrange
        IEnumerable<File> domainModels = _fileFixture.CreateMany();

        // Act
        IEnumerable<FileSystemTreeNodeResponse> result = domainModels.ToFileSystemTreeNodeResponses();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(domainModels.Count());

        List<FileSystemTreeNodeResponse> resultList = result.ToList();
        List<File> domainModelsList = domainModels.ToList();
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

    [Fact]
    public void ToResponse_WhenMappingFile_ShouldMapCorrectly()
    {
        // Arrange
        File domainModel = _fileFixture.CreateFile();

        // Act
        FileResponse result = domainModel.ToResponse();

        // Assert
        result.Should().NotBeNull();
        result.Path.Should().Be(domainModel.Id.Path);
        result.Name.Should().Be(domainModel.Name);
        result.DateCreated.Should().Be(domainModel.DateCreated.Value);
        result.DateModified.Should().Be(domainModel.DateModified.Value);
        result.Size.Should().Be(domainModel.Size);
    }

    [Fact]
    public void ToResponses_WhenMappingMultipleFiles_ShouldMapCorrectly()
    {
        // Arrange
        IEnumerable<File> domainModels = _fileFixture.CreateMany();

        // Act
        IEnumerable<FileResponse> result = domainModels.ToResponses();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(domainModels.Count());

        List<FileResponse> resultList = result.ToList();
        List<File> domainModelsList = domainModels.ToList();
        for (int i = 0; i < resultList.Count; i++)
        {
            resultList[i].Name.Should().Be(domainModelsList[i].Name);
            resultList[i].Path.Should().Be(domainModelsList[i].Id.Path);
            resultList[i].Size.Should().Be(domainModelsList[i].Size);
            resultList[i].DateCreated.Should().Be(domainModelsList[i].DateCreated.Value);
            resultList[i].DateModified.Should().Be(domainModelsList[i].DateModified.Value);
        }
    }
}
