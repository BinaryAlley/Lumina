#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.DTO.Authentication;

/// <summary>
/// Data transfer object for an authorization role.
/// </summary>
/// <param name="Id">The Id of the role.</param>
/// <param name="RoleName">The name of the role.</param>
[DebuggerDisplay("Id: {Id}, RoleName: {RoleName}")]
public record RoleDto(
    Guid Id,
    string RoleName
);
