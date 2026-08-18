#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Requests.Authorization;
using System;
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
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void RoundTrip_WhenSerializingGetUserPermissionsRequest_ShouldPreserveValues()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        GetUserPermissionsRequest expected = new(userId);

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        GetUserPermissionsRequest? actual = JsonSerializer.Deserialize<GetUserPermissionsRequest>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void RoundTrip_WhenSerializingGetUserPermissionsRequestWithNullUserId_ShouldPreserveNull()
    {
        // Arrange
        GetUserPermissionsRequest expected = new(null);

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        GetUserPermissionsRequest? actual = JsonSerializer.Deserialize<GetUserPermissionsRequest>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Null(actual.UserId);
    }
}
