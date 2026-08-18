#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.Authentication;
using Lumina.Contracts.Fixtures.Core.DTO.Authentication;
using Lumina.Domain.SharedKernel.Common.Enums.Authorization;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
#endregion

namespace Lumina.Contracts.UnitTests.DTO.Authentication;

/// <summary>
/// Contains unit tests for the <see cref="PermissionDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class PermissionDtoTests
{
    private readonly PermissionDtoFixture _permissionDtoFixture = new();
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    [Fact]
    public void RoundTrip_WhenSerializingPermission_ShouldPreserveValues()
    {
        // Arrange
        PermissionDto expected = _permissionDtoFixture.Create(permissionName: AuthorizationPermission.CanViewUsers);

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        PermissionDto? actual = JsonSerializer.Deserialize<PermissionDto>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Serialize_WhenSerializingPermission_ShouldSerializeEnumAsCamelCaseString()
    {
        // Arrange
        PermissionDto sut = _permissionDtoFixture.Create(permissionName: AuthorizationPermission.CanDeleteUsers);

        // Act
        string json = JsonSerializer.Serialize(sut, _jsonOptions);

        // Assert
        Assert.Contains("\"PermissionName\":\"canDeleteUsers\"", json, StringComparison.Ordinal);
    }

    [Fact]
    public void Equality_WhenTwoInstancesHaveSameValues_ShouldBeEqual()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        PermissionDto first = _permissionDtoFixture.Create(id: id, permissionName: AuthorizationPermission.CanRegisterUsers);
        PermissionDto second = _permissionDtoFixture.Create(id: id, permissionName: AuthorizationPermission.CanRegisterUsers);

        // Act
        bool areEqual = first == second;

        // Assert
        Assert.True(areEqual);
    }
}
