#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.Mapping.Authorization;
using Lumina.Application.Core.UsersManagement.Authorization.Commands.UpdateUserRoleAndPermissions;
using Lumina.Contracts.Requests.Authorization;
using Lumina.Contracts.Responses.Authorization;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Common.Routes.UsersManagement;
using Lumina.Presentation.Api.Core.Endpoints.Common;
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
    private readonly ICommandHandler<UpdateUserRoleAndPermissionsCommand, Result<AuthorizationResponse>> _updateUserRoleAndPermissionsCommandHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateUserRoleAndPermissionsEndpoint"/> class.
    /// </summary>
    /// <param name="updateUserRoleAndPermissionsCommandHandler">Injected service for handling update user role and permissions commands.</param>
    public UpdateUserRoleAndPermissionsEndpoint(ICommandHandler<UpdateUserRoleAndPermissionsCommand, Result<AuthorizationResponse>> updateUserRoleAndPermissionsCommandHandler)
    {
        _updateUserRoleAndPermissionsCommandHandler = updateUserRoleAndPermissionsCommandHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(FastEndpoints.Http.PUT);
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
        Result<AuthorizationResponse> result = await _updateUserRoleAndPermissionsCommandHandler.HandleAsync(request.ToCommand(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
