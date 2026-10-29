#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.Mapping.MediaConsumption;
using Lumina.Application.Core.MediaConsumption.Queries.GetAllUsersConsumptionStatistics;
using Lumina.Contracts.Requests.MediaConsumption;
using Lumina.Contracts.Responses.MediaConsumption;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Common.Routes.Library.Statistics;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Library.Statistics.GetAllUsersConsumptionStatistics;

/// <summary>
/// API endpoint for the <c>/users/consumption-statistics</c> route.
/// </summary>
public class GetAllUsersConsumptionStatisticsEndpoint : BaseEndpoint<GetAllUsersConsumptionStatisticsRequest, IResult>
{
    private readonly IQueryHandler<GetAllUsersConsumptionStatisticsQuery, Result<AllUsersConsumptionStatisticsResponse>> _getAllUsersConsumptionStatisticsQueryHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetAllUsersConsumptionStatisticsEndpoint"/> class.
    /// </summary>
    /// <param name="getAllUsersConsumptionStatisticsQueryHandler">Injected service for handling get all users consumption statistics queries.</param>
    public GetAllUsersConsumptionStatisticsEndpoint(IQueryHandler<GetAllUsersConsumptionStatisticsQuery, Result<AllUsersConsumptionStatisticsResponse>> getAllUsersConsumptionStatisticsQueryHandler)
    {
        _getAllUsersConsumptionStatisticsQueryHandler = getAllUsersConsumptionStatisticsQueryHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(FastEndpoints.Http.GET);
        Routes(ApiRoutes.LibraryStatistics.GET_ALL_USERS_CONSUMPTION_STATISTICS);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Gets the consumption statistics of every user.
    /// </summary>
    /// <param name="request">The request containing the optional filters and the date range.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(GetAllUsersConsumptionStatisticsRequest request, CancellationToken cancellationToken)
    {
        // The consumption statistics belong to every user of the server, so no intermediary may store any response of this route, including its
        // problem responses, and every response varies by the bearer token that authorized it.
        HttpContext.Response.Headers["Cache-Control"] = "private, no-store";
        HttpContext.Response.Headers.Append("Vary", "Authorization");
        // The handler is the only place that checks the admin role, so a regular user receives the same not authorized result as any other protected administration route.
        Result<AllUsersConsumptionStatisticsResponse> result = await _getAllUsersConsumptionStatisticsQueryHandler.HandleAsync(request.ToQuery(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
