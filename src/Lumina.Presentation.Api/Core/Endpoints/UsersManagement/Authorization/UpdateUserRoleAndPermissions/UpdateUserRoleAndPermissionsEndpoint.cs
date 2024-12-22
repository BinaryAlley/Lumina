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

namespace Lumina.Presentation.Api.Core.Endpoints.UsersManagement.Authorization.UpdateUserRoleAndPermissions;

/// <summary>
/// API endpoint for the <c>/auth/users/{userId}/role-and-permissions</c> route.
/// </summary>
public class UpdateUserRoleAndPermissionsEndpoint : BaseEndpoint<UpdateUserRoleAndPermissionsRequest, IResult>
{
    private readonly ISender _sender;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateUserRoleAndPermissionsEndpoint"/> class.
    /// </summary>
    /// <param name="sender">Injected service for mediating commands and queries.</param>
    public UpdateUserRoleAndPermissionsEndpoint(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(Http.PUT);
        Routes(ApiRoutes.Authorization.UPDATE_USER_ROLE_AND_PERMISSIONS_BY_USER_ID);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Updates an the authorization role and permissions of a user.
    /// </summary>
    /// <param name="request">The request containing the authorization role and permission of the user to be updated.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(UpdateUserRoleAndPermissionsRequest request, CancellationToken cancellationToken)
    {
        ErrorOr<AuthorizationResponse> result = await _sender.Send(request.ToCommand(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
