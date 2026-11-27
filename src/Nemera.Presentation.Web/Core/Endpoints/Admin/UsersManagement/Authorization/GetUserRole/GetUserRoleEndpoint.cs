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

namespace Lumina.Presentation.Web.Core.Endpoints.Admin.UsersManagement.Authorization.GetUserRole;

/// <summary>
/// API endpoint for the <c>/{culture}/admin/api-get-role-by-user-id/{userId}</c> route.
/// </summary>
public class GetUserRoleEndpoint : BaseEndpoint<GetUserRoleRequest, IResult>
{
    private readonly IApiHttpClient _apiHttpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetUserRoleEndpoint"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected service for interactions with the API.</param>
    public GetUserRoleEndpoint(IApiHttpClient apiHttpClient)
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
        Routes(WebRoutes.Admin.GET_ROLE_BY_USER_ID);
        DontAutoTag();
        Options(options => options.WithTags("Admin"));
        Policies(AuthorizationPolicies.REQUIRE_ADMIN_ROLE);
    }

    /// <summary>
    /// Retrieves the authorization role of the user identified by the request.
    /// </summary>
    /// <param name="request">The request containing the unique identifier of the user whose authorization role is retrieved.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(GetUserRoleRequest request, CancellationToken cancellationToken)
    {
        RoleDto? response = await _apiHttpClient.GetAsync<RoleDto?>(ApiRoutes.Authorization.GET_USER_ROLE_BY_USER_ID.Replace("{userId}", request.UserId.ToString()), cancellationToken).ConfigureAwait(false);
        return JsonSuccess(response);
    }
}
