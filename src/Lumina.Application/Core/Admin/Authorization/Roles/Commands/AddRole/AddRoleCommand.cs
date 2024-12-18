#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Contracts.Responses.Authorization;
using Mediator;
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.Admin.Authorization.Roles.Commands.AddRole;

/// <summary>
/// Command for adding an authorization role.
/// </summary>
/// <param name="RoleName">The name of the role.</param>
/// <param name="Permissions">The collection or permissions of the role.</param>
[DebuggerDisplay("RoleName: {RoleName}")]
public record AddRoleCommand(
    string RoleName,
    List<Guid> Permissions
) : IRequest<ErrorOr<RolePermissionsResponse>>;
