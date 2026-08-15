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
/// Fixture class for the <see cref="PermissionEntity"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class PermissionEntityFixture
{
    /// <summary>
    /// Creates a random valid <see cref="PermissionEntity"/>.
    /// </summary>
    /// <param name="id">Optional. The permission Id.</param>
    /// <param name="permissionName">Optional. The permission name.</param>
    /// <returns>The created <see cref="PermissionEntity"/>.</returns>
    public PermissionEntity Create(Guid? id = null, AuthorizationPermission? permissionName = null)
    {
        return new Faker<PermissionEntity>()
            .RuleFor(x => x.Id, f => id ?? f.Random.Guid())
            .RuleFor(x => x.PermissionName, f => permissionName ?? f.PickRandom<AuthorizationPermission>())
            .RuleFor(x => x.RolePermissions, [])
            .RuleFor(x => x.UserPermissions, [])
            .RuleFor(x => x.CreatedOnUtc, f => f.Date.Past())
            .RuleFor(x => x.CreatedBy, f => f.Random.Guid())
            .RuleFor(x => x.UpdatedOnUtc, f => f.Date.Recent())
            .RuleFor(x => x.UpdatedBy, f => f.Random.Guid())
            .Generate();
    }

    /// <summary>
    /// Creates a list of <see cref="PermissionEntity"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="PermissionEntity"/> instances.</returns>
    public List<PermissionEntity> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
