#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.Mapping.Scheduling;
using Lumina.Application.Core.Scheduling.Commands.AddScheduledJob;
using Lumina.Contracts.Requests.Scheduling;
using Lumina.Contracts.Responses.Scheduling;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Common.Routes.Scheduling;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Scheduling.AddScheduledJob;

/// <summary>
/// API endpoint for the <c>/scheduled-jobs</c> route.
/// </summary>
public class AddScheduledJobEndpoint : BaseEndpoint<AddScheduledJobRequest, IResult>
{
    private readonly ICommandHandler<AddScheduledJobCommand, Result<ScheduledJobResponse>> _addScheduledJobCommandHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="AddScheduledJobEndpoint"/> class.
    /// </summary>
    /// <param name="addScheduledJobCommandHandler">Injected service for handling add scheduled job commands.</param>
    public AddScheduledJobEndpoint(ICommandHandler<AddScheduledJobCommand, Result<ScheduledJobResponse>> addScheduledJobCommandHandler)
    {
        _addScheduledJobCommandHandler = addScheduledJobCommandHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(FastEndpoints.Http.POST);
        Routes(ApiRoutes.ScheduledJobs.ADD_SCHEDULED_JOB);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Adds a scheduled job.
    /// </summary>
    /// <param name="request">The request object.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(AddScheduledJobRequest request, CancellationToken cancellationToken)
    {
        Result<ScheduledJobResponse> result = await _addScheduledJobCommandHandler.HandleAsync(request.ToCommand(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}


