#region ========================================================================= USING =====================================================================================
using Nemera.Domain.Common.Primitives;
using Nemera.Application.Common.CQRS;
using Nemera.Application.Common.Mapping.Authorization;
using Nemera.Application.Core.Admin.Authorization.Roles.Queries.GetRolePermissions;
using Nemera.Contracts.Requests.Authorization;
using Nemera.Contracts.Responses.Authorization;
using Nemera.Presentation.Api.Common.Routes.UsersManagement;
using Nemera.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Nemera.Presentation.Api.Core.Endpoints.Admin.Authorization.Roles.GetRolePermissions;

/// <summary>
/// API endpoint for the <c>/auth/roles/{roleId}/permissions</c> route.
/// </summary>
public class GetRolePermissionsEndpoint : BaseEndpoint<GetRolePermissionsRequest, IResult>
{
    private readonly IQueryHandler<GetRolePermissionsQuery, Result<RolePermissionsResponse>> _addRoleQueryHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetRolePermissionsEndpoint"/> class.
    /// </summary>
    /// <param name="addRoleQueryHandler">Injected service for handling add role commands.</param>
    public GetRolePermissionsEndpoint(IQueryHandler<GetRolePermissionsQuery, Result<RolePermissionsResponse>> addRoleQueryHandler)
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
        Result<RolePermissionsResponse> result = await _addRoleQueryHandler.HandleAsync(request.ToQuery(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
