#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Enums.Authorization;
using System;
#endregion

namespace Lumina.Presentation.Web.Common.Models.Authorization;

/// <summary>
/// Represents a model for an authorization permission.
/// </summary>
/// <param name="Id">The unique identifier of the authorization permission.</param>
/// <param name="PermissionName">The authorization permission.</param>
public record PermissionModel(
    Guid Id,
    AuthorizationPermission PermissionName
);
