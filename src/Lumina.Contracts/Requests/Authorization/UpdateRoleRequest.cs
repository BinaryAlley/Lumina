#region ========================================================================= USING =====================================================================================
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Requests.Authorization;

/// <summary>
/// Represents a request for updating an existing authorization role.
/// </summary>
/// <param name="RoleId">The unique identifier of the role. Required.</param>
/// <param name="RoleName">The name of the role. Required.</param>
/// <param name="Permissions">The collection of Ids of the permissions of the role. Required.</param>
[DebuggerDisplay("RoleName: {RoleName}")]
public record UpdateRoleRequest(
    Guid? RoleId,
    string? RoleName,
    List<Guid>? Permissions
);
