#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Responses.Authorization;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Contracts.UnitTests.Responses.Authorization;

/// <summary>
/// Contains unit tests for the <see cref="RoleResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class RoleResponseTests
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void RoundTrip_WhenSerializingRoleResponse_ShouldPreserveValues()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        RoleResponse expected = new(id, "Admin");

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        RoleResponse? actual = JsonSerializer.Deserialize<RoleResponse>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Equality_WhenTwoInstancesHaveSameValues_ShouldBeEqual()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        RoleResponse first = new(id, "Admin");
        RoleResponse second = new(id, "Admin");

        // Act
        bool areEqual = first == second;

        // Assert
        Assert.True(areEqual);
    }
}
