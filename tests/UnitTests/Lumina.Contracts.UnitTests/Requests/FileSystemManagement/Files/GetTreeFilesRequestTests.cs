#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Fixtures.Core.Requests.FileSystemManagement.Files;
using Lumina.Contracts.Requests.FileSystemManagement.Files;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Contracts.UnitTests.Requests.FileSystemManagement.Files;

/// <summary>
/// Contains unit tests for the <see cref="GetTreeFilesRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetTreeFilesRequestTests
{
    private readonly GetTreeFilesRequestFixture _getTreeFilesRequestFixture = new();

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void Create_WhenCalled_ShouldReturnValidGetTreeFilesRequest()
    {
        // Act
        GetTreeFilesRequest sut = _getTreeFilesRequestFixture.Create();

        // Assert
        Assert.NotNull(sut);
        Assert.False(string.IsNullOrWhiteSpace(sut.Path));
    }

    [Fact]
    public void RoundTrip_WhenSerializingGetTreeFilesRequest_ShouldPreserveValues()
    {
        // Arrange
        GetTreeFilesRequest expected = _getTreeFilesRequestFixture.Create();

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        GetTreeFilesRequest? actual = JsonSerializer.Deserialize<GetTreeFilesRequest>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Deconstruct_WhenCalled_ShouldReturnAllProperties()
    {
        // Arrange
        GetTreeFilesRequest sut = _getTreeFilesRequestFixture.Create(path: @"C:\Media", includeHiddenElements: false);

        // Act
        (string? path, bool includeHiddenElements) = sut;

        // Assert
        Assert.Equal(sut.Path, path);
        Assert.Equal(sut.IncludeHiddenElements, includeHiddenElements);
    }
}
