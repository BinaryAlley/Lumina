#region ========================================================================= USING =====================================================================================
using ErrorOr;
using FastEndpoints;
using Lumina.Application.Common.Mapping.Authorization;
using Lumina.Contracts.Requests.Authorization;
using Lumina.Contracts.Responses.Authorization;
using Lumina.Presentation.Api.Common.Routes.UsersManagement;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Mediator;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Admin.Authorization.Roles.GetRolePermissions;

/// <summary>
/// API endpoint for the <c>/auth/roles/{roleId}/permissions</c> route.
/// </summary>
public class GetRolePermissionsEndpoint : BaseEndpoint<GetRolePermissionsRequest, IResult>
{
    private readonly ISender _sender;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetRolePermissionsEndpoint"/> class.
    /// </summary>
    /// <param name="sender">Injected service for mediating commands and queries.</param>
    public GetRolePermissionsEndpoint(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(Http.GET);
        Routes(ApiRoutes.Roles.GET_ROLE_PERMISSIONS_BY_ROLE_ID);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Gets the list of authorization permissions of a role identified by Id.
    /// </summary>
    /// <param name="request">The request containing the id of the role for which to get the list of permissions.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(GetRolePermissionsRequest request, CancellationToken cancellationToken)
    {
        ErrorOr<RolePermissionsResponse> result = await _sender.Send(request.ToQuery(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
