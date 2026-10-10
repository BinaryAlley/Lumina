#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Common.DataAccess.Entities.Authorization;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.UsersManagement;
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
    private readonly UserEntityFixture _userEntityFixture = new();
    private readonly RoleEntityFixture _roleEntityFixture = new();

    /// <summary>
    /// Creates a random valid <see cref="UserRoleEntity"/>.
    /// </summary>
    /// <param name="id">Optional. The Id of the user role association.</param>
    /// <param name="userId">Optional. The Id of the user the role is granted to.</param>
    /// <param name="user">Optional. The user the role is granted to.</param>
    /// <param name="roleId">Optional. The Id of the role granted to the user.</param>
    /// <param name="role">Optional. The role granted to the user.</param>
    /// <param name="includeUser">Whether the user should be included, or forced to <see langword="null"/>.</param>
    /// <param name="createdOnUtc">Optional. The time and date when the association was created.</param>
    /// <param name="createdBy">Optional. The Id of the user that created the association.</param>
    /// <returns>The created <see cref="UserRoleEntity"/>.</returns>
    public UserRoleEntity Create(
        Guid? id = null,
        Guid? userId = null,
        UserEntity? user = null,
        Guid? roleId = null,
        RoleEntity? role = null,
        bool includeUser = true,
        DateTime? createdOnUtc = null,
        Guid? createdBy = null)
    {
        UserEntity resolvedUser = user ?? _userEntityFixture.Create(id: userId);
        RoleEntity resolvedRole = role ?? _roleEntityFixture.Create(id: roleId);

        return new Faker<UserRoleEntity>()
            .CustomInstantiator(f => new UserRoleEntity
            {
                Id = id ?? f.Random.Guid(),
                UserId = resolvedUser.Id,
                User = includeUser ? (user ?? resolvedUser) : null!,
                RoleId = resolvedRole.Id,
                Role = role ?? resolvedRole,
                CreatedOnUtc = createdOnUtc ?? f.Date.Past(),
                CreatedBy = createdBy ?? f.Random.Guid()
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
