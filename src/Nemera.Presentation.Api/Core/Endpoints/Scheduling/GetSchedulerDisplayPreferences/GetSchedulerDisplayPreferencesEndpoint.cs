#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.CQRS;
using Nemera.Application.Core.Scheduling.Queries.GetSchedulerDisplayPreferences;
using Nemera.Contracts.Responses.Scheduling;
using Nemera.Domain.Common.Primitives;
using Nemera.Presentation.Api.Common.Routes.Scheduling;
using Nemera.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Nemera.Presentation.Api.Core.Endpoints.Scheduling.GetSchedulerDisplayPreferences;

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
    /// <param name="request">The empty request, which carries no data.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(FastEndpoints.EmptyRequest request, CancellationToken cancellationToken)
    {
        Result<SchedulerDisplayPreferencesResponse> result = await _getSchedulerDisplayPreferencesQueryHandler.HandleAsync(new GetSchedulerDisplayPreferencesQuery(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
