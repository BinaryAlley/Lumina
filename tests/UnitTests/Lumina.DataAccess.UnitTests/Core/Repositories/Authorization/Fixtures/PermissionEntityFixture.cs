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
/// Fixture class for the <see cref="PermissionEntity"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class PermissionEntityFixture
{
    private readonly Fixture _fixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="PermissionEntityFixture"/> class.
    /// </summary>
    public PermissionEntityFixture()
    {
        _fixture = new Fixture();
        ConfigureCustomTypes();
    }

    /// <summary>
    /// Creates a random valid <see cref="PermissionEntity"/>.
    /// </summary>
    /// <returns>The created <see cref="PermissionEntity"/>.</returns>
    public PermissionEntity CreatePermissionModel()
    {
        return new Faker<PermissionEntity>()
            .RuleFor(x => x.Id, f => f.Random.Guid())
            .RuleFor(x => x.PermissionName, f => f.PickRandom<AuthorizationPermission>())
            .RuleFor(x => x.RolePermissions, [])
            .RuleFor(x => x.UserPermissions, [])
            .RuleFor(x => x.CreatedOnUtc, f => f.Date.Past())
            .RuleFor(x => x.CreatedBy, f => f.Random.Guid())
            .RuleFor(x => x.UpdatedOnUtc, f => f.Date.Recent())
            .RuleFor(x => x.UpdatedBy, f => f.Random.Guid())
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
    }
}
