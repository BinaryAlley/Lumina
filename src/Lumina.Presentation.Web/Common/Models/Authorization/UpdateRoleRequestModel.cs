#region ========================================================================= USING =====================================================================================
using System;
using System.Collections.Generic;
#endregion

namespace Lumina.Presentation.Web.Common.Models.Authorization;

/// <summary>
/// Represents the request model for updating an existing authorization role.
/// </summary>
/// <param name="RoleId">The Id of the role.</param>
/// <param name="RoleName">The name of the role.</param>
/// <param name="Permissions">The collection or permissions of the role.</param>
public record UpdateRoleRequestModel(
    Guid RoleId,
    string? RoleName,
    List<Guid>? Permissions
);
