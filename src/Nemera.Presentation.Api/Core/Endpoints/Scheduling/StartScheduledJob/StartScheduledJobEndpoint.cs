#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.Mapping.Scheduling;
using Lumina.Application.Core.Scheduling.Commands.StartScheduledJob;
using Lumina.Contracts.Requests.Scheduling;
using Lumina.Contracts.Responses.Scheduling;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Common.Routes.Scheduling;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Scheduling.StartScheduledJob;

/// <summary>
/// API endpoint for the <c>/scheduled-jobs/{scheduledJobId}/start</c> route.
/// </summary>
public class StartScheduledJobEndpoint : BaseEndpoint<StartScheduledJobRequest, IResult>
{
    private readonly ICommandHandler<StartScheduledJobCommand, Result<ScheduledJobResponse>> _startScheduledJobCommandHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="StartScheduledJobEndpoint"/> class.
    /// </summary>
    /// <param name="startScheduledJobCommandHandler">Injected service for handling start scheduled job commands.</param>
    public StartScheduledJobEndpoint(ICommandHandler<StartScheduledJobCommand, Result<ScheduledJobResponse>> startScheduledJobCommandHandler)
    {
        _startScheduledJobCommandHandler = startScheduledJobCommandHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(FastEndpoints.Http.PUT);
        Routes(ApiRoutes.ScheduledJobs.START_SCHEDULED_JOB);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Starts the execution cycle of a scheduled job.
    /// </summary>
    /// <param name="request">The request object.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(StartScheduledJobRequest request, CancellationToken cancellationToken)
    {
        Result<ScheduledJobResponse> result = await _startScheduledJobCommandHandler.HandleAsync(request.ToCommand(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}


