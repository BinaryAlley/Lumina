#region ========================================================================= USING =====================================================================================
using System;
using System.Collections.Generic;
#endregion

namespace Lumina.Presentation.Web.Common.Models.Authorization;

/// <summary>
/// Represents the request model for adding an authorization role.
/// </summary>
/// <param name="RoleName">The name of the role.</param>
/// <param name="Permissions">The collection or permissions of the role.</param>
public record AddRoleRequestModel(
    string? RoleName,
    List<Guid>? Permissions
);
