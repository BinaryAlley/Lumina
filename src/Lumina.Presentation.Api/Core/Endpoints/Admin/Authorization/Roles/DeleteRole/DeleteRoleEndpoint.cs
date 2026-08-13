#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.Mapping.Authorization;
using Lumina.Application.Core.Admin.Authorization.Roles.Commands.DeleteRole;
using Lumina.Contracts.Requests.Authorization;
using Lumina.Presentation.Api.Common.Routes.UsersManagement;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Admin.Authorization.Roles.DeleteRole;

/// <summary>
/// API endpoint for the <c>/auth/roles</c> route.
/// </summary>
public class DeleteRoleEndpoint : BaseEndpoint<DeleteRoleRequest, IResult>
{
    private readonly ICommandHandler<DeleteRoleCommand, ErrorOr<Deleted>> _deleteRoleCommandHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteRoleEndpoint"/> class.
    /// </summary>
    /// <param name="deleteRoleCommandHandler">Injected service for handling delete role commands.</param>
    public DeleteRoleEndpoint(ICommandHandler<DeleteRoleCommand, ErrorOr<Deleted>> deleteRoleCommandHandler)
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
        ErrorOr<Deleted> result = await _deleteRoleCommandHandler.HandleAsync(request.ToCommand(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.NoContent(), Problem);
    }
}
