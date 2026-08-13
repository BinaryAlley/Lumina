#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.Mapping.Authorization;
using Lumina.Application.Core.Admin.Authorization.Roles.Commands.AddRole;
using Lumina.Contracts.Requests.Authorization;
using Lumina.Contracts.Responses.Authorization;
using Lumina.Presentation.Api.Common.Routes.UsersManagement;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Admin.Authorization.Roles.AddRole;

/// <summary>
/// API endpoint for the <c>/auth/roles</c> route.
/// </summary>
public class AddRoleEndpoint : BaseEndpoint<AddRoleRequest, IResult>
{
    private readonly ICommandHandler<AddRoleCommand, ErrorOr<RolePermissionsResponse>> _addRoleCommandHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="AddRoleEndpoint"/> class.
    /// </summary>
    /// <param name="addRoleCommandHandler">Injected service for handling add role commands.</param>
    public AddRoleEndpoint(ICommandHandler<AddRoleCommand, ErrorOr<RolePermissionsResponse>> addRoleCommandHandler)
    {
        _addRoleCommandHandler = addRoleCommandHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(FastEndpoints.Http.POST);
        Routes(ApiRoutes.Roles.CREATE_ROLE);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Creates a new authorization role.
    /// </summary>
    /// <param name="request">The request containing the authorization role to be created.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(AddRoleRequest request, CancellationToken cancellationToken)
    {
        ErrorOr<RolePermissionsResponse> result = await _addRoleCommandHandler.HandleAsync(request.ToCommand(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
