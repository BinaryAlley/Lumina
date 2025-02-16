#region ========================================================================= USING =====================================================================================
using System;
using System.Collections.Generic;
#endregion

namespace Lumina.Presentation.Web.Common.Models.Authorization;

/// <summary>
/// Represents the request model for updating the authorization role and permissions of a user.
/// </summary>
/// <param name="RoleId">The unique identifier of the user.</param>
/// <param name="RoleId">The unique identifier of the role.</param>
/// <param name="Permissions">The collection of Ids of the permissions of the user.</param>
public record UpdateUserRoleAndPermissionsRequestModel(
    Guid UserId,
    Guid? RoleId,
    List<Guid>? Permissions
);
