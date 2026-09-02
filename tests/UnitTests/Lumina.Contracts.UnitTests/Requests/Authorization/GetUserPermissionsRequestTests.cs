#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Fixtures.Core.Requests.Authorization;
using Lumina.Contracts.Requests.Authorization;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Contracts.UnitTests.Requests.Authorization;

/// <summary>
/// Contains unit tests for the <see cref="GetUserPermissionsRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetUserPermissionsRequestTests
{
    private readonly GetUserPermissionsRequestFixture _getUserPermissionsRequestFixture = new();

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void Create_WhenCalled_ShouldReturnValidGetUserPermissionsRequest()
    {
        // Act
        GetUserPermissionsRequest sut = _getUserPermissionsRequestFixture.Create();

        // Assert
        Assert.NotNull(sut);
        Assert.True(sut.UserId.HasValue);
    }

    [Fact]
    public void Constructor_WhenPassingNullUserId_ShouldReturnNullUserId()
    {
        // Act
        GetUserPermissionsRequest sut = new(UserId: null);

        // Assert
        Assert.Null(sut.UserId);
    }

    [Fact]
    public void RoundTrip_WhenSerializingGetUserPermissionsRequest_ShouldPreserveValues()
    {
        // Arrange
        GetUserPermissionsRequest expected = _getUserPermissionsRequestFixture.Create();

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        GetUserPermissionsRequest? actual = JsonSerializer.Deserialize<GetUserPermissionsRequest>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(expected, actual);
    }
}
