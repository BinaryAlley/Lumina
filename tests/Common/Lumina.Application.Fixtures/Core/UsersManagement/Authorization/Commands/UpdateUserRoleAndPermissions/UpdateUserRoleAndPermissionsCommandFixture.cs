#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Core.UsersManagement.Authorization.Commands.UpdateUserRoleAndPermissions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.UsersManagement.Authorization.Commands.UpdateUserRoleAndPermissions;

/// <summary>
/// Fixture class for the <see cref="UpdateUserRoleAndPermissionsCommand"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateUserRoleAndPermissionsCommandFixture
{
    /// <summary>
    /// Creates a random valid command to update user role and permissions.
    /// </summary>
    /// <param name="userId">Optional. The user Id.</param>
    /// <param name="roleId">Optional. The role Id to assign. If null, no role will be assigned.</param>
    /// <param name="permissions">Optional. The permission Ids to assign.</param>
    /// <param name="permissionCount">The number of permissions to generate when none are provided.</param>
    /// <returns>The created command.</returns>
    public UpdateUserRoleAndPermissionsCommand Create(
        Guid? userId = null,
        Guid? roleId = null,
        IEnumerable<Guid>? permissions = null,
        int permissionCount = 3)
    {
        return new Faker<UpdateUserRoleAndPermissionsCommand>()
            .CustomInstantiator(f => new UpdateUserRoleAndPermissionsCommand(
                userId ?? Guid.NewGuid(),
                roleId,
                default!))
            .RuleFor(x => x.Permissions, f => [.. permissions ?? Enumerable.Range(0, permissionCount).Select(_ => Guid.NewGuid())])
            .Generate();
    }

    /// <summary>
    /// Creates a list of <see cref="UpdateUserRoleAndPermissionsCommand"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="UpdateUserRoleAndPermissionsCommand"/> instances.</returns>
    public List<UpdateUserRoleAndPermissionsCommand> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
