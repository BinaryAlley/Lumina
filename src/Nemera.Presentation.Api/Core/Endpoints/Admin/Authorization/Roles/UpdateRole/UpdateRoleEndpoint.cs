#region ========================================================================= USING =====================================================================================
using Nemera.Domain.Common.Primitives;
using Nemera.Application.Common.CQRS;
using Nemera.Application.Common.Mapping.Authorization;
using Nemera.Application.Core.Admin.Authorization.Roles.Commands.UpdateRole;
using Nemera.Contracts.Requests.Authorization;
using Nemera.Contracts.Responses.Authorization;
using Nemera.Presentation.Api.Common.Routes.UsersManagement;
using Nemera.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Nemera.Presentation.Api.Core.Endpoints.Admin.Authorization.Roles.UpdateRole;

/// <summary>
/// API endpoint for the <c>/auth/roles</c> route.
/// </summary>
public class UpdateRoleEndpoint : BaseEndpoint<UpdateRoleRequest, IResult>
{
    private readonly ICommandHandler<UpdateRoleCommand, Result<RolePermissionsResponse>> _updateRoleCommandHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateRoleEndpoint"/> class.
    /// </summary>
    /// <param name="updateRoleCommandHandler">Injected service for handling update role commands.</param>
    public UpdateRoleEndpoint(ICommandHandler<UpdateRoleCommand, Result<RolePermissionsResponse>> updateRoleCommandHandler)
    {
        _updateRoleCommandHandler = updateRoleCommandHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(FastEndpoints.Http.PUT);
        Routes(ApiRoutes.Roles.UPDATE_ROLE);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Updates an existing authorization role.
    /// </summary>
    /// <param name="request">The request containing the authorization role to be updated.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(UpdateRoleRequest request, CancellationToken cancellationToken)
    {
        Result<RolePermissionsResponse> result = await _updateRoleCommandHandler.HandleAsync(request.ToCommand(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
