#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.Mapping.Scheduling;
using Lumina.Application.Core.Scheduling.Commands.FireScheduledJob;
using Lumina.Contracts.Requests.Scheduling;
using Lumina.Contracts.Responses.Scheduling;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Common.Routes.Scheduling;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Scheduling.FireScheduledJob;

/// <summary>
/// API endpoint for the <c>/scheduled-jobs/{scheduledJobId}/fire</c> route.
/// </summary>
public class FireScheduledJobEndpoint : BaseEndpoint<FireScheduledJobRequest, IResult>
{
    private readonly ICommandHandler<FireScheduledJobCommand, Result<ScheduledJobResponse>> _fireScheduledJobCommandHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="FireScheduledJobEndpoint"/> class.
    /// </summary>
    /// <param name="fireScheduledJobCommandHandler">Injected service for handling fire scheduled job commands.</param>
    public FireScheduledJobEndpoint(ICommandHandler<FireScheduledJobCommand, Result<ScheduledJobResponse>> fireScheduledJobCommandHandler)
    {
        _fireScheduledJobCommandHandler = fireScheduledJobCommandHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(FastEndpoints.Http.PUT);
        Routes(ApiRoutes.ScheduledJobs.FIRE_SCHEDULED_JOB);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Fires the task of a scheduled job once, without affecting its execution cycle.
    /// </summary>
    /// <param name="request">The request object.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(FireScheduledJobRequest request, CancellationToken cancellationToken)
    {
        Result<ScheduledJobResponse> result = await _fireScheduledJobCommandHandler.HandleAsync(request.ToCommand(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}


