#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Core.Admin.Authorization.Roles.Commands.UpdateRole;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.Admin.Authorization.Roles.Commands.UpdateRole;

/// <summary>
/// Fixture class for the <see cref="UpdateRoleCommand"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateRoleCommandFixture
{
    /// <summary>
    /// Creates a random valid command to update a role.
    /// </summary>
    /// <param name="roleId">Optional. The role Id.</param>
    /// <param name="roleName">Optional. The role name.</param>
    /// <param name="permissions">Optional. The permission Ids to assign.</param>
    /// <param name="permissionCount">The number of permissions to generate when none are provided.</param>
    /// <returns>The created command to update a role.</returns>
    public UpdateRoleCommand Create(
        Guid? roleId = null,
        string? roleName = null,
        IEnumerable<Guid>? permissions = null,
        int permissionCount = 3)
    {
        return new Faker<UpdateRoleCommand>()
            .CustomInstantiator(f => new UpdateRoleCommand(
                default,
                default!,
                default!))
            .RuleFor(x => x.RoleId, f => roleId ?? Guid.NewGuid())
            .RuleFor(x => x.RoleName, f => roleName ?? f.Random.String2(f.Random.Number(1, 50)))
            .RuleFor(x => x.Permissions, f => [.. permissions ?? Enumerable.Range(0, permissionCount).Select(_ => Guid.NewGuid())])
            .Generate();
    }

    /// <summary>
    /// Creates a list of <see cref="UpdateRoleCommand"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<UpdateRoleCommand> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
