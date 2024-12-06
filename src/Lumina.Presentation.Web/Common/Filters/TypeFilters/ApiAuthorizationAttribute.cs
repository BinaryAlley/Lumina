#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Filters.AuthorizationFilters;
using Microsoft.AspNetCore.Mvc;
using System;
#endregion

namespace Lumina.Presentation.Web.Common.Filters.TypeFilters;

/// <summary>
/// Authorization filter for API endpoints.
/// </summary>
/// <remarks>
/// This attribute can be applied to both classes and methods and supports multiple instances on the same target.
/// </remarks>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class ApiAuthorizationAttribute : TypeFilterAttribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ApiAuthorizationAttribute"/> class.
    /// </summary>
    public ApiAuthorizationAttribute() : base(typeof(ApiAuthorizationFilter))
    {
    }
}
