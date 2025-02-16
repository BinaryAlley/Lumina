#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Requests.Authorization;

/// <summary>
/// Represents a request for deleting an authorization role.
/// </summary>
/// <param name="RoleId">The unique identifier of the role. Required.</param>
[DebuggerDisplay("RoleId: {RoleId}")]
public record DeleteRoleRequest(
    Guid? RoleId
);
