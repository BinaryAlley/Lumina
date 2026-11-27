#region ========================================================================= USING =====================================================================================
using Nemera.Domain.Common.Primitives;
using Nemera.Application.Common.CQRS;
using Nemera.Application.Common.Mapping.Authorization;
using Nemera.Application.Core.Admin.Authorization.Roles.Commands.DeleteRole;
using Nemera.Contracts.Requests.Authorization;
using Nemera.Presentation.Api.Common.Routes.UsersManagement;
using Nemera.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Nemera.Presentation.Api.Core.Endpoints.Admin.Authorization.Roles.DeleteRole;

/// <summary>
/// API endpoint for the <c>/auth/roles/{roleId}</c> route.
/// </summary>
public class DeleteRoleEndpoint : BaseEndpoint<DeleteRoleRequest, IResult>
{
    private readonly ICommandHandler<DeleteRoleCommand, Result<Deleted>> _deleteRoleCommandHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteRoleEndpoint"/> class.
    /// </summary>
    /// <param name="deleteRoleCommandHandler">Injected service for handling delete role commands.</param>
    public DeleteRoleEndpoint(ICommandHandler<DeleteRoleCommand, Result<Deleted>> deleteRoleCommandHandler)
    {
        _deleteRoleCommandHandler = deleteRoleCommandHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(FastEndpoints.Http.DELETE);
        Routes(ApiRoutes.Roles.DELETE_ROLE);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Deletes an authorization role.
    /// </summary>
    /// <param name="request">The request containing the Id of the authorization role to be deleted.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(DeleteRoleRequest request, CancellationToken cancellationToken)
    {
        Result<Deleted> result = await _deleteRoleCommandHandler.HandleAsync(request.ToCommand(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.NoContent(), Problem);
    }
}
