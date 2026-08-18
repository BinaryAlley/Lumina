#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Responses.FileSystemManagement.Path;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Contracts.UnitTests.Responses.FileSystemManagement.Path;

/// <summary>
/// Contains unit tests for the <see cref="PathExistsResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class PathExistsResponseTests
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void RoundTrip_WhenSerializingPathExistsResponse_ShouldPreserveValues()
    {
        // Arrange
        PathExistsResponse expected = new(true);

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        PathExistsResponse? actual = JsonSerializer.Deserialize<PathExistsResponse>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Equality_WhenTwoInstancesHaveSameValues_ShouldBeEqual()
    {
        // Arrange
        PathExistsResponse first = new(true);
        PathExistsResponse second = new(true);

        // Act
        bool areEqual = first == second;

        // Assert
        Assert.True(areEqual);
    }
}
