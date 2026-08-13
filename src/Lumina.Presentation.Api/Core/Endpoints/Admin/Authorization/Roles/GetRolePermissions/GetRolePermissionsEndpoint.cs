#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.Mapping.Authorization;
using Lumina.Application.Core.Admin.Authorization.Roles.Queries.GetRolePermissions;
using Lumina.Contracts.Requests.Authorization;
using Lumina.Contracts.Responses.Authorization;
using Lumina.Presentation.Api.Common.Routes.UsersManagement;
using Lumina.Presentation.Api.Core.Endpoints.Common;
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
    private readonly IQueryHandler<GetRolePermissionsQuery, ErrorOr<RolePermissionsResponse>> _addRoleQueryHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetRolePermissionsEndpoint"/> class.
    /// </summary>
    /// <param name="addRoleQueryHandler">Injected service for handling add role commands.</param>
    public GetRolePermissionsEndpoint(IQueryHandler<GetRolePermissionsQuery, ErrorOr<RolePermissionsResponse>> addRoleQueryHandler)
    {
        _addRoleQueryHandler = addRoleQueryHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(FastEndpoints.Http.GET);
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
        ErrorOr<RolePermissionsResponse> result = await _addRoleQueryHandler.HandleAsync(request.ToQuery(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
