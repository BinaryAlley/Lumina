#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Nemera.Presentation.Web.Common.Api;
using Nemera.Presentation.Web.Common.Authorization;
using Nemera.Presentation.Web.Common.Requests.Authorization;
using Nemera.Presentation.Web.Common.Responses.Authorization;
using Nemera.Presentation.Web.Common.Routes;
using Nemera.Presentation.Web.Core.Endpoints.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Nemera.Presentation.Web.Core.Endpoints.Admin.UsersManagement.Authorization.UpdateUserRoleAndPermissions;

/// <summary>
/// API endpoint for the <c>/{culture}/admin/api-update-user-authorization</c> route.
/// </summary>
public class UpdateUserRoleAndPermissionsEndpoint : BaseEndpoint<UpdateUserRoleAndPermissionsRequest, IResult>
{
    private readonly IApiHttpClient _apiHttpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateUserRoleAndPermissionsEndpoint"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected service for interactions with the API.</param>
    public UpdateUserRoleAndPermissionsEndpoint(IApiHttpClient apiHttpClient)
    {
        _apiHttpClient = apiHttpClient;
    }

    /// <summary>
    /// Configures the endpoint.
    /// </summary>
    public override void Configure()
    {
        base.Configure();
        Verbs(Http.PUT);
        Routes(WebRoutes.Admin.UPDATE_USER_AUTHORIZATION);
        DontAutoTag();
        Options(options => options.WithTags("Admin"));
        Policies(AuthorizationPolicies.REQUIRE_ADMIN_ROLE);
        EnableAntiforgery();
    }

    /// <summary>
    /// Updates the authorization role and permissions of the user identified by the request.
    /// </summary>
    /// <param name="request">The request containing the updated authorization role and permissions of the user.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(UpdateUserRoleAndPermissionsRequest request, CancellationToken cancellationToken)
    {
        GetAuthorizationResponse response = await _apiHttpClient.PutAsync<GetAuthorizationResponse, UpdateUserRoleAndPermissionsRequest>(ApiRoutes.Authorization.UPDATE_USER_ROLE_AND_PERMISSIONS_BY_USER_ID.Replace("{userId}", request.UserId.ToString()), request, cancellationToken).ConfigureAwait(false);
        return JsonSuccess(response);
    }
}
