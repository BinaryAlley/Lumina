#region ========================================================================= USING =====================================================================================
using FluentAssertions;
using Lumina.Application.Common.Mapping.FileManagement;
using Lumina.Application.UnitTests.Core.FileManagement.Files.Fixtures;
using Lumina.Contracts.Responses.FileManagement;
using Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.Entities;
using Mapster;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.FileManagement;

/// <summary>
/// Contains unit tests for the <see cref="FileMappingConfig"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class FileMappingConfigTests
{
    #region ================================================================== FIELD MEMBERS ================================================================================
    private readonly TypeAdapterConfig _config;
    private readonly FileMappingConfig _fileMappingConfig;
    private readonly FileSystemTreeMappingConfig _fileSystemTreeMappingConfig;
    #endregion

    #region ====================================================================== CTOR =====================================================================================
    public FileMappingConfigTests()
    {
        _config = new TypeAdapterConfig();
        _fileMappingConfig = new FileMappingConfig();
        _fileSystemTreeMappingConfig = new FileSystemTreeMappingConfig();
        _fileMappingConfig.Register(_config);
        _fileSystemTreeMappingConfig.Register(_config);
    }
    #endregion

    #region ===================================================================== METHODS ===================================================================================
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

    [Fact]
    public void Register_WhenMappingFileToFileResponse_ShouldMapCorrectly()
    {
        // Arrange
        FileFixture fileFixture = new();
        File file = fileFixture.CreateFile();

        // Act
        FileResponse result = file.Adapt<FileResponse>(_config);

        // Assert
        result.Should().NotBeNull();
        result.Path.Should().Be(file.Id.Path);
        result.Name.Should().Be(file.Name);
        result.DateCreated.Should().Be(file.DateCreated.Value);
        result.DateModified.Should().Be(file.DateModified.Value);
        result.Size.Should().Be(file.Size);
    }

    [Fact]
    public void Register_WhenMappingFileToFileResponseWithoutDates_ShouldMapCorrectly()
    {
        // Arrange
        FileFixture fileFixture = new();
        File file = FileFixture.CreateFileWithoutDates();

        // Act
        FileResponse result = file.Adapt<FileResponse>(_config);

        // Assert
        result.Should().NotBeNull();
        result.Path.Should().Be(file.Id.Path);
        result.Name.Should().Be(file.Name);
        result.DateCreated.Should().Be(default);
        result.DateModified.Should().Be(default);
        result.Size.Should().Be(file.Size);
    }
    #endregion
}
