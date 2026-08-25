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
/// Fixture class for the <see cref="RoleEntity"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class RoleEntityFixture
{
    private readonly RolePermissionEntityFixture _rolePermissionEntityFixture = new();

    /// <summary>
    /// Creates a random valid <see cref="RoleEntity"/>.
    /// </summary>
    /// <param name="id">Optional. The Id of the role.</param>
    /// <param name="roleName">Optional. The name of the role.</param>
    /// <param name="rolePermissions">Optional. The role permission associations of the role.</param>
    /// <param name="includeRolePermissions">Whether the role should include role permission associations or not.</param>
    /// <param name="createdBy">Optional. The Id of the user that created the role.</param>
    /// <param name="createdOnUtc">Optional. The time and date when the role was added.</param>
    /// <returns>The created <see cref="RoleEntity"/>.</returns>
    public RoleEntity Create(
        Guid? id = null,
        string? roleName = null,
        IEnumerable<RolePermissionEntity>? rolePermissions = null,
        bool includeRolePermissions = false,
        Guid? createdBy = null,
        DateTime? createdOnUtc = null)
    {
        return new Faker<RoleEntity>()
            .CustomInstantiator(faker => new RoleEntity
            {
                Id = id ?? faker.Random.Guid(),
                RoleName = roleName ?? faker.Random.String2(faker.Random.Number(1, 50)),
                RolePermissions = includeRolePermissions ? (rolePermissions ?? _rolePermissionEntityFixture.CreateMany()).ToList() : [],
                CreatedOnUtc = createdOnUtc ?? faker.Date.Past(),
                CreatedBy = createdBy ?? faker.Random.Guid(),
                UpdatedOnUtc = null,
                UpdatedBy = null
            })
            .Generate();
    }

    /// <summary>
    /// Creates a list of <see cref="RoleEntity"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="RoleEntity"/> instances.</returns>
    public List<RoleEntity> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
