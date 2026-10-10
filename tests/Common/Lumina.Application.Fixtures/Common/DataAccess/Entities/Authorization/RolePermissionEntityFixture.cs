#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Common.DataAccess.Entities.Authorization;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Common.DataAccess.Entities.Authorization;

/// <summary>
/// Fixture class for the <see cref="RolePermissionEntity"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class RolePermissionEntityFixture
{
    // RoleEntityFixture composes this fixture, so the role fixture is resolved lazily to avoid an eager mutual composition cycle.
    private readonly Lazy<RoleEntityFixture> _roleEntityFixture = new();
    private readonly PermissionEntityFixture _permissionEntityFixture = new();

    /// <summary>
    /// Creates a random valid <see cref="RolePermissionEntity"/>.
    /// </summary>
    /// <param name="id">Optional. The Id of the role permission association.</param>
    /// <param name="roleId">Optional. The Id of the role the permission is granted to.</param>
    /// <param name="role">Optional. The role the permission is granted to.</param>
    /// <param name="permissionId">Optional. The Id of the permission granted to the role.</param>
    /// <param name="permission">Optional. The permission granted to the role.</param>
    /// <param name="createdOnUtc">Optional. The time and date when the association was created.</param>
    /// <param name="createdBy">Optional. The Id of the user that created the association.</param>
    /// <returns>The created <see cref="RolePermissionEntity"/>.</returns>
    public RolePermissionEntity Create(
        Guid? id = null,
        Guid? roleId = null,
        RoleEntity? role = null,
        Guid? permissionId = null,
        PermissionEntity? permission = null,
        DateTime? createdOnUtc = null,
        Guid? createdBy = null)
    {
        RoleEntity resolvedRole = role ?? _roleEntityFixture.Value.Create(id: roleId);
        PermissionEntity resolvedPermission = permission ?? _permissionEntityFixture.Create(id: permissionId);

        return new Faker<RolePermissionEntity>()
            .CustomInstantiator(f => new RolePermissionEntity
            {
                Id = id ?? f.Random.Guid(),
                RoleId = resolvedRole.Id,
                Role = role ?? resolvedRole,
                PermissionId = resolvedPermission.Id,
                Permission = permission ?? resolvedPermission,
                CreatedOnUtc = createdOnUtc ?? f.Date.Past(),
                CreatedBy = createdBy ?? f.Random.Guid(),
                UpdatedOnUtc = f.Date.Recent(),
                UpdatedBy = f.Random.Guid()
            })
            .Generate();
    }

    /// <summary>
    /// Creates a list of <see cref="RolePermissionEntity"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="RolePermissionEntity"/> instances.</returns>
    public List<RolePermissionEntity> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }

}
