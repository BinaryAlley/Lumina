#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Fixtures.Core.Requests.FileSystemManagement.Files;
using Lumina.Contracts.Requests.FileSystemManagement.Files;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Contracts.UnitTests.Requests.FileSystemManagement.Files;

/// <summary>
/// Contains unit tests for the <see cref="GetFilesRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetFilesRequestTests
{
    private readonly GetFilesRequestFixture _getFilesRequestFixture = new();

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void Create_WhenCalled_ShouldReturnValidGetFilesRequest()
    {
        // Act
        GetFilesRequest sut = _getFilesRequestFixture.Create();

        // Assert
        Assert.NotNull(sut);
        Assert.False(string.IsNullOrWhiteSpace(sut.Path));
    }

    [Fact]
    public void RoundTrip_WhenSerializingGetFilesRequest_ShouldPreserveValues()
    {
        // Arrange
        GetFilesRequest expected = _getFilesRequestFixture.Create();

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        GetFilesRequest? actual = JsonSerializer.Deserialize<GetFilesRequest>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Deconstruct_WhenCalled_ShouldReturnAllProperties()
    {
        // Arrange
        GetFilesRequest sut = _getFilesRequestFixture.Create(path: @"C:\Media", includeHiddenElements: true);

        // Act
        (string? path, bool includeHiddenElements) = sut;

        // Assert
        Assert.Equal(sut.Path, path);
        Assert.Equal(sut.IncludeHiddenElements, includeHiddenElements);
    }
}
