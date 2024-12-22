#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Models.UsersManagement;
#endregion

namespace Lumina.Presentation.Web.Common.Models.Authorization;

/// <summary>
/// Represents a model for a user with its authorization permissions.
/// </summary>
/// <param name="User">The user.</param>
/// <param name="Permissions">The authorization permissions of the user.</param>
public record UserPermissionsModel(
    UserModel User,
    PermissionModel[] Permissions
);
