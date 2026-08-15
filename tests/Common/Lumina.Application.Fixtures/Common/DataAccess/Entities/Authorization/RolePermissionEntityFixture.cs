#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Common.DataAccess.Entities.Authorization;
using Lumina.Domain.SharedKernel.Common.Enums.Authorization;
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
    private readonly Faker _faker;

    /// <summary>
    /// Initializes a new instance of the <see cref="RolePermissionEntityFixture"/> class.
    /// </summary>
    public RolePermissionEntityFixture()
    {
        _faker = new Faker();
    }

    /// <summary>
    /// Creates a random valid <see cref="RolePermissionEntity"/>.
    /// </summary>
    /// <returns>The created <see cref="RolePermissionEntity"/>.</returns>
    public RolePermissionEntity Create()
    {
        PermissionEntity permission = new()
        {
            Id = _faker.Random.Guid(),
            PermissionName = _faker.PickRandom<AuthorizationPermission>(),
            CreatedOnUtc = _faker.Date.Past(),
            CreatedBy = _faker.Random.Guid(),
            UpdatedOnUtc = _faker.Date.Recent(),
            UpdatedBy = _faker.Random.Guid()
        };

        RoleEntity role = new()
        {
            Id = _faker.Random.Guid(),
            RoleName = _faker.Random.String2(10),
            CreatedOnUtc = _faker.Date.Past(),
            CreatedBy = _faker.Random.Guid(),
            UpdatedOnUtc = _faker.Date.Recent(),
            UpdatedBy = _faker.Random.Guid()
        };

        return new Faker<RolePermissionEntity>()
            .CustomInstantiator(f => new RolePermissionEntity
            {
                Id = f.Random.Guid(),
                RoleId = role.Id,
                Role = role,
                PermissionId = permission.Id,
                Permission = permission,
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
