#region ========================================================================= USING =====================================================================================
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Requests.Authorization;

/// <summary>
/// Represents a request for updating the authorization role and permissions of a user.
/// </summary>
/// <param name="UserId">The Id of the user. Required.</param>
/// <param name="RoleId">The Id of the role. Optional.</param>
/// <param name="Permissions">The collection of Ids of the permissions of the role. Optional.</param>
[DebuggerDisplay("UserId: {UserId}")]
public record UpdateUserRoleAndPermissionsRequest(
    Guid? UserId,
    Guid? RoleId,
    List<Guid>? Permissions
);
