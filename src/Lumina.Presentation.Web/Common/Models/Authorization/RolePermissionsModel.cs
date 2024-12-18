#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Enums.Authorization;
using System;
#endregion

namespace Lumina.Presentation.Web.Common.Models.Authorization;

/// <summary>
/// Represents a model for an authorization role.
/// </summary>
/// <param name="Id">The Id of the authorization role.</param>
/// <param name="Role">The authorization role.</param>
public record RolePermissionsModel(
    RoleModel Role,
    PermissionModel[] Permissions
);
