#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Core.UsersManagement.Authorization.Commands.UpdateUserRoleAndPermissions;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.UnitTests.Core.UsersManagement.Authorization.Commands.UpdateUserRoleAndPermissions.Fixtures;

/// <summary>
/// Fixture class for the <see cref="UpdateUserRoleAndPermissionsCommand"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateUserRoleAndPermissionsCommandFixture
{
    /// <summary>
    /// Creates a random valid command to update user role and permissions.
    /// </summary>
    /// <param name="roleId">Optional role ID to assign. If null, no role will be assigned.</param>
    /// <param name="permissionCount">The number of permissions to generate.</param>
    /// <returns>The created command.</returns>
    public UpdateUserRoleAndPermissionsCommand CreateCommand(Guid? roleId = null, int permissionCount = 3)
    {
        return new Faker<UpdateUserRoleAndPermissionsCommand>()
            .CustomInstantiator(f => new UpdateUserRoleAndPermissionsCommand(
                Guid.NewGuid(),
                roleId,
                default!))
            .RuleFor(x => x.Permissions, f => Enumerable.Range(0, permissionCount)
                .Select(_ => Guid.NewGuid())
                .ToList())
            .Generate();
    }
}

