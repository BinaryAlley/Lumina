#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.Enums.Authorization;
using Lumina.Presentation.Web.Common.Exceptions;
using Lumina.Presentation.Web.Common.Http;
using Lumina.Presentation.Web.Common.Responses.Authorization;
using Microsoft.AspNetCore.Http;
using Polly.CircuitBreaker;
using System;
using System.Linq;
using System.Net.Http;
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
    /// Retrieves user authorization details from the authorization service.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The user's authorization details.</returns>
    private async Task<GetAuthorizationResponse> GetUserAuthorizationAsync(CancellationToken cancellationToken)
    {
        // anonymous users and users without an identifier claim have no role or permissions, so deny without querying the API
        string? userId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return new GetAuthorizationResponse(Guid.Empty, string.Empty, []);
        try
        {
            return await _apiHttpClient.GetAsync<GetAuthorizationResponse>($"auth/get-authorization?userId={Uri.EscapeDataString(userId)}", cancellationToken);
        }
        catch (Exception exception)
        {
            HttpContext? httpContext = _httpContextAccessor.HttpContext;
            if (httpContext is null || httpContext.RequestAborted.IsCancellationRequested)
                throw;
            if (!IsApiOutage(exception))
                throw;
            bool isPageNavigation = (httpContext.Request.Headers.Accept.Count == 0 && httpContext.Request.Headers.Range.Count == 0)
                || httpContext.Request.Headers.Accept.Any(accept => accept?.Contains("text/html", StringComparison.OrdinalIgnoreCase) == true)
                || string.Equals(httpContext.Request.Headers["Sec-Fetch-Mode"].ToString(), "navigate", StringComparison.OrdinalIgnoreCase)
                || string.Equals(httpContext.Request.Headers["Sec-Fetch-Dest"].ToString(), "document", StringComparison.OrdinalIgnoreCase);
            if (!isPageNavigation)
                throw;
            // The page is still rendered while the API is unreachable, with no role and no permissions, and the request is marked so
            // that the page reports the failure to the user. API requests keep propagating the failure as a JSON error.
            httpContext.Items[HttpContextItemKeys.SERVER_UNREACHABLE] = true;
            return new GetAuthorizationResponse(Guid.Empty, string.Empty, []);
        }
    }

    /// <summary>
    /// Determines whether the exception means that the API could not answer, as opposed to an authorization answer from the API.
    /// </summary>
    /// <param name="exception">The exception thrown by the API call.</param>
    /// <returns><see langword="true"/> for a connection, timeout, circuit breaker or server failure; otherwise <see langword="false"/>.</returns>
    private static bool IsApiOutage(Exception exception)
    {
        return exception is HttpRequestException or BrokenCircuitException or TaskCanceledException
            || (exception is ApiException apiException && (int)apiException.HttpStatusCode >= 500);
    }
}
