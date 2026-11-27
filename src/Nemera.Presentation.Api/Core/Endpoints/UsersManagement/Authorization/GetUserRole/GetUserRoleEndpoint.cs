#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Nemera.Application.Common.CQRS;
using Nemera.Application.Common.Mapping.Authentication;
using Nemera.Application.Common.Mapping.Authorization;
using Nemera.Application.Core.UsersManagement.Authorization.Queries.GetUserRole;
using Nemera.Contracts.Requests.Authorization;
using Nemera.Contracts.Responses.Authorization;
using Nemera.Domain.Common.Primitives;
using Nemera.Presentation.Api.Common.Routes.UsersManagement;
using Nemera.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Nemera.Presentation.Api.Core.Endpoints.UsersManagement.Authorization.GetUserRole;

/// <summary>
/// API endpoint for the <c>/auth/users/{userId}/role</c> route.
/// </summary>
public class GetUserRoleEndpoint : BaseEndpoint<GetUserRoleRequest, IResult>
{
    private readonly IQueryHandler<GetUserRoleQuery, Result<RoleResponse?>> _getUserRoleQueryHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetUserRoleEndpoint"/> class.
    /// </summary>
    /// <param name="getUserRoleQueryHandler">Injected service for handling get user role queries.</param>
    public GetUserRoleEndpoint(IQueryHandler<GetUserRoleQuery, Result<RoleResponse?>> getUserRoleQueryHandler)
    {
        _getUserRoleQueryHandler = getUserRoleQueryHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(Http.GET);
        Routes(ApiRoutes.Authorization.GET_USER_ROLE_BY_USER_ID);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Gets the authorization role of a user.
    /// </summary>
    /// <param name="request">The request containing the user for whom to get the authorization role.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(GetUserRoleRequest request, CancellationToken cancellationToken)
    {
        Result<RoleResponse?> result = await _getUserRoleQueryHandler.HandleAsync(request.ToQuery(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
