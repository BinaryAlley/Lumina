#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.DTO.Authorization;

/// <summary>
/// Data transfer object for a model for an authorization role with its permissions.
/// </summary>
/// <param name="Role">The authorization role.</param>
/// <param name="Permissions">The permissions of the authorization role.</param>
[DebuggerDisplay("Role: {Role}")]
public record RolePermissionsDto(
    RoleDto Role,
    PermissionDto[] Permissions
);
