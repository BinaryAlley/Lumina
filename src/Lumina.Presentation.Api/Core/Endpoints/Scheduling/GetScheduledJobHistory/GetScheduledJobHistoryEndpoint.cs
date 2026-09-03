#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.Mapping.Scheduling;
using Lumina.Application.Core.Scheduling.Queries.GetScheduledJobHistory;
using Lumina.Contracts.Requests.Scheduling;
using Lumina.Contracts.Responses.Scheduling;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Common.Routes.Scheduling;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Scheduling.GetScheduledJobHistory;

/// <summary>
/// API endpoint for the <c>/scheduled-jobs/history</c> route.
/// </summary>
public class GetScheduledJobHistoryEndpoint : BaseEndpoint<GetScheduledJobHistoryRequest, IResult>
{
    private readonly IQueryHandler<GetScheduledJobHistoryQuery, Result<IEnumerable<ScheduledJobExecutionResponse>>> _getScheduledJobHistoryQueryHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetScheduledJobHistoryEndpoint"/> class.
    /// </summary>
    /// <param name="getScheduledJobHistoryQueryHandler">Injected service for handling get scheduled job history queries.</param>
    public GetScheduledJobHistoryEndpoint(IQueryHandler<GetScheduledJobHistoryQuery, Result<IEnumerable<ScheduledJobExecutionResponse>>> getScheduledJobHistoryQueryHandler)
    {
        _getScheduledJobHistoryQueryHandler = getScheduledJobHistoryQueryHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(FastEndpoints.Http.GET);
        Routes(ApiRoutes.ScheduledJobs.GET_SCHEDULED_JOB_HISTORY);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Gets the history of the executions of the tasks of scheduled jobs.
    /// </summary>
    /// <param name="request">The request object.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(GetScheduledJobHistoryRequest request, CancellationToken cancellationToken)
    {
        Result<IEnumerable<ScheduledJobExecutionResponse>> result = await _getScheduledJobHistoryQueryHandler.HandleAsync(request.ToQuery(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}


