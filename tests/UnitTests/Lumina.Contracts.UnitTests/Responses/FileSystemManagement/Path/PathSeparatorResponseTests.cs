#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Fixtures.Core.Responses.FileSystemManagement.Path;
using Lumina.Contracts.Responses.FileSystemManagement.Path;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Contracts.UnitTests.Responses.FileSystemManagement.Path;

/// <summary>
/// Contains unit tests for the <see cref="PathSeparatorResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class PathSeparatorResponseTests
{
    private readonly PathSeparatorResponseFixture _pathSeparatorResponseFixture = new();

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void Create_WhenCalled_ShouldReturnValidPathSeparatorResponse()
    {
        // Act
        PathSeparatorResponse sut = _pathSeparatorResponseFixture.Create();

        // Assert
        Assert.NotNull(sut);
        Assert.False(string.IsNullOrWhiteSpace(sut.Separator));
    }

    [Fact]
    public void RoundTrip_WhenSerializingPathSeparatorResponse_ShouldPreserveValues()
    {
        // Arrange
        PathSeparatorResponse expected = _pathSeparatorResponseFixture.Create(separator: "\\");

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        PathSeparatorResponse? actual = JsonSerializer.Deserialize<PathSeparatorResponse>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Equality_WhenTwoInstancesHaveSameValues_ShouldBeEqual()
    {
        // Arrange
        PathSeparatorResponse first = _pathSeparatorResponseFixture.Create(separator: "/");
        PathSeparatorResponse second = _pathSeparatorResponseFixture.Create(separator: "/");

        // Act
        bool areEqual = first == second;

        // Assert
        Assert.True(areEqual);
    }
}
