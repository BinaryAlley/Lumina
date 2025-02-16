#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Contracts.Responses.Authorization;
using Mediator;
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.Admin.Authorization.Roles.Commands.UpdateRole;

/// <summary>
/// Command for updating an authorization role.
/// </summary>
/// <param name="RoleId">The unique identifier of the role.</param>
/// <param name="RoleName">The name of the role.</param>
/// <param name="Permissions">The collection of Ids of the permissions of the role.</param>
[DebuggerDisplay("RoleName: {RoleName}")]
public record UpdateRoleCommand(
    Guid RoleId,
    string RoleName,
    List<Guid> Permissions
) : IRequest<ErrorOr<RolePermissionsResponse>>;
