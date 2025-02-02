#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Authorization;
using Lumina.Application.Common.Mapping.Authorization;
using Lumina.Contracts.Responses.Authorization;
using Lumina.Domain.Common.Enums.Authorization;
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
    [Fact]
    public void ToResponse_WhenMappingValidEntity_ShouldMapCorrectly()
    {
        // Arrange
        RoleEntity entity = new()
        {
            Id = Guid.NewGuid(),
            RoleName = "Admin"
        };

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
        RoleEntity role = new()
        {
            Id = roleId,
            RoleName = "Admin"
        };

        role.RolePermissions =
        [
            new()
            {
                PermissionId = Guid.NewGuid(),
                RoleId = roleId,
                Role = role,
                Permission = new()
                {
                    Id = Guid.NewGuid(),
                    PermissionName = AuthorizationPermission.CanViewUsers
                }
            },
            new()
            {
                PermissionId = Guid.NewGuid(),
                RoleId = roleId,
                Role = role,
                Permission = new()
                {
                    Id = Guid.NewGuid(),
                    PermissionName = AuthorizationPermission.CanDeleteUsers
                }
            }
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
            new() { Id = Guid.NewGuid(), RoleName = "Admin" },
            new() { Id = Guid.NewGuid(), RoleName = "User" },
            new() { Id = Guid.NewGuid(), RoleName = "Guest" }
        ];

        // Act
        IEnumerable<RoleResponse> results = entities.ToResponses();

        // Assert
        Assert.NotNull(results);
        Assert.Equal(3, results.Count());
        List<RoleResponse> resultList = results.ToList();
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
        RoleEntity entity = new()
        {
            Id = Guid.NewGuid(),
            RoleName = roleName
        };

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
        RoleEntity entity = new()
        {
            Id = Guid.Parse(idString),
            RoleName = "Admin"
        };

        // Act
        RoleResponse result = entity.ToResponse();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(entity.Id, result.Id);
        Assert.Equal(entity.RoleName, result.RoleName);
    }
}
