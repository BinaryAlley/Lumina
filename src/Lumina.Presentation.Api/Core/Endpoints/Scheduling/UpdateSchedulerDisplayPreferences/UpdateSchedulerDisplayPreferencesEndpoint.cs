#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.Mapping.Scheduling;
using Lumina.Application.Core.Scheduling.Commands.UpdateSchedulerDisplayPreferences;
using Lumina.Contracts.Requests.Scheduling;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Common.Routes.Scheduling;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Scheduling.UpdateSchedulerDisplayPreferences;

/// <summary>
/// API endpoint for the <c>/scheduled-jobs/display-preferences</c> route.
/// </summary>
public class UpdateSchedulerDisplayPreferencesEndpoint : BaseEndpoint<UpdateSchedulerDisplayPreferencesRequest, IResult>
{
    private readonly ICommandHandler<UpdateSchedulerDisplayPreferencesCommand, Result<Updated>> _updateSchedulerDisplayPreferencesCommandHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateSchedulerDisplayPreferencesEndpoint"/> class.
    /// </summary>
    /// <param name="updateSchedulerDisplayPreferencesCommandHandler">Injected service for handling update scheduler display preferences commands.</param>
    public UpdateSchedulerDisplayPreferencesEndpoint(ICommandHandler<UpdateSchedulerDisplayPreferencesCommand, Result<Updated>> updateSchedulerDisplayPreferencesCommandHandler)
    {
        _updateSchedulerDisplayPreferencesCommandHandler = updateSchedulerDisplayPreferencesCommandHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(FastEndpoints.Http.PUT);
        Routes(ApiRoutes.ScheduledJobs.UPDATE_DISPLAY_PREFERENCES);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Updates the display preferences of the scheduler page of the current user.
    /// </summary>
    /// <param name="request">The request containing the display preferences of the scheduler page of the current user.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(UpdateSchedulerDisplayPreferencesRequest request, CancellationToken cancellationToken)
    {
        Result<Updated> result = await _updateSchedulerDisplayPreferencesCommandHandler.HandleAsync(request.ToCommand(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
