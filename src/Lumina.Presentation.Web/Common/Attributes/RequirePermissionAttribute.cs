#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Enums.Authorization;
using Lumina.Presentation.Web.Common.Filters.TypeFilters;
using Lumina.Presentation.Web.Common.Models.Authorization;
#endregion

namespace Lumina.Presentation.Web.Common.Attributes;

/// <summary>
/// Attribute that restricts endpoint access to users with specified permissions.
/// </summary>
/// <remarks>
/// Can be applied to controllers or individual actions. Multiple permissions create an OR condition.
/// </remarks>
public class RequirePermissionAttribute : ApiAuthorizationAttribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RequirePermissionAttribute"/> class.
    /// </summary>
    /// <param name="permissions">One or more permissions that are allowed to access the endpoint.</param>
    public RequirePermissionAttribute(params AuthorizationPermission[] permissions) : base()
    {
        Arguments = [new AuthorizationRequirementModel { Permissions = permissions }];
    }
}
