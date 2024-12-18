#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Enums.Authorization;
using System;
#endregion

namespace Lumina.Presentation.Web.Common.Models.Authorization;

/// <summary>
/// Model for storing the authorization requirements for accessing a resource.
/// </summary>
public class AuthorizationRequirementModel
{
    /// <summary>
    /// The collection of roles required for accessing the resource.
    /// </summary>
    public string[]? Roles { get; set; }

    /// <summary>
    /// The collection of permissions required for accessing the resource.
    /// </summary>
    public AuthorizationPermission[]? Permissions { get; set; }

    /// <summary>
    /// The type of the policy required for accessing the resource.
    /// </summary>
    public Type? PolicyType { get; set; }
}
