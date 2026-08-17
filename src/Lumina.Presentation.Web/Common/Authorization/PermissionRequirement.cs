#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Enums.Authorization;
using Microsoft.AspNetCore.Authorization;
#endregion

namespace Lumina.Presentation.Web.Common.Authorization;

/// <summary>
/// Defines a requirement that the user must hold at least one of the specified permissions.
/// </summary>
public class PermissionRequirement : IAuthorizationRequirement
{
    /// <summary>
    /// Gets the permissions that allow access, of which the user must hold at least one.
    /// </summary>
    public AuthorizationPermission[] Permissions { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PermissionRequirement"/> class.
    /// </summary>
    /// <param name="permissions">The permissions that allow access.</param>
    public PermissionRequirement(params AuthorizationPermission[] permissions)
    {
        Permissions = permissions;
    }
}
