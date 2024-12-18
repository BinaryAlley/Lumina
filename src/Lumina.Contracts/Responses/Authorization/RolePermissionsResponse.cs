#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.Authentication;
#endregion

namespace Lumina.Contracts.Responses.Authorization;

/// <summary>
/// Represents an authorizaton role permissions response.
/// </summary>
/// <param name="Role">The authorization role.</param>
/// <param name="Permissions">The permissions of the authorization role.</param>
public record RolePermissionsResponse(
    RoleDto Role,
    PermissionDto[] Permissions
);
