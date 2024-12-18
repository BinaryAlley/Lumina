#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Requests.Authorization;

/// <summary>
/// Represents a request to get the authorization permissions of a role identified by <paramref name="RoleId"/>.
/// </summary>
/// <param name="RoleId">The Id of authorization role. Required.</param>
[DebuggerDisplay("RoleId: {RoleId}")]
public record GetRolePermissionsRequest(
    Guid RoleId
);
