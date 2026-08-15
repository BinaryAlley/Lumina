#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Core.Admin.Authorization.Roles.Commands.DeleteRole;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.Admin.Authorization.Roles.Commands.DeleteRole;

/// <summary>
/// Fixture class for the <see cref="DeleteRoleCommand"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class DeleteRoleCommandFixture
{
    /// <summary>
    /// Creates a random valid command to delete a role.
    /// </summary>
    /// <param name="roleId">Optional. The role Id.</param>
    /// <returns>The created command.</returns>
    public DeleteRoleCommand Create(Guid? roleId = null)
    {
        return new Faker<DeleteRoleCommand>()
            .CustomInstantiator(f => new DeleteRoleCommand(
                roleId ?? Guid.NewGuid()))
            .Generate();
    }

    /// <summary>
    /// Creates a list of <see cref="DeleteRoleCommand"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<DeleteRoleCommand> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
