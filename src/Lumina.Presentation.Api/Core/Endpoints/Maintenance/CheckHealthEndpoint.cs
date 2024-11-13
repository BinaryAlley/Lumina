#region ========================================================================= USING =====================================================================================
using ErrorOr;
using FastEndpoints;
using Lumina.Application.Common.Mapping.FileSystemManagement.Paths;
using Lumina.Contracts.Requests.FileSystemManagement.Directories;
using Lumina.Application.Common.Mapping.FileSystemManagement.Directories;
using Lumina.Contracts.Responses.FileSystemManagement;
using Lumina.Presentation.Api.Common.Routes.Maintenance;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Mediator;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Maintenance;

/// <summary>
/// API endpoint for the <c>/directories/get-directories</c> route.
/// </summary>
public class CheckHealthEndpoint : BaseEndpoint<EmptyRequest, IResult>
{
    private readonly ISender _sender;

    /// <summary>
    /// Initializes a new instance of the <see cref="CheckHealthEndpoint"/> class.
    /// </summary>
    /// <param name="sender">Injected service for mediating commands and queries.</param>
    public CheckHealthEndpoint(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(Http.GET);
        Routes(ApiRoutes.Health.CHECK_HEALTH);
        Version(1);
        AllowAnonymous();
        DontCatchExceptions();
    }

    /// <summary>
    /// Gwts the health status of the backend system.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(EmptyRequest _, CancellationToken cancellationToken)
    {
        // TODO: to be implemented
        return await Task.FromResult(TypedResults.Ok());
    }
}
