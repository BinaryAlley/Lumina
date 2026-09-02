#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Fixtures.Core.Requests.Authorization;
using Lumina.Contracts.Requests.Authorization;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Contracts.UnitTests.Requests.Authorization;

/// <summary>
/// Contains unit tests for the <see cref="GetUserRoleRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetUserRoleRequestTests
{
    private readonly GetUserRoleRequestFixture _getUserRoleRequestFixture = new();

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void Create_WhenCalled_ShouldReturnValidGetUserRoleRequest()
    {
        // Act
        GetUserRoleRequest sut = _getUserRoleRequestFixture.Create();

        // Assert
        Assert.NotNull(sut);
        Assert.True(sut.UserId.HasValue);
    }

    [Fact]
    public void Constructor_WhenPassingNullUserId_ShouldReturnNullUserId()
    {
        // Act
        GetUserRoleRequest sut = new(UserId: null);

        // Assert
        Assert.Null(sut.UserId);
    }

    [Fact]
    public void RoundTrip_WhenSerializingGetUserRoleRequest_ShouldPreserveValues()
    {
        // Arrange
        GetUserRoleRequest expected = _getUserRoleRequestFixture.Create();

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        GetUserRoleRequest? actual = JsonSerializer.Deserialize<GetUserRoleRequest>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(expected, actual);
    }
}
