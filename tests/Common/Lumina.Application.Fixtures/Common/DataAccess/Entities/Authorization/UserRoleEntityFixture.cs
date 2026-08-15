#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Common.DataAccess.Entities.Authorization;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Common.DataAccess.Entities.Authorization;

/// <summary>
/// Fixture class for the <see cref="UserRoleEntity"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UserRoleEntityFixture
{
    /// <summary>
    /// Creates a random valid <see cref="UserRoleEntity"/>.
    /// </summary>
    /// <returns>The created <see cref="UserRoleEntity"/>.</returns>
    public UserRoleEntity Create()
    {
        return new Faker<UserRoleEntity>()
            .CustomInstantiator(f => new UserRoleEntity
            {
                Id = f.Random.Guid(),
                UserId = f.Random.Guid(),
                User = new()
                {
                    Id = f.Random.Guid(),
                    Username = f.Internet.UserName(),
                    Password = f.Internet.Password(),
                    Libraries = [],
                    UserRole = null,
                    UserPermissions = [],
                    CreatedOnUtc = f.Date.Past(),
                    CreatedBy = f.Random.Guid()
                },
                RoleId = f.Random.Guid(),
                Role = new()
                {
                    Id = f.Random.Guid(),
                    RoleName = f.Random.String2(f.Random.Number(1, 50)),
                    CreatedOnUtc = f.Date.Past(),
                    CreatedBy = f.Random.Guid()
                },
                CreatedOnUtc = f.Date.Past(),
                CreatedBy = f.Random.Guid()
            })
            .Generate();
    }

    /// <summary>
    /// Creates a list of <see cref="UserRoleEntity"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="UserRoleEntity"/> instances.</returns>
    public List<UserRoleEntity> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }

}
