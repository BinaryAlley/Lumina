#region ========================================================================= USING =====================================================================================
using Nemera.Domain.Common.Primitives;
using Nemera.Application.Common.CQRS;
using Nemera.Application.Common.Mapping.Authorization;
using Nemera.Application.Core.Admin.Authorization.Roles.Commands.AddRole;
using Nemera.Contracts.Requests.Authorization;
using Nemera.Contracts.Responses.Authorization;
using Nemera.Presentation.Api.Common.Routes.UsersManagement;
using Nemera.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Nemera.Presentation.Api.Core.Endpoints.Admin.Authorization.Roles.AddRole;

/// <summary>
/// API endpoint for the <c>/auth/roles</c> route.
/// </summary>
public class AddRoleEndpoint : BaseEndpoint<AddRoleRequest, IResult>
{
    private readonly ICommandHandler<AddRoleCommand, Result<RolePermissionsResponse>> _addRoleCommandHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="AddRoleEndpoint"/> class.
    /// </summary>
    /// <param name="addRoleCommandHandler">Injected service for handling add role commands.</param>
    public AddRoleEndpoint(ICommandHandler<AddRoleCommand, Result<RolePermissionsResponse>> addRoleCommandHandler)
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
    /// Adds a new authorization role.
    /// </summary>
    /// <param name="request">The request containing the authorization role to be created.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(AddRoleRequest request, CancellationToken cancellationToken)
    {
        Result<RolePermissionsResponse> result = await _addRoleCommandHandler.HandleAsync(request.ToCommand(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
