#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Fixtures.Core.Requests.FileSystemManagement.Directories;
using Lumina.Contracts.Requests.FileSystemManagement.Directories;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Contracts.UnitTests.Requests.FileSystemManagement.Directories;

/// <summary>
/// Contains unit tests for the <see cref="GetDirectoriesRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetDirectoriesRequestTests
{
    private readonly GetDirectoriesRequestFixture _getDirectoriesRequestFixture = new();

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void Create_WhenCalled_ShouldReturnValidGetDirectoriesRequest()
    {
        // Act
        GetDirectoriesRequest sut = _getDirectoriesRequestFixture.Create();

        // Assert
        Assert.NotNull(sut);
        Assert.False(string.IsNullOrWhiteSpace(sut.Path));
    }

    [Fact]
    public void RoundTrip_WhenSerializingGetDirectoriesRequest_ShouldPreserveValues()
    {
        // Arrange
        GetDirectoriesRequest expected = _getDirectoriesRequestFixture.Create();

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        GetDirectoriesRequest? actual = JsonSerializer.Deserialize<GetDirectoriesRequest>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Deconstruct_WhenCalled_ShouldReturnAllProperties()
    {
        // Arrange
        GetDirectoriesRequest sut = _getDirectoriesRequestFixture.Create(path: @"C:\Media", includeHiddenElements: true);

        // Act
        (string? path, bool includeHiddenElements) = sut;

        // Assert
        Assert.Equal(sut.Path, path);
        Assert.Equal(sut.IncludeHiddenElements, includeHiddenElements);
    }
}
