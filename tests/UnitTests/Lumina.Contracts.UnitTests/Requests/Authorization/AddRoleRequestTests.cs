#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Requests.Authorization;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Contracts.UnitTests.Requests.Authorization;

/// <summary>
/// Contains unit tests for the <see cref="AddRoleRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddRoleRequestTests
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void RoundTrip_WhenSerializingAddRoleRequest_ShouldPreserveValues()
    {
        // Arrange
        List<Guid> permissions = [Guid.NewGuid(), Guid.NewGuid()];
        AddRoleRequest expected = new("Admin", permissions);

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        AddRoleRequest? actual = JsonSerializer.Deserialize<AddRoleRequest>(json, _jsonOptions);
        // Assert
        Assert.NotNull(actual);
        Assert.Equivalent(expected, actual);
    }

    [Fact]
    public void RoundTrip_WhenSerializingAddRoleRequestWithNullPermissions_ShouldPreserveNull()
    {
        // Arrange
        AddRoleRequest expected = new("Admin", null);

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        AddRoleRequest? actual = JsonSerializer.Deserialize<AddRoleRequest>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Null(actual.Permissions);
    }
}
