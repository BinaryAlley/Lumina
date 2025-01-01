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
/// Fixture class for the <see cref="UserPermissionEntity"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UserPermissionEntityFixture
{
    private readonly Fixture _fixture;
    private readonly Faker _faker;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserPermissionEntityFixture"/> class.
    /// </summary>
    public UserPermissionEntityFixture()
    {
        _fixture = new Fixture();
        _faker = new Faker();
        ConfigureCustomTypes();
    }

    /// <summary>
    /// Creates a random valid <see cref="UserPermissionEntity"/>.
    /// </summary>
    /// <returns>The created <see cref="UserPermissionEntity"/>.</returns>
    public UserPermissionEntity CreateUserPermissionModel(UserEntity user, PermissionEntity permission)
    {
        return new Faker<UserPermissionEntity>()
            .CustomInstantiator(f => new UserPermissionEntity
            {
                Id = f.Random.Guid(),
                UserId = user.Id,
                User = user,
                PermissionId = permission.Id,
                Permission = permission,
                CreatedOnUtc = f.Date.Past(),
                CreatedBy = user.Id,
                UpdatedOnUtc = f.Random.Bool() ? f.Date.Recent() : null,
                UpdatedBy = f.Random.Bool() ? user.Id : null
            })
            .Generate();
    }

    /// <summary>
    /// Configures custom type generation rules for the AutoFixture instance.
    /// </summary>
    private void ConfigureCustomTypes()
    {
        _fixture.Register(() => new UserPermissionEntity
        {
            Id = _fixture.Create<Guid>(),
            UserId = _fixture.Create<Guid>(),
            User = _fixture.Create<UserEntity>(),
            PermissionId = _fixture.Create<Guid>(),
            Permission = _fixture.Create<PermissionEntity>(),
            CreatedOnUtc = _fixture.Create<DateTime>(),
            CreatedBy = _fixture.Create<Guid>(),
            UpdatedOnUtc = _fixture.Create<DateTime>(),
            UpdatedBy = _fixture.Create<Guid>()
        });
    }
}
