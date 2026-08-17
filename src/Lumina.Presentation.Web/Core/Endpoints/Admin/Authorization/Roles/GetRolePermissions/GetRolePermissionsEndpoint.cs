#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.Authorization;
using Lumina.Presentation.Web.Common.DTO.Authorization;
using Lumina.Presentation.Web.Common.Requests.Authorization;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Admin.Authorization.Roles.GetRolePermissions;

/// <summary>
/// API endpoint for the <c>/{culture}/admin/api-get-permissions-by-role-id/{roleId}</c> route.
/// </summary>
public class GetRolePermissionsEndpoint : BaseEndpoint<GetRolePermissionsRequest, IResult>
{
    private readonly IApiHttpClient _apiHttpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetRolePermissionsEndpoint"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected service for interactions with the API.</param>
    public GetRolePermissionsEndpoint(IApiHttpClient apiHttpClient)
    {
        _apiHttpClient = apiHttpClient;
    }

    /// <summary>
    /// Configures the endpoint.
    /// </summary>
    public override void Configure()
    {
        base.Configure();
        Verbs(Http.GET);
        Routes(WebRoutes.Admin.GET_PERMISSIONS_BY_ROLE_ID);
        DontAutoTag();
        Options(options => options.WithTags("Admin"));
        Policies(AuthorizationPolicies.REQUIRE_ADMIN_ROLE);
    }

    /// <summary>
    /// Retrieves the permissions of the authorization role identified by the request.
    /// </summary>
    /// <param name="request">The request containing the unique identifier of the role whose permissions are retrieved.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(GetRolePermissionsRequest request, CancellationToken cancellationToken)
    {
        RolePermissionsDto response = await _apiHttpClient.GetAsync<RolePermissionsDto>(ApiRoutes.Roles.GET_ROLE_PERMISSIONS_BY_ROLE_ID.Replace("{roleId}", request.RoleId.ToString()), cancellationToken).ConfigureAwait(false);
        return JsonSuccess(response);
    }
}
