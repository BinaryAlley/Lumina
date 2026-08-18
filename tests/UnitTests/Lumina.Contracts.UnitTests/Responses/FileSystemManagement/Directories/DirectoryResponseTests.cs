#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Fixtures.Core.Responses.FileSystemManagement.Directories;
using Lumina.Contracts.Responses.FileSystemManagement.Directories;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Contracts.UnitTests.Responses.FileSystemManagement.Directories;

/// <summary>
/// Contains unit tests for the <see cref="DirectoryResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class DirectoryResponseTests
{
    private readonly DirectoryResponseFixture _directoryResponseFixture = new();

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void Create_WhenCalled_ShouldReturnValidDirectoryResponse()
    {
        // Act
        DirectoryResponse sut = _directoryResponseFixture.Create();

        // Assert
        Assert.NotNull(sut);
        Assert.False(string.IsNullOrWhiteSpace(sut.Path));
        Assert.False(string.IsNullOrWhiteSpace(sut.Name));
        Assert.NotEmpty(sut.Items);
    }

    [Fact]
    public void RoundTrip_WhenSerializingDirectoryResponse_ShouldPreserveValues()
    {
        // Arrange
        DirectoryResponse expected = _directoryResponseFixture.Create(itemCount: 2);

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        DirectoryResponse? actual = JsonSerializer.Deserialize<DirectoryResponse>(json, _jsonOptions);
        // Assert
        Assert.NotNull(actual);
        Assert.Equivalent(expected, actual);
    }

    [Fact]
    public void Deconstruct_WhenCalled_ShouldReturnAllProperties()
    {
        // Arrange
        DirectoryResponse sut = _directoryResponseFixture.Create(itemCount: 1);

        // Act
        (string path, string name, System.DateTime dateCreated, System.DateTime dateModified, System.Collections.Generic.List<Lumina.Contracts.DTO.FileSystemManagement.FileSystemItemDto> items) = sut;

        // Assert
        Assert.Equal(sut.Path, path);
        Assert.Equal(sut.Name, name);
        Assert.Equal(sut.DateCreated, dateCreated);
        Assert.Equal(sut.DateModified, dateModified);
        Assert.Equal(sut.Items, items);
    }
}
