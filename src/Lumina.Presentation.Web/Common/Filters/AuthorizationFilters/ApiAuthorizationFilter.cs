#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Authorization;
using Lumina.Presentation.Web.Common.Models.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Common.Filters.AuthorizationFilters;

/// <summary>
/// Implements authorization filter logic for API endpoints by validating user roles, permissions, and policies.
/// </summary>
public class ApiAuthorizationFilter : IAsyncAuthorizationFilter
{
    private readonly IAuthorizationService _authorizationService;
    private readonly AuthorizationRequirementModel _requirement;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiAuthorizationFilter"/> class.
    /// </summary>
    /// <param name="authorizationService">Injected service for interactions with the API.</param>
    /// <param name="requirement">Authorization requirements to evaluate.</param>
    public ApiAuthorizationFilter(IAuthorizationService authorizationService, AuthorizationRequirementModel requirement)
    {
        _authorizationService = authorizationService;
        _requirement = requirement;
    }

    /// <summary>
    /// Performs authorization for the current request.
    /// </summary>
    /// <param name="context">The authorization filter context.</param>
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        if (!await _authorizationService.EvaluateAuthorizationAsync(_requirement))
            context.Result = new ForbidResult();
    }
}
