#region ========================================================================= USING =====================================================================================
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Requests.Authorization;

/// <summary>
/// Represents a request for adding an authorization role.
/// </summary>
/// <param name="RoleName">The name of the role. Required.</param>
/// <param name="Permissions">The collection or permissions of the role. Required.</param>
[DebuggerDisplay("RoleName: {RoleName}")]
public record AddRoleRequest(
    string? RoleName,
    List<Guid>? Permissions
);
