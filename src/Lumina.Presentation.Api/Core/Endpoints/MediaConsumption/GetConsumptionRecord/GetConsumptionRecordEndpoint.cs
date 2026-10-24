#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.MediaConsumption.Queries.GetConsumptionRecord;
using Lumina.Contracts.Responses.MediaConsumption;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Common.Routes.MediaConsumption;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.MediaConsumption.GetConsumptionRecord;

/// <summary>
/// API endpoint for the <c>/users/me/consumption-records/{recordId}</c> route.
/// </summary>
/// <remarks>
/// Uses "me" because it is always for the currently authenticated user.
/// </remarks>
public class GetConsumptionRecordEndpoint : BaseEndpoint<FastEndpoints.EmptyRequest, IResult>
{
    private readonly IQueryHandler<GetConsumptionRecordQuery, Result<ConsumptionRecordResponse>> _getConsumptionRecordQueryHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetConsumptionRecordEndpoint"/> class.
    /// </summary>
    /// <param name="getConsumptionRecordQueryHandler">Injected service for handling get consumption record queries.</param>
    public GetConsumptionRecordEndpoint(IQueryHandler<GetConsumptionRecordQuery, Result<ConsumptionRecordResponse>> getConsumptionRecordQueryHandler)
    {
        _getConsumptionRecordQueryHandler = getConsumptionRecordQueryHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(FastEndpoints.Http.GET);
        Routes(ApiRoutes.MediaConsumption.GET_CONSUMPTION_RECORD);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Gets a consumption record of the current user.
    /// </summary>
    /// <param name="request">The request object.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(FastEndpoints.EmptyRequest request, CancellationToken cancellationToken)
    {
        // The consumption record belongs to the authenticated user, so no intermediary may store any response of this route, including its problem
        // responses, and every response varies by the bearer token that authorized it.
        HttpContext.Response.Headers["Cache-Control"] = "private, no-store";
        HttpContext.Response.Headers.Append("Vary", "Authorization");
        // Take the unique identifier from the route.
        string? recordId = HttpContext.Request.RouteValues["recordId"]?.ToString();
        Result<ConsumptionRecordResponse> result = await _getConsumptionRecordQueryHandler.HandleAsync(new GetConsumptionRecordQuery(recordId), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
