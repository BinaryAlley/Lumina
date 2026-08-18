#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Fixtures.Core.Requests.FileSystemManagement.Path;
using Lumina.Contracts.Requests.FileSystemManagement.Path;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Contracts.UnitTests.Requests.FileSystemManagement.Path;

/// <summary>
/// Contains unit tests for the <see cref="CheckPathExistsRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class CheckPathExistsRequestTests
{
    private readonly CheckPathExistsRequestFixture _checkPathExistsRequestFixture = new();

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void Create_WhenCalled_ShouldReturnValidCheckPathExistsRequest()
    {
        // Act
        CheckPathExistsRequest sut = _checkPathExistsRequestFixture.Create();

        // Assert
        Assert.NotNull(sut);
        Assert.False(string.IsNullOrWhiteSpace(sut.Path));
    }

    [Fact]
    public void RoundTrip_WhenSerializingCheckPathExistsRequest_ShouldPreserveValues()
    {
        // Arrange
        CheckPathExistsRequest expected = _checkPathExistsRequestFixture.Create();

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        CheckPathExistsRequest? actual = JsonSerializer.Deserialize<CheckPathExistsRequest>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Deconstruct_WhenCalled_ShouldReturnAllProperties()
    {
        // Arrange
        CheckPathExistsRequest sut = _checkPathExistsRequestFixture.Create(path: @"C:\Media\Books", includeHiddenElements: true);

        // Act
        (string? path, bool includeHiddenElements) = sut;

        // Assert
        Assert.Equal(sut.Path, path);
        Assert.Equal(sut.IncludeHiddenElements, includeHiddenElements);
    }
}
