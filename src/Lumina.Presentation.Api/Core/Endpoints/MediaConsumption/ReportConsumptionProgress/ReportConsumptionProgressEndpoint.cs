#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.Mapping.MediaConsumption;
using Lumina.Application.Core.MediaConsumption.Commands.ReportConsumptionProgress;
using Lumina.Contracts.Requests.MediaConsumption;
using Lumina.Contracts.Responses.MediaConsumption;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Common.Routes.MediaConsumption;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.MediaConsumption.ReportConsumptionProgress;

/// <summary>
/// API endpoint for the <c>/users/me/consumption-records/{recordId}/progress</c> route.
/// </summary>
/// <remarks>
/// Uses "me" because it is always for the currently authenticated user.
/// </remarks>
public class ReportConsumptionProgressEndpoint : BaseEndpoint<ReportConsumptionProgressRequest, IResult>
{
    private readonly ICommandHandler<ReportConsumptionProgressCommand, Result<ConsumptionRecordResponse>> _reportConsumptionProgressCommandHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReportConsumptionProgressEndpoint"/> class.
    /// </summary>
    /// <param name="reportConsumptionProgressCommandHandler">Injected service for handling report consumption progress commands.</param>
    public ReportConsumptionProgressEndpoint(ICommandHandler<ReportConsumptionProgressCommand, Result<ConsumptionRecordResponse>> reportConsumptionProgressCommandHandler)
    {
        _reportConsumptionProgressCommandHandler = reportConsumptionProgressCommandHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(FastEndpoints.Http.PUT);
        Routes(ApiRoutes.MediaConsumption.REPORT_CONSUMPTION_PROGRESS);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Reports the progress of a consumption session of the current user.
    /// </summary>
    /// <param name="request">The request containing the position, the progress fraction, and the consumed duration.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(ReportConsumptionProgressRequest request, CancellationToken cancellationToken)
    {
        // The consumption record belongs to the authenticated user, so no intermediary may store any response of this route, including its problem
        // responses, and every response varies by the bearer token that authorized it.
        HttpContext.Response.Headers["Cache-Control"] = "private, no-store";
        HttpContext.Response.Headers.Append("Vary", "Authorization");
        // Take the unique identifier from the route, so that it cannot be replaced by a value posted in the body.
        string? recordId = HttpContext.Request.RouteValues["recordId"]?.ToString();
        Result<ConsumptionRecordResponse> result = await _reportConsumptionProgressCommandHandler.HandleAsync(request.ToCommand(recordId), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
