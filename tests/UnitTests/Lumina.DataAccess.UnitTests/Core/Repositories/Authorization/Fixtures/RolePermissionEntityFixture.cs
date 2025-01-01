#region ========================================================================= USING =====================================================================================
using AutoFixture;
using Bogus;
using Lumina.Application.Common.DataAccess.Entities.Authorization;
using Lumina.Domain.Common.Enums.Authorization;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.DataAccess.UnitTests.Core.Repositories.Authorization.Fixtures;

/// <summary>
/// Fixture class for the <see cref="RolePermissionEntity"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class RolePermissionEntityFixture
{
    private readonly Fixture _fixture;
    private readonly Faker _faker;

    /// <summary>
    /// Initializes a new instance of the <see cref="RolePermissionEntityFixture"/> class.
    /// </summary>
    public RolePermissionEntityFixture()
    {
        _fixture = new Fixture();
        _faker = new Faker();
        ConfigureCustomTypes();
    }

    /// <summary>
    /// Creates a random valid <see cref="RolePermissionEntity"/>.
    /// </summary>
    /// <returns>The created <see cref="RolePermissionEntity"/>.</returns>
    public RolePermissionEntity CreateRolePermissionModel()
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
    /// Configures custom type generation rules for the AutoFixture instance.
    /// </summary>
    private void ConfigureCustomTypes()
    {
        _fixture.Register(() => new PermissionEntity
        {
            Id = _fixture.Create<Guid>(),
            PermissionName = _fixture.Create<AuthorizationPermission>(),
            CreatedOnUtc = _fixture.Create<DateTime>(),
            CreatedBy = _fixture.Create<Guid>(),
            UpdatedOnUtc = _fixture.Create<DateTime>(),
            UpdatedBy = _fixture.Create<Guid>()
        });

        _fixture.Register(() => new RoleEntity
        {
            Id = _fixture.Create<Guid>(),
            RoleName = _fixture.Create<string>(),
            CreatedOnUtc = _fixture.Create<DateTime>(),
            CreatedBy = _fixture.Create<Guid>(),
            UpdatedOnUtc = _fixture.Create<DateTime>(),
            UpdatedBy = _fixture.Create<Guid>()
        });

        _fixture.Register(() => new RolePermissionEntity
        {
            Id = _fixture.Create<Guid>(),
            RoleId = _fixture.Create<Guid>(),
            Role = _fixture.Create<RoleEntity>(),
            PermissionId = _fixture.Create<Guid>(),
            Permission = _fixture.Create<PermissionEntity>(),
            CreatedOnUtc = _fixture.Create<DateTime>(),
            CreatedBy = _fixture.Create<Guid>(),
            UpdatedOnUtc = _fixture.Create<DateTime>(),
            UpdatedBy = _fixture.Create<Guid>()
        });
    }
}
