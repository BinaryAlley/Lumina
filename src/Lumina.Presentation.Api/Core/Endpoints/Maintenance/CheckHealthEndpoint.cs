#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Api.Common.Routes.Maintenance;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Maintenance;

/// <summary>
/// API endpoint for the <c>/directories/get-directories</c> route.
/// </summary>
public class CheckHealthEndpoint : BaseEndpoint<EmptyRequest, IResult>
{
    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(FastEndpoints.Http.GET);
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
