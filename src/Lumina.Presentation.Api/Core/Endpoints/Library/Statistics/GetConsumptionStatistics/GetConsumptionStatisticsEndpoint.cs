#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.Mapping.MediaConsumption;
using Lumina.Application.Core.MediaConsumption.Queries.GetConsumptionStatistics;
using Lumina.Contracts.Requests.MediaConsumption;
using Lumina.Contracts.Responses.MediaConsumption;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Common.Routes.Library.Statistics;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Library.Statistics.GetConsumptionStatistics;

/// <summary>
/// API endpoint for the <c>/users/me/consumption-statistics</c> route.
/// </summary>
/// <remarks>
/// Uses "me" because it is always for the currently authenticated user.
/// </remarks>
public class GetConsumptionStatisticsEndpoint : BaseEndpoint<GetConsumptionStatisticsRequest, IResult>
{
    private readonly IQueryHandler<GetConsumptionStatisticsQuery, Result<ConsumptionSummaryResponse>> _getConsumptionStatisticsQueryHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetConsumptionStatisticsEndpoint"/> class.
    /// </summary>
    /// <param name="getConsumptionStatisticsQueryHandler">Injected service for handling get consumption statistics queries.</param>
    public GetConsumptionStatisticsEndpoint(IQueryHandler<GetConsumptionStatisticsQuery, Result<ConsumptionSummaryResponse>> getConsumptionStatisticsQueryHandler)
    {
        _getConsumptionStatisticsQueryHandler = getConsumptionStatisticsQueryHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(FastEndpoints.Http.GET);
        Routes(ApiRoutes.LibraryStatistics.GET_CONSUMPTION_STATISTICS);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Gets the consumption statistics of the current user.
    /// </summary>
    /// <param name="request">The request containing the optional filters and the date range.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(GetConsumptionStatisticsRequest request, CancellationToken cancellationToken)
    {
        // The consumption statistics belong to the authenticated user, so no intermediary may store any response of this route, including its
        // problem responses, and every response varies by the bearer token that authorized it.
        HttpContext.Response.Headers["Cache-Control"] = "private, no-store";
        HttpContext.Response.Headers.Append("Vary", "Authorization");
        Result<ConsumptionSummaryResponse> result = await _getConsumptionStatisticsQueryHandler.HandleAsync(request.ToQuery(targetUserId: null), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
