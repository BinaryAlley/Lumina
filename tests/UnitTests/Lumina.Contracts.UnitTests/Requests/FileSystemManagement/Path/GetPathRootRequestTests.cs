#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Fixtures.Core.Requests.FileSystemManagement.Path;
using Lumina.Contracts.Requests.FileSystemManagement.Path;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Contracts.UnitTests.Requests.FileSystemManagement.Path;

/// <summary>
/// Contains unit tests for the <see cref="GetPathRootRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetPathRootRequestTests
{
    private readonly GetPathRootRequestFixture _getPathRootRequestFixture = new();

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void Create_WhenCalled_ShouldReturnValidGetPathRootRequest()
    {
        // Act
        GetPathRootRequest sut = _getPathRootRequestFixture.Create();

        // Assert
        Assert.NotNull(sut);
        Assert.False(string.IsNullOrWhiteSpace(sut.Path));
    }

    [Fact]
    public void RoundTrip_WhenSerializingGetPathRootRequest_ShouldPreserveValues()
    {
        // Arrange
        GetPathRootRequest expected = _getPathRootRequestFixture.Create();

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        GetPathRootRequest? actual = JsonSerializer.Deserialize<GetPathRootRequest>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Equality_WhenTwoInstancesHaveSameValues_ShouldBeEqual()
    {
        // Arrange
        GetPathRootRequest first = _getPathRootRequestFixture.Create(path: @"C:\Media\Books");
        GetPathRootRequest second = _getPathRootRequestFixture.Create(path: @"C:\Media\Books");

        // Act
        bool areEqual = first == second;

        // Assert
        Assert.True(areEqual);
    }
}
