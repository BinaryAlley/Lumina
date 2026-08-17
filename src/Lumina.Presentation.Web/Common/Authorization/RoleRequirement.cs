#region ========================================================================= USING =====================================================================================
using Microsoft.AspNetCore.Authorization;
#endregion

namespace Lumina.Presentation.Web.Common.Authorization;

/// <summary>
/// Defines a requirement that the user must belong to at least one of the specified roles.
/// </summary>
public class RoleRequirement : IAuthorizationRequirement
{
    /// <summary>
    /// Gets the roles that allow access, of which the user must belong to at least one.
    /// </summary>
    public string[] Roles { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RoleRequirement"/> class.
    /// </summary>
    /// <param name="roles">The roles that allow access.</param>
    public RoleRequirement(params string[] roles)
    {
        Roles = roles;
    }
}
