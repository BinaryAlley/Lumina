#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Fixtures.Core.Responses.FileSystemManagement.Path;
using Lumina.Contracts.Responses.FileSystemManagement.Path;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Contracts.UnitTests.Responses.FileSystemManagement.Path;

/// <summary>
/// Contains unit tests for the <see cref="PathSegmentResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class PathSegmentResponseTests
{
    private readonly PathSegmentResponseFixture _pathSegmentResponseFixture = new();

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void Create_WhenCalled_ShouldReturnValidPathSegmentResponse()
    {
        // Act
        PathSegmentResponse sut = _pathSegmentResponseFixture.Create();

        // Assert
        Assert.NotNull(sut);
        Assert.False(string.IsNullOrWhiteSpace(sut.Path));
    }

    [Fact]
    public void RoundTrip_WhenSerializingPathSegmentResponse_ShouldPreserveValues()
    {
        // Arrange
        PathSegmentResponse expected = _pathSegmentResponseFixture.Create(path: @"C:\Media");

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        PathSegmentResponse? actual = JsonSerializer.Deserialize<PathSegmentResponse>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Equality_WhenTwoInstancesHaveSameValues_ShouldBeEqual()
    {
        // Arrange
        PathSegmentResponse first = _pathSegmentResponseFixture.Create(path: @"C:\Media\Books");
        PathSegmentResponse second = _pathSegmentResponseFixture.Create(path: @"C:\Media\Books");

        // Act
        bool areEqual = first == second;

        // Assert
        Assert.True(areEqual);
    }
}
