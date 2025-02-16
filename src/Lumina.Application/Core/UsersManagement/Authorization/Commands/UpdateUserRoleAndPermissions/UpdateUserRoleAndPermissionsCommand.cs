#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Contracts.Responses.Authorization;
using Mediator;
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.UsersManagement.Authorization.Commands.UpdateUserRoleAndPermissions;

/// <summary>
/// Command for updating the authorization role and permissions of a user.
/// </summary>
/// <param name="UserId">The unique identifier of the user.</param>
/// <param name="RoleId">The unique identifier of the role.</param>
/// <param name="Permissions">The collection of Ids of the permissions of the user.</param>
[DebuggerDisplay("UserId: {UserId}")]
public record UpdateUserRoleAndPermissionsCommand(
    Guid UserId,
    Guid? RoleId,
    List<Guid> Permissions
) : IRequest<ErrorOr<AuthorizationResponse>>;
