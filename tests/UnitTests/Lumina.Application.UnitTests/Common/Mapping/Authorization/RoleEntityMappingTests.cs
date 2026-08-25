#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Authorization;
using Lumina.Application.Common.Mapping.Authorization;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Authorization;
using Lumina.Contracts.Responses.Authorization;
using Lumina.Domain.SharedKernel.Common.Enums.Authorization;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.Authorization;

/// <summary>
/// Contains unit tests for the <see cref="RoleEntityMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class RoleEntityMappingTests
{
    private readonly RoleEntityFixture _roleEntityFixture = new();
    private readonly RolePermissionEntityFixture _rolePermissionEntityFixture = new();
    private readonly PermissionEntityFixture _permissionEntityFixture = new();

    [Fact]
    public void ToResponse_WhenMappingValidEntity_ShouldMapCorrectly()
    {
        // Arrange
        RoleEntity entity = _roleEntityFixture.Create(roleName: "Admin");

        // Act
        RoleResponse result = entity.ToResponse();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(entity.Id, result.Id);
        Assert.Equal(entity.RoleName, result.RoleName);
    }

    [Fact]
    public void ToRolePermissionsResponse_WhenMappingValidEntity_ShouldMapCorrectly()
    {
        // Arrange
        Guid roleId = Guid.NewGuid();
        RoleEntity role = _roleEntityFixture.Create(id: roleId, roleName: "Admin");

        role.RolePermissions =
        [
            _rolePermissionEntityFixture.Create(
                roleId: roleId,
                role: role,
                permission: _permissionEntityFixture.Create(permissionName: AuthorizationPermission.CanViewUsers)),
            _rolePermissionEntityFixture.Create(
                roleId: roleId,
                role: role,
                permission: _permissionEntityFixture.Create(permissionName: AuthorizationPermission.CanDeleteUsers))
        ];

        // Act
        RolePermissionsResponse result = role.ToRolePermissionsResponse();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(role.Id, result.Role.Id);
        Assert.Equal(role.RoleName, result.Role.RoleName);
        Assert.Equal(2, result.Permissions.Length);
    }

    [Fact]
    public void ToResponses_WhenMappingMultipleEntities_ShouldMapCorrectly()
    {
        // Arrange
        List<RoleEntity> entities =
        [
            _roleEntityFixture.Create(roleName: "Admin"),
            _roleEntityFixture.Create(roleName: "User"),
            _roleEntityFixture.Create(roleName: "Guest")
        ];

        // Act
        IEnumerable<RoleResponse> results = entities.ToResponses();

        // Assert
        Assert.NotNull(results);
        Assert.Equal(3, results.Count());
        List<RoleResponse> resultList = [.. results];
        for (int i = 0; i < entities.Count; i++)
        {
            Assert.Equal(entities[i].Id, resultList[i].Id);
            Assert.Equal(entities[i].RoleName, resultList[i].RoleName);
        }
    }

    [Fact]
    public void ToResponses_WhenMappingEmptyCollection_ShouldReturnEmptyCollection()
    {
        // Arrange
        List<RoleEntity> entities = [];

        // Act
        IEnumerable<RoleResponse> results = entities.ToResponses();

        // Assert
        Assert.NotNull(results);
        Assert.Empty(results);
    }

    [Theory]
    [InlineData("Admin")]
    [InlineData("User")]
    [InlineData("Guest")]
    public void ToResponse_WhenMappingDifferentRoleNames_ShouldMapCorrectly(string roleName)
    {
        // Arrange
        RoleEntity entity = _roleEntityFixture.Create(roleName: roleName);

        // Act
        RoleResponse result = entity.ToResponse();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(entity.Id, result.Id);
        Assert.Equal(roleName, result.RoleName);
    }

    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    [InlineData("12345678-1234-1234-1234-123456789012")]
    [InlineData("FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF")]
    public void ToResponse_WhenMappingDifferentIds_ShouldMapCorrectly(string idString)
    {
        // Arrange
        RoleEntity entity = _roleEntityFixture.Create(id: Guid.Parse(idString), roleName: "Admin");

        // Act
        RoleResponse result = entity.ToResponse();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(entity.Id, result.Id);
        Assert.Equal(entity.RoleName, result.RoleName);
    }
}
