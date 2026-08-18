#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Requests.Authorization;
using System;
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
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void RoundTrip_WhenSerializingGetUserRoleRequest_ShouldPreserveValues()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        GetUserRoleRequest expected = new(userId);

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        GetUserRoleRequest? actual = JsonSerializer.Deserialize<GetUserRoleRequest>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void RoundTrip_WhenSerializingGetUserRoleRequestWithNullUserId_ShouldPreserveNull()
    {
        // Arrange
        GetUserRoleRequest expected = new(null);

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        GetUserRoleRequest? actual = JsonSerializer.Deserialize<GetUserRoleRequest>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Null(actual.UserId);
    }
}
