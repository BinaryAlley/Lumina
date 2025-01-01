#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Core.Admin.Authorization.Roles.Commands.UpdateRole;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.UnitTests.Core.Admin.Authorization.Roles.Commands.UpdateRole.Fixtures;

/// <summary>
/// Fixture class for the <see cref="UpdateRoleCommand"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateRoleCommandFixture
{
    private readonly Faker _faker;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateRoleCommandFixture"/> class.
    /// </summary>
    public UpdateRoleCommandFixture()
    {
        _faker = new Faker();
    }

    /// <summary>
    /// Creates a random valid command to update a role.
    /// </summary>
    /// <param name="permissionCount">The number of permissions to generate.</param>
    /// <returns>The created command to update a role.</returns>
    public UpdateRoleCommand CreateCommand(int permissionCount = 3)
    {
        return new Faker<UpdateRoleCommand>()
            .CustomInstantiator(f => new UpdateRoleCommand(
                default,
                default!,
                default!))
            .RuleFor(x => x.RoleId, f => Guid.NewGuid())
            .RuleFor(x => x.RoleName, f => f.Random.String2(f.Random.Number(1, 50)))
            .RuleFor(x => x.Permissions, f => Enumerable.Range(0, permissionCount)
                .Select(_ => Guid.NewGuid())
                .ToList())
            .Generate();
    }
}
