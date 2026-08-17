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

namespace Lumina.Presentation.Web.Core.Endpoints.Admin.Authorization.Roles.UpdateRole;

/// <summary>
/// API endpoint for the <c>/{culture}/admin/api-update-role</c> route.
/// </summary>
public class UpdateRoleEndpoint : BaseEndpoint<UpdateRoleRequest, IResult>
{
    private readonly IApiHttpClient _apiHttpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateRoleEndpoint"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected service for interactions with the API.</param>
    public UpdateRoleEndpoint(IApiHttpClient apiHttpClient)
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
        Routes(WebRoutes.Admin.UPDATE_ROLE);
        DontAutoTag();
        Options(options => options.WithTags("Admin"));
        Policies(AuthorizationPolicies.REQUIRE_ADMIN_ROLE);
        EnableAntiforgery();
    }

    /// <summary>
    /// Updates an existing authorization role.
    /// </summary>
    /// <param name="request">The request containing the updated details of the authorization role.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(UpdateRoleRequest request, CancellationToken cancellationToken)
    {
        RolePermissionsDto response = await _apiHttpClient.PutAsync<RolePermissionsDto, UpdateRoleRequest>(ApiRoutes.Roles.UPDATE_ROLE, request, cancellationToken).ConfigureAwait(false);
        return JsonSuccess(response);
    }
}
