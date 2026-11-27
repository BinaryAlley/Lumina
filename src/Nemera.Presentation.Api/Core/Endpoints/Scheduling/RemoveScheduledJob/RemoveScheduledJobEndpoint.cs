#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.Mapping.Scheduling;
using Lumina.Application.Core.Scheduling.Commands.RemoveScheduledJob;
using Lumina.Contracts.Requests.Scheduling;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Common.Routes.Scheduling;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Scheduling.RemoveScheduledJob;

/// <summary>
/// API endpoint for the <c>/scheduled-jobs/{scheduledJobId}</c> route.
/// </summary>
public class RemoveScheduledJobEndpoint : BaseEndpoint<RemoveScheduledJobRequest, IResult>
{
    private readonly ICommandHandler<RemoveScheduledJobCommand, Result<Success>> _removeScheduledJobCommandHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="RemoveScheduledJobEndpoint"/> class.
    /// </summary>
    /// <param name="removeScheduledJobCommandHandler">Injected service for handling remove scheduled job commands.</param>
    public RemoveScheduledJobEndpoint(ICommandHandler<RemoveScheduledJobCommand, Result<Success>> removeScheduledJobCommandHandler)
    {
        _removeScheduledJobCommandHandler = removeScheduledJobCommandHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(FastEndpoints.Http.DELETE);
        Routes(ApiRoutes.ScheduledJobs.REMOVE_SCHEDULED_JOB);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Removes a scheduled job.
    /// </summary>
    /// <param name="request">The request object.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(RemoveScheduledJobRequest request, CancellationToken cancellationToken)
    {
        Result<Success> result = await _removeScheduledJobCommandHandler.HandleAsync(request.ToCommand(), cancellationToken).ConfigureAwait(false);
        return result.Match(_ => TypedResults.Ok(new { }), Problem);
    }
}


