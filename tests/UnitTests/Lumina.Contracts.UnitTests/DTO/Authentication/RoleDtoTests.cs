#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.Authentication;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Contracts.UnitTests.DTO.Authentication;

/// <summary>
/// Contains unit tests for the <see cref="RoleDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class RoleDtoTests
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void RoundTrip_WhenSerializingRole_ShouldPreserveValues()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        RoleDto expected = new(id, "Admin");

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        RoleDto? actual = JsonSerializer.Deserialize<RoleDto>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Equality_WhenTwoInstancesHaveSameValues_ShouldBeEqual()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        RoleDto first = new(id, "Admin");
        RoleDto second = new(id, "Admin");

        // Act
        bool areEqual = first == second;

        // Assert
        Assert.True(areEqual);
    }
}
