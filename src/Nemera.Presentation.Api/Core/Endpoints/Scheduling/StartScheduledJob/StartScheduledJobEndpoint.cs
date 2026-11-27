#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.CQRS;
using Nemera.Application.Common.Mapping.Scheduling;
using Nemera.Application.Core.Scheduling.Commands.StartScheduledJob;
using Nemera.Contracts.Requests.Scheduling;
using Nemera.Contracts.Responses.Scheduling;
using Nemera.Domain.Common.Primitives;
using Nemera.Presentation.Api.Common.Routes.Scheduling;
using Nemera.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Nemera.Presentation.Api.Core.Endpoints.Scheduling.StartScheduledJob;

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


