#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Core.Admin.Authorization.Roles.Commands.DeleteRole;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.Admin.Authorization.Roles.Commands.DeleteRole.Fixtures;

/// <summary>
/// Fixture class for the <see cref="DeleteRoleCommand"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class DeleteRoleCommandFixture
{
    /// <summary>
    /// Creates a random valid command to delete a role.
    /// </summary>
    /// <returns>The created command.</returns>
    public DeleteRoleCommand CreateCommand()
    {
        return new Faker<DeleteRoleCommand>()
            .CustomInstantiator(f => new DeleteRoleCommand(
                Guid.NewGuid()))
            .Generate();
    }
}
