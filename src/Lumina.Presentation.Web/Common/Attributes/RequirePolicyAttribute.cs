#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Filters.TypeFilters;
using Lumina.Presentation.Web.Common.Models.Authorization;
using System;
#endregion

namespace Lumina.Presentation.Web.Common.Attributes;

/// <summary>
/// Attribute that restricts endpoint access based on a custom authorization policy.
/// </summary>
/// <remarks>
/// Can be applied to controllers or individual actions.
/// </remarks>
public class RequirePolicyAttribute : ApiAuthorizationAttribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RequirePolicyAttribute"/> class.
    /// </summary>
    /// <param name="policyType">The type of the authorization policy to apply.</param>
    public RequirePolicyAttribute(Type policyType) : base()
    {
        Arguments = [new AuthorizationRequirementModel { PolicyType = policyType }];
    }
}
