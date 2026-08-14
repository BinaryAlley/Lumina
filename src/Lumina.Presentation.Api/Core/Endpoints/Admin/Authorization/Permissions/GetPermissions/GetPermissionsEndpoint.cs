#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using FastEndpoints;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.Admin.Authorization.Permissions.Queries.GetPermissions;
using Lumina.Contracts.Responses.Authorization;
using Lumina.Presentation.Api.Common.Routes.UsersManagement;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Admin.Authorization.Permissions.GetPermissions;

/// <summary>
/// API endpoint for the <c>/auth/permissions</c> route.
/// </summary>
public class GetPermissionsEndpoint : BaseEndpoint<EmptyRequest, IResult>
{
    private readonly IQueryHandler<GetPermissionsQuery, Result<IEnumerable<PermissionResponse>>> _getPermissionsQueryHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPermissionsEndpoint"/> class.
    /// </summary>
    /// <param name="getPermissionsQueryHandler">Injected service for handling get permissions queries.</param>
    public GetPermissionsEndpoint(IQueryHandler<GetPermissionsQuery, Result<IEnumerable<PermissionResponse>>> getPermissionsQueryHandler)
    {
        _getPermissionsQueryHandler = getPermissionsQueryHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(FastEndpoints.Http.GET);
        Routes(ApiRoutes.Permissions.GET_PERMISSIONS);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Gets the list of authorization permissions.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(EmptyRequest _, CancellationToken cancellationToken)
    {
        Result<IEnumerable<PermissionResponse>> result = await _getPermissionsQueryHandler.HandleAsync(new GetPermissionsQuery(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
