#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Fixtures.Core.Requests.FileSystemManagement.Path;
using Lumina.Contracts.Requests.FileSystemManagement.Path;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Contracts.UnitTests.Requests.FileSystemManagement.Path;

/// <summary>
/// Contains unit tests for the <see cref="GetPathParentRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetPathParentRequestTests
{
    private readonly GetPathParentRequestFixture _getPathParentRequestFixture = new();

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void Create_WhenCalled_ShouldReturnValidGetPathParentRequest()
    {
        // Act
        GetPathParentRequest sut = _getPathParentRequestFixture.Create();

        // Assert
        Assert.NotNull(sut);
        Assert.False(string.IsNullOrWhiteSpace(sut.Path));
    }

    [Fact]
    public void RoundTrip_WhenSerializingGetPathParentRequest_ShouldPreserveValues()
    {
        // Arrange
        GetPathParentRequest expected = _getPathParentRequestFixture.Create();

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        GetPathParentRequest? actual = JsonSerializer.Deserialize<GetPathParentRequest>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Equality_WhenTwoInstancesHaveSameValues_ShouldBeEqual()
    {
        // Arrange
        GetPathParentRequest first = _getPathParentRequestFixture.Create(path: @"C:\Media\Books");
        GetPathParentRequest second = _getPathParentRequestFixture.Create(path: @"C:\Media\Books");

        // Act
        bool areEqual = first == second;

        // Assert
        Assert.True(areEqual);
    }
}
