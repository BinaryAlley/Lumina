#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Authorization;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Authorization;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.UsersManagement;
using Lumina.Domain.SharedKernel.Common.Enums.Authorization;
using Lumina.Infrastructure.Core.Authorization;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Infrastructure.Fixtures.Core.Authorization;

/// <summary>
/// Test-support class for the <see cref="AuthorizationService"/> tests.
/// </summary>
[ExcludeFromCodeCoverage]
public class AuthorizationServiceFixture
{
    private readonly UserEntityFixture _userEntityFixture = new();
    private readonly RoleEntityFixture _roleEntityFixture = new();
    private readonly PermissionEntityFixture _permissionEntityFixture = new();
    private readonly RolePermissionEntityFixture _rolePermissionEntityFixture = new();
    private readonly UserRoleEntityFixture _userRoleEntityFixture = new();
    private readonly UserPermissionEntityFixture _userPermissionEntityFixture = new();

    /// <summary>
    /// Creates a user entity with specified permissions and roles.
    /// </summary>
    /// <param name="directPermissions">Direct permissions to assign to the user.</param>
    /// <param name="rolePermissions">Permissions to assign through roles.</param>
    /// <returns>The created user entity.</returns>
    public UserEntity CreateUserWithPermissions(
        IEnumerable<AuthorizationPermission>? directPermissions = null,
        Dictionary<string, IEnumerable<AuthorizationPermission>>? rolePermissions = null)
    {
        Guid userId = Guid.NewGuid();
        DateTime utcNow = DateTime.UtcNow;
        UserRoleEntity? userRole = null;

        if (rolePermissions is not null)
        {
            foreach (KeyValuePair<string, IEnumerable<AuthorizationPermission>> rolePerm in rolePermissions)
            {
                Guid roleId = Guid.NewGuid();
                RoleEntity role = _roleEntityFixture.Create(
                    id: roleId,
                    roleName: rolePerm.Key,
                    createdBy: userId,
                    createdOnUtc: utcNow);

                foreach (AuthorizationPermission permission in rolePerm.Value)
                {
                    Guid permissionId = Guid.NewGuid();
                    PermissionEntity permissionEntity = _permissionEntityFixture.Create(
                        id: permissionId,
                        permissionName: permission,
                        createdBy: userId,
                        createdOnUtc: utcNow);

                    role.RolePermissions.Add(_rolePermissionEntityFixture.Create(
                        role: role,
                        permission: permissionEntity,
                        createdBy: userId,
                        createdOnUtc: utcNow));
                }

                userRole = _userRoleEntityFixture.Create(
                    userId: userId,
                    roleId: roleId,
                    role: role,
                    includeUser: false,
                    createdBy: userId,
                    createdOnUtc: utcNow);
            }
        }

        UserEntity user = _userEntityFixture.Create(
            username: "test-user",
            password: "hashed-password",
            id: userId,
            userRole: userRole,
            includeUserRole: true,
            createdBy: userId,
            createdOnUtc: utcNow);

        if (directPermissions is not null)
        {
            foreach (AuthorizationPermission permission in directPermissions)
            {
                Guid permissionId = Guid.NewGuid();
                PermissionEntity permissionEntity = _permissionEntityFixture.Create(
                    id: permissionId,
                    permissionName: permission,
                    createdBy: userId,
                    createdOnUtc: utcNow);

                user.UserPermissions.Add(_userPermissionEntityFixture.Create(
                    user,
                    permissionEntity,
                    createdOnUtc: utcNow,
                    createdBy: userId));
            }
        }

        return user;
    }
}