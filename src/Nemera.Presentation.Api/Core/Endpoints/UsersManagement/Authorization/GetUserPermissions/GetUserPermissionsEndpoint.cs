#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Nemera.Application.Common.CQRS;
using Nemera.Application.Common.Mapping.Authentication;
using Nemera.Application.Common.Mapping.Authorization;
using Nemera.Application.Core.UsersManagement.Authorization.Queries.GetUserPermissions;
using Nemera.Contracts.Requests.Authorization;
using Nemera.Contracts.Responses.Authorization;
using Nemera.Domain.Common.Primitives;
using Nemera.Presentation.Api.Common.Routes.UsersManagement;
using Nemera.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Nemera.Presentation.Api.Core.Endpoints.UsersManagement.Authorization.GetUserPermissions;

/// <summary>
/// API endpoint for the <c>/auth/users/{userId}/permissions</c> route.
/// </summary>
public class GetUserPermissionsEndpoint : BaseEndpoint<GetUserPermissionsRequest, IResult>
{
    private readonly IQueryHandler<GetUserPermissionsQuery, Result<IEnumerable<PermissionResponse>>> _getUserPermissionsQueryHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetUserPermissionsEndpoint"/> class.
    /// </summary>
    /// <param name="getUserPermissionsQueryHandler">Injected service for handling get user permissions queries.</param>
    public GetUserPermissionsEndpoint(IQueryHandler<GetUserPermissionsQuery, Result<IEnumerable<PermissionResponse>>> getUserPermissionsQueryHandler)
    {
        _getUserPermissionsQueryHandler = getUserPermissionsQueryHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(Http.GET);
        Routes(ApiRoutes.Authorization.GET_USER_PERMISSIONS_BY_USER_ID);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Gets the authorization permissions of a user.
    /// </summary>
    /// <param name="request">The request containing the user for whom to get the authorization permissions.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(GetUserPermissionsRequest request, CancellationToken cancellationToken)
    {
        Result<IEnumerable<PermissionResponse>> result = await _getUserPermissionsQueryHandler.HandleAsync(request.ToQuery(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
