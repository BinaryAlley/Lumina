#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Nemera.Presentation.Web.Common.Api;
using Nemera.Presentation.Web.Common.Authorization;
using Nemera.Presentation.Web.Common.Requests.Authorization;
using Nemera.Presentation.Web.Common.Routes;
using Nemera.Presentation.Web.Core.Endpoints.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Nemera.Presentation.Web.Core.Endpoints.Admin.Authorization.Roles.DeleteRole;

/// <summary>
/// API endpoint for the <c>/{culture}/admin/api-delete-role/{roleId}</c> route.
/// </summary>
public class DeleteRoleEndpoint : BaseEndpoint<DeleteRoleRequest, IResult>
{
    private readonly IApiHttpClient _apiHttpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteRoleEndpoint"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected service for interactions with the API.</param>
    public DeleteRoleEndpoint(IApiHttpClient apiHttpClient)
    {
        _apiHttpClient = apiHttpClient;
    }

    /// <summary>
    /// Configures the endpoint.
    /// </summary>
    public override void Configure()
    {
        base.Configure();
        Verbs(Http.DELETE);
        Routes(WebRoutes.Admin.DELETE_ROLE);
        DontAutoTag();
        Options(options => options.WithTags("Admin"));
        Policies(AuthorizationPolicies.REQUIRE_ADMIN_ROLE);
        EnableAntiforgery();
    }

    /// <summary>
    /// Deletes the authorization role identified by the request.
    /// </summary>
    /// <param name="request">The request containing the unique identifier of the authorization role to delete.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(DeleteRoleRequest request, CancellationToken cancellationToken)
    {
        await _apiHttpClient.DeleteAsync(ApiRoutes.Roles.DELETE_ROLE.Replace("{roleId}", request.RoleId.ToString()), cancellationToken).ConfigureAwait(false);
        return JsonSuccess();
    }
}
