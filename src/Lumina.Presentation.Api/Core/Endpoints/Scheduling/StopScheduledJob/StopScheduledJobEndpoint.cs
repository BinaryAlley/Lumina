#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.Mapping.Scheduling;
using Lumina.Application.Core.Scheduling.Commands.StopScheduledJob;
using Lumina.Contracts.Requests.Scheduling;
using Lumina.Contracts.Responses.Scheduling;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Common.Routes.Scheduling;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Scheduling.StopScheduledJob;

/// <summary>
/// API endpoint for the <c>/scheduled-jobs/{scheduledJobId}/stop</c> route.
/// </summary>
public class StopScheduledJobEndpoint : BaseEndpoint<StopScheduledJobRequest, IResult>
{
    private readonly ICommandHandler<StopScheduledJobCommand, Result<ScheduledJobResponse>> _stopScheduledJobCommandHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="StopScheduledJobEndpoint"/> class.
    /// </summary>
    /// <param name="stopScheduledJobCommandHandler">Injected service for handling stop scheduled job commands.</param>
    public StopScheduledJobEndpoint(ICommandHandler<StopScheduledJobCommand, Result<ScheduledJobResponse>> stopScheduledJobCommandHandler)
    {
        _stopScheduledJobCommandHandler = stopScheduledJobCommandHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(FastEndpoints.Http.PUT);
        Routes(ApiRoutes.ScheduledJobs.STOP_SCHEDULED_JOB);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Stops the execution cycle of a scheduled job.
    /// </summary>
    /// <param name="request">The request object.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(StopScheduledJobRequest request, CancellationToken cancellationToken)
    {
        Result<ScheduledJobResponse> result = await _stopScheduledJobCommandHandler.HandleAsync(request.ToCommand(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}


