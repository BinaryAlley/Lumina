#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.CQRS;
using System;
using System.Diagnostics;
#endregion

namespace Nemera.Application.Core.Admin.Authorization.Roles.Commands.DeleteRole;

/// <summary>
/// Command for deleting an authorization role.
/// </summary>
/// <param name="RoleId">The unique identifier of the role.</param>
[DebuggerDisplay("RoleId: {RoleId}")]
public record DeleteRoleCommand(
    Guid RoleId
) : ICommand;
