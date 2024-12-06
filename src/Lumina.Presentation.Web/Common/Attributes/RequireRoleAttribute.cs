#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Enums.Authorization;
using Lumina.Presentation.Web.Common.Filters.TypeFilters;
using Lumina.Presentation.Web.Common.Models.Authorization;
#endregion

namespace Lumina.Presentation.Web.Common.Attributes;

/// <summary>
/// Attribute that restricts endpoint access to users with specified roles.
/// </summary>
/// <remarks>
/// Can be applied to controllers or individual actions. Multiple roles create an OR condition.
/// </remarks>
public class RequireRoleAttribute : ApiAuthorizationAttribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RequireRoleAttribute"/> class.
    /// </summary>
    /// <param name="roles">One or more roles that are allowed to access the endpoint.</param>
    public RequireRoleAttribute(params AuthorizationRole[] roles) : base()
    {
        Arguments = [new AuthorizationRequirementModel { Roles = roles }];
    }
}
