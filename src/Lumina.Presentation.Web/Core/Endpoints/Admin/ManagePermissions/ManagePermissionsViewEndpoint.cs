#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.Authorization;
using Lumina.Presentation.Web.Common.DTO.Authorization;
using Lumina.Presentation.Web.Common.DTO.UsersManagement;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Admin.ManagePermissions;

/// <summary>
/// API endpoint for the <c>/{culture}/admin/manage-permissions</c> route.
/// </summary>
public class ManagePermissionsViewEndpoint : BaseEndpoint<EmptyRequest, IResult>
{
    private readonly IApiHttpClient _apiHttpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="ManagePermissionsViewEndpoint"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected service for interactions with the API.</param>
    public ManagePermissionsViewEndpoint(IApiHttpClient apiHttpClient)
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
        Routes(WebRoutes.Admin.MANAGE_PERMISSIONS);
        DontAutoTag();
        Options(options => options.WithTags("Admin"));
        Policies(AuthorizationPolicies.REQUIRE_ADMIN_ROLE);
    }

    /// <summary>
    /// Displays the authorization permissions management view.
    /// </summary>
    /// <param name="request">The request object.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(EmptyRequest request, CancellationToken cancellationToken)
    {
        UserDto[] users = await _apiHttpClient.GetAsync<UserDto[]>(ApiRoutes.Authentication.USERS, cancellationToken).ConfigureAwait(false);
        RoleDto[] roles = await _apiHttpClient.GetAsync<RoleDto[]>(ApiRoutes.Roles.GET_ROLES, cancellationToken).ConfigureAwait(false);
        PermissionDto[] permissions = await _apiHttpClient.GetAsync<PermissionDto[]>(ApiRoutes.Permissions.GET_PERMISSIONS, cancellationToken).ConfigureAwait(false);
        Dictionary<string, object?> viewData = new() { ["users"] = users, ["roles"] = roles, ["permissions"] = permissions };
        return View("/Core/Views/Admin/ManagePermissions.cshtml", viewData: viewData);
    }
}
