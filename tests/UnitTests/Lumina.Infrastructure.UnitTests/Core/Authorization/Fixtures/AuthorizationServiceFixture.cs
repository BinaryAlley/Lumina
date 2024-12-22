#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Authorization;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Domain.Common.Enums.Authorization;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Infrastructure.UnitTests.Core.Authorization.Fixtures;

/// <summary>
/// Fixture class for the <see cref="AuthorizationService"/> tests.
/// </summary>
[ExcludeFromCodeCoverage]
public class AuthorizationServiceFixture
{
    /// <summary>
    /// Creates a user entity with specified permissions and roles.
    /// </summary>
    /// <param name="directPermissions">Direct permissions to assign to the user.</param>
    /// <param name="rolePermissions">Permissions to assign through roles.</param>
    /// <returns>The created user entity.</returns>
    public static UserEntity CreateUserWithPermissions(
        IEnumerable<AuthorizationPermission>? directPermissions = null,
        Dictionary<string, IEnumerable<AuthorizationPermission>>? rolePermissions = null)
    {
        Guid userId = Guid.NewGuid();
        DateTime utcNow = DateTime.UtcNow;
        List<UserPermissionEntity> userPermissions = [];
        UserRoleEntity? userRole = null;

        // create role first if rolePermissions is provided
        if (rolePermissions is not null)
        {
            foreach (KeyValuePair<string, IEnumerable<AuthorizationPermission>> rolePerm in rolePermissions)
            {
                Guid roleId = Guid.NewGuid();
                RoleEntity role = new()
                {
                    Id = roleId,
                    RoleName = rolePerm.Key,
                    CreatedOnUtc = utcNow,
                    CreatedBy = userId,
                    RolePermissions = []
                };

                foreach (AuthorizationPermission permission in rolePerm.Value)
                {
                    Guid permissionId = Guid.NewGuid();
                    PermissionEntity permissionEntity = new()
                    {
                        Id = permissionId,
                        PermissionName = permission,
                        CreatedOnUtc = utcNow,
                        CreatedBy = userId
                    };

                    role.RolePermissions.Add(new RolePermissionEntity
                    {
                        Id = Guid.NewGuid(),
                        RoleId = roleId,
                        Role = role,
                        PermissionId = permissionId,
                        Permission = permissionEntity,
                        CreatedOnUtc = utcNow,
                        CreatedBy = userId
                    });
                }

                userRole = new UserRoleEntity
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    RoleId = roleId,
                    Role = role,
                    CreatedOnUtc = utcNow,
                    CreatedBy = userId,
                    User = null!
                };
            }
        }

        UserEntity user = new()
        {
            Id = userId,
            Username = "test-user",
            Password = "hashed-password",
            CreatedOnUtc = utcNow,
            CreatedBy = userId,
            Libraries = [],
            UserPermissions = userPermissions,
            UserRole = userRole
        };

        if (directPermissions is not null)
        {
            foreach (AuthorizationPermission permission in directPermissions)
            {
                Guid permissionId = Guid.NewGuid();
                PermissionEntity permissionEntity = new()
                {
                    Id = permissionId,
                    PermissionName = permission,
                    CreatedOnUtc = utcNow,
                    CreatedBy = userId
                };

                userPermissions.Add(new UserPermissionEntity
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    User = user,
                    PermissionId = permissionId,
                    Permission = permissionEntity,
                    CreatedOnUtc = utcNow,
                    CreatedBy = userId
                });
            }
        }

        if (rolePermissions is not null)
        {
            foreach (KeyValuePair<string, IEnumerable<AuthorizationPermission>> rolePerm in rolePermissions)
            {
                Guid roleId = Guid.NewGuid();
                RoleEntity role = new()
                {
                    Id = roleId,
                    RoleName = rolePerm.Key,
                    CreatedOnUtc = utcNow,
                    CreatedBy = userId,
                    RolePermissions = []
                };

                foreach (AuthorizationPermission permission in rolePerm.Value)
                {
                    Guid permissionId = Guid.NewGuid();
                    PermissionEntity permissionEntity = new()
                    {
                        Id = permissionId,
                        PermissionName = permission,
                        CreatedOnUtc = utcNow,
                        CreatedBy = userId
                    };

                    role.RolePermissions.Add(new RolePermissionEntity
                    {
                        Id = Guid.NewGuid(),
                        RoleId = roleId,
                        Role = role,
                        PermissionId = permissionId,
                        Permission = permissionEntity,
                        CreatedOnUtc = utcNow,
                        CreatedBy = userId
                    });
                }

                userRole = new UserRoleEntity
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    User = user,
                    RoleId = roleId,
                    Role = role,
                    CreatedOnUtc = utcNow,
                    CreatedBy = userId
                };
            }
        }

        return user;
    }
}
