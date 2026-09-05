#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.Scheduling.Queries.GetSchedulerDisplayPreferences;
using Lumina.Contracts.Responses.Scheduling;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Common.Routes.Scheduling;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Scheduling.GetSchedulerDisplayPreferences;

/// <summary>
/// API endpoint for the <c>/scheduled-jobs/display-preferences</c> route.
/// </summary>
public class GetSchedulerDisplayPreferencesEndpoint : BaseEndpoint<FastEndpoints.EmptyRequest, IResult>
{
    private readonly IQueryHandler<GetSchedulerDisplayPreferencesQuery, Result<SchedulerDisplayPreferencesResponse>> _getSchedulerDisplayPreferencesQueryHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetSchedulerDisplayPreferencesEndpoint"/> class.
    /// </summary>
    /// <param name="getSchedulerDisplayPreferencesQueryHandler">Injected service for handling get scheduler display preferences queries.</param>
    public GetSchedulerDisplayPreferencesEndpoint(IQueryHandler<GetSchedulerDisplayPreferencesQuery, Result<SchedulerDisplayPreferencesResponse>> getSchedulerDisplayPreferencesQueryHandler)
    {
        _getSchedulerDisplayPreferencesQueryHandler = getSchedulerDisplayPreferencesQueryHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(FastEndpoints.Http.GET);
        Routes(ApiRoutes.ScheduledJobs.GET_DISPLAY_PREFERENCES);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Gets the display preferences of the scheduler page of the current user.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(FastEndpoints.EmptyRequest request, CancellationToken cancellationToken)
    {
        Result<SchedulerDisplayPreferencesResponse> result = await _getSchedulerDisplayPreferencesQueryHandler.HandleAsync(new GetSchedulerDisplayPreferencesQuery(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
