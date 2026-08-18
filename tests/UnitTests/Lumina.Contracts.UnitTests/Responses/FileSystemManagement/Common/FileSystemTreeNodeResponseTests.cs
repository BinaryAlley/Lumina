#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Fixtures.Core.Responses.FileSystemManagement.Common;
using Lumina.Contracts.Responses.FileSystemManagement.Common;
using Lumina.Domain.SharedKernel.Common.Enums.FileSystem;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
#endregion

namespace Lumina.Contracts.UnitTests.Responses.FileSystemManagement.Common;

/// <summary>
/// Contains unit tests for the <see cref="FileSystemTreeNodeResponse"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class FileSystemTreeNodeResponseTests
{
    private readonly FileSystemTreeNodeResponseFixture _fileSystemTreeNodeResponseFixture = new();

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    [Fact]
    public void Constructor_WhenInstantiatingNode_ShouldInitializeEmptyChildren()
    {
        // Act
        FileSystemTreeNodeResponse sut = new();

        // Assert
        Assert.NotNull(sut.Children);
        Assert.Empty(sut.Children);
    }

    [Fact]
    public void Create_WhenCalled_ShouldReturnValidTreeNode()
    {
        // Act
        FileSystemTreeNodeResponse sut = _fileSystemTreeNodeResponseFixture.Create();

        // Assert
        Assert.NotNull(sut);
        Assert.False(string.IsNullOrWhiteSpace(sut.Path));
        Assert.False(string.IsNullOrWhiteSpace(sut.Name));
        Assert.Equal(FileSystemItemType.Directory, sut.ItemType);
    }

    [Fact]
    public void RoundTrip_WhenSerializingTreeNode_ShouldPreserveValues()
    {
        // Arrange
        FileSystemTreeNodeResponse expected = _fileSystemTreeNodeResponseFixture.Create(maxDepth: 1, maxChildren: 2);

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        FileSystemTreeNodeResponse? actual = JsonSerializer.Deserialize<FileSystemTreeNodeResponse>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(expected.Path, actual.Path);
        Assert.Equal(expected.Name, actual.Name);
        Assert.Equal(expected.ItemType, actual.ItemType);
        Assert.Equal(expected.Children.Count, actual.Children.Count);
    }

    [Fact]
    public void Serialize_WhenSerializingTreeNode_ShouldSerializeItemTypeAsCamelCaseString()
    {
        // Arrange
        FileSystemTreeNodeResponse sut = new()
        {
            Path = @"C:\Media",
            Name = "Media",
            ItemType = FileSystemItemType.Directory,
            IsExpanded = false,
            ChildrenLoaded = true,
            Children = []
        };

        // Act
        string json = JsonSerializer.Serialize(sut, _jsonOptions);

        // Assert
        Assert.Contains("\"ItemType\":\"directory\"", json, StringComparison.Ordinal);
    }
}
