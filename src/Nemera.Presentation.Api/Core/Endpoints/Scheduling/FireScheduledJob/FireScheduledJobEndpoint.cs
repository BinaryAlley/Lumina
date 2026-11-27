#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.CQRS;
using Nemera.Application.Common.Mapping.Scheduling;
using Nemera.Application.Core.Scheduling.Commands.FireScheduledJob;
using Nemera.Contracts.Requests.Scheduling;
using Nemera.Contracts.Responses.Scheduling;
using Nemera.Domain.Common.Primitives;
using Nemera.Presentation.Api.Common.Routes.Scheduling;
using Nemera.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Nemera.Presentation.Api.Core.Endpoints.Scheduling.FireScheduledJob;

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


