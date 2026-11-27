#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.CQRS;
using Nemera.Application.Common.Mapping.Scheduling;
using Nemera.Application.Core.Scheduling.Queries.GetScheduledJobHistory;
using Nemera.Contracts.Requests.Scheduling;
using Nemera.Contracts.Responses.Scheduling;
using Nemera.Domain.Common.Primitives;
using Nemera.Presentation.Api.Common.Routes.Scheduling;
using Nemera.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Nemera.Presentation.Api.Core.Endpoints.Scheduling.GetScheduledJobHistory;

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


