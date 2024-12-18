#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Mediator;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.Admin.Authorization.Roles.Commands.DeleteRole;

/// <summary>
/// Command for deleting an authorization role.
/// </summary>
/// <param name="RoleId">The Id of the role.</param>
[DebuggerDisplay("RoleId: {RoleId}")]
public record DeleteRoleCommand(
    Guid RoleId
) : IRequest<ErrorOr<Deleted>>;
