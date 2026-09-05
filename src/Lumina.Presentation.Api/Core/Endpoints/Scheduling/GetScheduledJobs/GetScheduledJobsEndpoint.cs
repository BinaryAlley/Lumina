#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.Scheduling.Queries.GetScheduledJobs;
using Lumina.Contracts.Responses.Scheduling;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Common.Routes.Scheduling;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Scheduling.GetScheduledJobs;

/// <summary>
/// API endpoint for the <c>/scheduled-jobs</c> route.
/// </summary>
public class GetScheduledJobsEndpoint : BaseEndpoint<FastEndpoints.EmptyRequest, IResult>
{
    private readonly IQueryHandler<GetScheduledJobsQuery, Result<IEnumerable<ScheduledJobResponse>>> _getScheduledJobsQueryHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetScheduledJobsEndpoint"/> class.
    /// </summary>
    /// <param name="getScheduledJobsQueryHandler">Injected service for handling get scheduled jobs queries.</param>
    public GetScheduledJobsEndpoint(IQueryHandler<GetScheduledJobsQuery, Result<IEnumerable<ScheduledJobResponse>>> getScheduledJobsQueryHandler)
    {
        _getScheduledJobsQueryHandler = getScheduledJobsQueryHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(FastEndpoints.Http.GET);
        Routes(ApiRoutes.ScheduledJobs.GET_SCHEDULED_JOBS);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Gets the list of scheduled jobs.
    /// </summary>
    /// <param name="request">The request object.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(FastEndpoints.EmptyRequest request, CancellationToken cancellationToken)
    {
        Result<IEnumerable<ScheduledJobResponse>> result = await _getScheduledJobsQueryHandler.HandleAsync(new GetScheduledJobsQuery(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}


