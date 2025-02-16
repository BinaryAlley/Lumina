#region ========================================================================= USING =====================================================================================
using System;
#endregion

namespace Lumina.Presentation.Web.Common.Models.Authorization;

/// <summary>
/// Represents a model for an authorization role.
/// </summary>
/// <param name="Id">The unique identifier of the authorization role.</param>
/// <param name="RoleName">The authorization role.</param>
public record RoleModel(
    Guid Id,
    string RoleName
);
