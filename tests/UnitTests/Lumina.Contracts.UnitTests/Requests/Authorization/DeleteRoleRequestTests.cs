#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Requests.Authorization;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Contracts.UnitTests.Requests.Authorization;

/// <summary>
/// Contains unit tests for the <see cref="DeleteRoleRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class DeleteRoleRequestTests
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void RoundTrip_WhenSerializingDeleteRoleRequest_ShouldPreserveValues()
    {
        // Arrange
        Guid roleId = Guid.NewGuid();
        DeleteRoleRequest expected = new(roleId);

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        DeleteRoleRequest? actual = JsonSerializer.Deserialize<DeleteRoleRequest>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void RoundTrip_WhenSerializingDeleteRoleRequestWithNullRoleId_ShouldPreserveNull()
    {
        // Arrange
        DeleteRoleRequest expected = new(null);

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        DeleteRoleRequest? actual = JsonSerializer.Deserialize<DeleteRoleRequest>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Null(actual.RoleId);
    }
}
