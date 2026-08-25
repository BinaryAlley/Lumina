#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Common.DataAccess.Entities.Authorization;
using Lumina.Domain.SharedKernel.Common.Enums.Authorization;
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
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="RolePermissionEntity"/>.
    /// </summary>
    /// <param name="id">Optional. The Id of the role permission association.</param>
    /// <param name="roleId">Optional. The Id of the role the permission is granted to.</param>
    /// <param name="role">Optional. The role the permission is granted to.</param>
    /// <param name="permissionId">Optional. The Id of the permission granted to the role.</param>
    /// <param name="permission">Optional. The permission granted to the role.</param>
    /// <returns>The created <see cref="RolePermissionEntity"/>.</returns>
    public RolePermissionEntity Create(
        Guid? id = null,
        Guid? roleId = null,
        RoleEntity? role = null,
        Guid? permissionId = null,
        PermissionEntity? permission = null)
    {
        RoleEntity resolvedRole = role ?? new RoleEntity
        {
            Id = roleId ?? _faker.Random.Guid(),
            RoleName = _faker.Random.String2(10),
            CreatedOnUtc = _faker.Date.Past(),
            CreatedBy = _faker.Random.Guid(),
            UpdatedOnUtc = _faker.Date.Recent(),
            UpdatedBy = _faker.Random.Guid()
        };

        PermissionEntity resolvedPermission = permission ?? new PermissionEntity
        {
            Id = permissionId ?? _faker.Random.Guid(),
            PermissionName = _faker.PickRandom<AuthorizationPermission>(),
            CreatedOnUtc = _faker.Date.Past(),
            CreatedBy = _faker.Random.Guid(),
            UpdatedOnUtc = _faker.Date.Recent(),
            UpdatedBy = _faker.Random.Guid()
        };

        return new Faker<RolePermissionEntity>()
            .CustomInstantiator(f => new RolePermissionEntity
            {
                Id = id ?? f.Random.Guid(),
                RoleId = resolvedRole.Id,
                Role = role ?? resolvedRole,
                PermissionId = resolvedPermission.Id,
                Permission = permission ?? resolvedPermission,
                CreatedOnUtc = f.Date.Past(),
                CreatedBy = f.Random.Guid(),
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
