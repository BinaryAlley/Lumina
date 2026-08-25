#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Common.DataAccess.Entities.Authorization;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using System;
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
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="UserRoleEntity"/>.
    /// </summary>
    /// <param name="id">Optional. The Id of the user role association.</param>
    /// <param name="userId">Optional. The Id of the user the role is granted to.</param>
    /// <param name="user">Optional. The user the role is granted to.</param>
    /// <param name="roleId">Optional. The Id of the role granted to the user.</param>
    /// <param name="role">Optional. The role granted to the user.</param>
    /// <returns>The created <see cref="UserRoleEntity"/>.</returns>
    public UserRoleEntity Create(
        Guid? id = null,
        Guid? userId = null,
        UserEntity? user = null,
        Guid? roleId = null,
        RoleEntity? role = null)
    {
        UserEntity resolvedUser = user ?? new UserEntity
        {
            Id = userId ?? _faker.Random.Guid(),
            Username = _faker.Internet.UserName(),
            Password = _faker.Internet.Password(),
            Libraries = [],
            UserRole = null,
            UserPermissions = [],
            CreatedOnUtc = _faker.Date.Past(),
            CreatedBy = _faker.Random.Guid()
        };

        RoleEntity resolvedRole = role ?? new RoleEntity
        {
            Id = roleId ?? _faker.Random.Guid(),
            RoleName = _faker.Random.String2(_faker.Random.Number(1, 50)),
            CreatedOnUtc = _faker.Date.Past(),
            CreatedBy = _faker.Random.Guid()
        };

        return new Faker<UserRoleEntity>()
            .CustomInstantiator(f => new UserRoleEntity
            {
                Id = id ?? f.Random.Guid(),
                UserId = resolvedUser.Id,
                User = user ?? resolvedUser,
                RoleId = resolvedRole.Id,
                Role = role ?? resolvedRole,
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
