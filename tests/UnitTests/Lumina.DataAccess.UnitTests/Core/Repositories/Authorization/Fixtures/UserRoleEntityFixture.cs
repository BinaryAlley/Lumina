#region ========================================================================= USING =====================================================================================
using AutoFixture;
using Bogus;
using Lumina.Application.Common.DataAccess.Entities.Authorization;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.DataAccess.UnitTests.Core.Repositories.Authorization.Fixtures;


/// <summary>
/// Fixture class for the <see cref="UserRoleEntity"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UserRoleEntityFixture
{
    private readonly Fixture _fixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserRoleEntityFixture"/> class.
    /// </summary>
    public UserRoleEntityFixture()
    {
        _fixture = new Fixture();
        ConfigureCustomTypes();
    }

    /// <summary>
    /// Creates a random valid <see cref="UserRoleEntity"/>.
    /// </summary>
    /// <returns>The created <see cref="UserRoleEntity"/>.</returns>
    public UserRoleEntity CreateUserRoleModel()
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
    /// Configures custom type generation rules for the AutoFixture instance.
    /// </summary>
    private void ConfigureCustomTypes()
    {
        _fixture.Register(() => new UserEntity
        {
            Id = _fixture.Create<Guid>(),
            Username = _fixture.Create<string>(),
            Password = _fixture.Create<string>(),
            Libraries = [],
            UserRole = null,
            UserPermissions = [],
            CreatedOnUtc = _fixture.Create<DateTime>(),
            CreatedBy = _fixture.Create<Guid>()
        });

        _fixture.Register(() => new RoleEntity
        {
            Id = _fixture.Create<Guid>(),
            RoleName = _fixture.Create<string>(),
            CreatedOnUtc = _fixture.Create<DateTime>(),
            CreatedBy = _fixture.Create<Guid>()
        });
    }
}
