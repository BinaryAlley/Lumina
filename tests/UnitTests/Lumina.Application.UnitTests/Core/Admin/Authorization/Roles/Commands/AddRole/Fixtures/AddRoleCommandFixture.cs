#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Core.Admin.Authorization.Roles.Commands.AddRole;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.UnitTests.Core.Admin.Authorization.Roles.Commands.AddRole.Fixtures;

/// <summary>
/// Fixture class for the <see cref="AddRoleCommand"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddRoleCommandFixture
{
    /// <summary>
    /// Creates a random valid command to add a role.
    /// </summary>
    /// <param name="permissionCount">The number of permissions to generate.</param>
    /// <returns>The created command to add a role.</returns>
    public AddRoleCommand CreateCommand(int permissionCount = 3)
    {
        return new Faker<AddRoleCommand>()
            .CustomInstantiator(f => new AddRoleCommand(
                default!,
                default!))
            .RuleFor(x => x.RoleName, f => f.Random.String2(f.Random.Number(1, 50)))
            .RuleFor(x => x.Permissions, f => Enumerable.Range(0, permissionCount)
                .Select(_ => Guid.NewGuid())
                .ToList())
            .Generate();
    }
}
