#region ========================================================================= USING =====================================================================================
using ErrorOr;
using FastEndpoints;
using Lumina.Application.Common.Mapping.Authentication;
using Lumina.Application.Common.Mapping.Authorization;
using Lumina.Contracts.Requests.Authorization;
using Lumina.Contracts.Responses.Authorization;
using Lumina.Presentation.Api.Common.Routes.UsersManagement;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Mediator;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.UsersManagement.Authorization.GetUserRole;

/// <summary>
/// API endpoint for the <c>/auth/users/{userId}/role</c> route.
/// </summary>
public class GetUserRoleEndpoint : BaseEndpoint<GetUserRoleRequest, IResult>
{
    private readonly ISender _sender;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetUserRoleEndpoint"/> class.
    /// </summary>
    /// <param name="sender">Injected service for mediating commands and queries.</param>
    public GetUserRoleEndpoint(ISender sender)
    {
        _sender = sender;
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
        ErrorOr<RoleResponse?> result = await _sender.Send(request.ToQuery(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
