#region ========================================================================= USING =====================================================================================
#endregion

namespace Lumina.Presentation.Web.Common.Models.Authorization;

/// <summary>
/// Represents a model for an authorization role with its permissions.
/// </summary>
/// <param name="Role">The authorization role.</param>
/// <param name="Permissions">The permissions of the authorization role.</param>
public record RolePermissionsModel(
    RoleModel Role,
    PermissionModel[] Permissions
);
