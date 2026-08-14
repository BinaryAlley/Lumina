#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using FastEndpoints;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.Admin.Authorization.Roles.Queries.GetRoles;
using Lumina.Contracts.Responses.Authorization;
using Lumina.Presentation.Api.Common.Routes.UsersManagement;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Admin.Authorization.Roles.GetRoles;

/// <summary>
/// API endpoint for the <c>/auth/roles</c> route.
/// </summary>
public class GetRolesEndpoint : BaseEndpoint<EmptyRequest, IResult>
{
    private readonly IQueryHandler<GetRolesQuery, Result<IEnumerable<RoleResponse>>> _getRolesQueryHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetRolesEndpoint"/> class.
    /// </summary>
    /// <param name="getRolesQueryHandler">Injected service for handling get roles queries.</param>
    public GetRolesEndpoint(IQueryHandler<GetRolesQuery, Result<IEnumerable<RoleResponse>>> getRolesQueryHandler)
    {
        _getRolesQueryHandler = getRolesQueryHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(FastEndpoints.Http.GET);
        Routes(ApiRoutes.Roles.GET_ROLES);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Gets the list of authorization roles.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(EmptyRequest _, CancellationToken cancellationToken)
    {
        Result<IEnumerable<RoleResponse>> result = await _getRolesQueryHandler.HandleAsync(new GetRolesQuery(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
