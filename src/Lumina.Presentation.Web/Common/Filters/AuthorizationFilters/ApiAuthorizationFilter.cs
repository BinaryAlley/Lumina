#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.Models.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Common.Filters.AuthorizationFilters;

/// <summary>
/// Implements authorization filter logic for API endpoints by validating user roles, permissions, and policies.
/// </summary>
public class ApiAuthorizationFilter : IAsyncAuthorizationFilter
{
    private readonly IApiHttpClient _apiHttpClient;
    private readonly AuthorizationRequirementModel _requirement;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiAuthorizationFilter"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected service for interactions with the API.</param>
    /// <param name="requirement">Authorization requirements to evaluate.</param>
    public ApiAuthorizationFilter(IApiHttpClient apiHttpClient, AuthorizationRequirementModel requirement)
    {
        _apiHttpClient = apiHttpClient;
        _requirement = requirement;
    }

    /// <summary>
    /// Performs authorization for the current request.
    /// </summary>
    /// <param name="context">The authorization filter context.</param>
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        // get the id of the currently logged in user
        Claim? currentUserIdClaim = context.HttpContext.User?.FindFirst(ClaimTypes.NameIdentifier);
        // if a valid user id cannot be retrieved, authorization fails
        if (currentUserIdClaim is null || string.IsNullOrWhiteSpace(currentUserIdClaim.Value) || !Guid.TryParse(currentUserIdClaim.Value, out Guid currentUserId))
            context.Result = new ForbidResult();
        else
        {
            // ask the API for the user's authorization details
            GetAuthorizationResponse userAuthorization = await GetUserAuthorizationAsync(currentUserId, context.HttpContext.RequestAborted);
            // if the user's authorization details don't satisfy the requested authorization details, deny the request
            if (!await EvaluateAuthorizationAsync(userAuthorization))
                context.Result = new ForbidResult();
        }
    }

    /// <summary>
    /// Retrieves user authorization details from the authorization service.
    /// </summary>
    /// <param name="userId">The Id of the user to get authorization for.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The user's authorization details.</returns>
    private async Task<GetAuthorizationResponse> GetUserAuthorizationAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await _apiHttpClient.GetAsync<GetAuthorizationResponse>($"auth/get-authorization?userId={Uri.EscapeDataString(userId.ToString())}", cancellationToken);
    }

    /// <summary>
    /// Evaluates if the user meets all authorization requirements.
    /// </summary>
    /// <param name="authorization">The user's authorization details.</param>
    /// <returns><see langword="true"/> if the user meets all requirements, <see langword="false"/> otherwise.</returns>
    private async Task<bool> EvaluateAuthorizationAsync(GetAuthorizationResponse authorization)
    {
        if (_requirement.Roles?.Length > 0 && !_requirement.Roles.Any(role => authorization.Roles?.Contains(role) == true))
            return false;

        if (_requirement.Permissions?.Length > 0 && !_requirement.Permissions.Any(permission => authorization.Permissions.Contains(permission)))
            return false;

        //if (_requirement.PolicyType is not null)
        //{
        //    bool response = await _apiHttpClient.PostAsync<bool, object>($"auth/evaluate-policy/{_requirement.PolicyType.Name}", null!);
        //    if (!response)
        //        return false;
        //}
        await Task.CompletedTask;
        return true;
    }
}
