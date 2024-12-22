#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.Enums.Authorization;
using Lumina.Presentation.Web.Common.Models.Authorization;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Common.Authorization;

/// <summary>
/// Implements authorization logic for API endpoints by validating user roles, permissions, and policies.
/// </summary>
public class AuthorizationService : IAuthorizationService
{
    private readonly IApiHttpClient _apiHttpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthorizationService"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected HTTP typed client for interactions with the API.</param>
    /// <param name="httpContextAccessor">The <see cref="IHttpContextAccessor"/> used to access the current HTTP context.</param>
    public AuthorizationService(IApiHttpClient apiHttpClient, IHttpContextAccessor httpContextAccessor)
    {
        _apiHttpClient = apiHttpClient;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Determines whether the currently logged in user has a specific permission.
    /// </summary>
    /// <param name="permission">The permission to check.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns><see langword="true"/> if the currently logged in user has the specified permission, <see langword="false"/> otherwise.</returns>
    public async Task<bool> HasPermissionAsync(AuthorizationPermission permission, CancellationToken cancellationToken)
    {
        GetAuthorizationResponse authorization = await GetUserAuthorizationAsync(cancellationToken).ConfigureAwait(false);
        return authorization.Permissions.Contains(permission);
    }

    /// <summary>
    /// Determines whether the currently logged in user belongs to a specific role.
    /// </summary>
    /// <param name="role">The role to check.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns><see langword="true"/> if the currently logged in user is in the specified role, <see langword="false"/> otherwise.</returns>
    public async Task<bool> IsInRoleAsync(string role, CancellationToken cancellationToken)
    {
        GetAuthorizationResponse authorization = await GetUserAuthorizationAsync(cancellationToken).ConfigureAwait(false);
        return authorization.Role == role;
    }

    /// <summary>
    /// Evaluates whether the currently logged in user meets the conditions defined in the specified authorization policy.
    /// </summary>
    /// <typeparam name="TAuthorizationPolicy">The type of authorization policy to evaluate.</typeparam>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns><see langword="true"/> if the currently logged in user satisfies the policy, <see langword="false"/> otherwise.</returns>
    public Task<bool> EvaluatePolicyAsync<TAuthorizationPolicy>(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Evaluates whether the currently logged in user meets the conditions defined in the specified authorization requirement.
    /// </summary>
    /// <param name="requirement">The authorization requirements to check.</param>
    /// <returns><see langword="true"/> if the currently logged in user satisfies the requirement, <see langword="false"/> otherwise.</returns>
    public async Task<bool> EvaluateAuthorizationAsync(AuthorizationRequirementModel requirement)
    {
        GetAuthorizationResponse authorization = await GetUserAuthorizationAsync(CancellationToken.None);

        if (requirement.Roles?.Length > 0 && !requirement.Roles.Any(role => authorization.Role?.Contains(role) == true))
            return false;

        if (requirement.Permissions?.Length > 0 && !requirement.Permissions.Any(permission => authorization.Permissions.Contains(permission)))
            return false;

        //if (_requirement.PolicyType is not null)
        //{
        //    bool response = await _apiHttpClient.PostAsync<bool, object>($"auth/evaluate-policy/{_requirement.PolicyType.Name}", null!);
        //    if (!response)
        //        return false;
        //}

        return true;
    }

    /// <summary>
    /// Retrieves user authorization details from the authorization service.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The user's authorization details.</returns>
    private async Task<GetAuthorizationResponse> GetUserAuthorizationAsync(CancellationToken cancellationToken)
    {
        // get the id of the currently logged in user
        string userId = _httpContextAccessor.HttpContext!.User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        return await _apiHttpClient.GetAsync<GetAuthorizationResponse>($"auth/get-authorization?userId={Uri.EscapeDataString(userId.ToString())}", cancellationToken);
    }
}
