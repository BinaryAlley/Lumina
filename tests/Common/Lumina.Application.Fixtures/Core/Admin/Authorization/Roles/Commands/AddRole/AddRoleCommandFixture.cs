#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Core.Admin.Authorization.Roles.Commands.AddRole;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.Admin.Authorization.Roles.Commands.AddRole;

/// <summary>
/// Fixture class for the <see cref="AddRoleCommand"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddRoleCommandFixture
{
    /// <summary>
    /// Creates a random valid command to add a role.
    /// </summary>
    /// <param name="roleName">Optional. The role name.</param>
    /// <param name="permissions">Optional. The permission Ids to assign.</param>
    /// <param name="permissionCount">The number of permissions to generate when none are provided.</param>
    /// <returns>The created command to add a role.</returns>
    public AddRoleCommand Create(
        string? roleName = null,
        IEnumerable<Guid>? permissions = null,
        int permissionCount = 3)
    {
        Faker<AddRoleCommand> faker = new Faker<AddRoleCommand>()
            .CustomInstantiator(f => new AddRoleCommand(
                default!,
                default!))
            .RuleFor(x => x.RoleName, f => roleName ?? f.Random.String2(f.Random.Number(1, 50)))
            .RuleFor(x => x.Permissions, f => [.. permissions ?? Enumerable.Range(0, permissionCount).Select(_ => Guid.NewGuid())]);
        return faker.Generate();
    }

    /// <summary>
    /// Creates a list of <see cref="AddRoleCommand"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<AddRoleCommand> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
