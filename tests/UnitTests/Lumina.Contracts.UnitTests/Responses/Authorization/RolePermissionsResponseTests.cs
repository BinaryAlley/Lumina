#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.Authentication;
using Lumina.Contracts.Fixtures.Core.Responses.Authorization;
using Lumina.Contracts.Responses.Authorization;
using Lumina.Domain.SharedKernel.Common.Enums.Authorization;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
#endregion

namespace Lumina.Contracts.UnitTests.Responses.Authorization;

/// <summary>
/// Contains unit tests for the <see cref="RolePermissionsResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class RolePermissionsResponseTests
{
    private readonly RolePermissionsResponseFixture _rolePermissionsResponseFixture = new();
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    [Fact]
    public void RoundTrip_WhenSerializingRolePermissionsResponse_ShouldPreserveValues()
    {
        // Arrange
        RolePermissionsResponse expected = _rolePermissionsResponseFixture.Create();

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        RolePermissionsResponse? actual = JsonSerializer.Deserialize<RolePermissionsResponse>(json, _jsonOptions);
        // Assert
        Assert.NotNull(actual);
        Assert.Equivalent(expected, actual);
    }

    [Fact]
    public void Serialize_WhenSerializingRolePermissionsResponse_ShouldSerializePermissionNameAsCamelCaseString()
    {
        // Arrange
        RolePermissionsResponse sut = _rolePermissionsResponseFixture.Create(permissions:
        [
            new PermissionDto(Guid.NewGuid(), AuthorizationPermission.CanDeleteUsers)
        ]);

        // Act
        string json = JsonSerializer.Serialize(sut, _jsonOptions);

        // Assert
        Assert.Contains("canDeleteUsers", json, StringComparison.Ordinal);
    }
}
