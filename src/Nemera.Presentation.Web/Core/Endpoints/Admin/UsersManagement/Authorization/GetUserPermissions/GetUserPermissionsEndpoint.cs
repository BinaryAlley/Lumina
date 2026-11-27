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

namespace Lumina.Presentation.Web.Core.Endpoints.Admin.UsersManagement.Authorization.GetUserPermissions;

/// <summary>
/// API endpoint for the <c>/{culture}/admin/api-get-permissions-by-user-id/{userId}</c> route.
/// </summary>
public class GetUserPermissionsEndpoint : BaseEndpoint<GetUserPermissionsRequest, IResult>
{
    private readonly IApiHttpClient _apiHttpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetUserPermissionsEndpoint"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected service for interactions with the API.</param>
    public GetUserPermissionsEndpoint(IApiHttpClient apiHttpClient)
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
        Routes(WebRoutes.Admin.GET_PERMISSIONS_BY_USER_ID);
        DontAutoTag();
        Options(options => options.WithTags("Admin"));
        Policies(AuthorizationPolicies.REQUIRE_ADMIN_ROLE);
    }

    /// <summary>
    /// Retrieves the permissions of the user identified by the request.
    /// </summary>
    /// <param name="request">The request containing the unique identifier of the user whose permissions are retrieved.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(GetUserPermissionsRequest request, CancellationToken cancellationToken)
    {
        PermissionDto[] response = await _apiHttpClient.GetAsync<PermissionDto[]>(ApiRoutes.Authorization.GET_USER_PERMISSIONS_BY_USER_ID.Replace("{userId}", request.UserId.ToString()), cancellationToken).ConfigureAwait(false);
        return JsonSuccess(response);
    }
}
