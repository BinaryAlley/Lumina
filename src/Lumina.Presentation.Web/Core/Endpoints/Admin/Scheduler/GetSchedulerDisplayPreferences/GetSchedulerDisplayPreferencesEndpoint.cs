#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.Authorization;
using Lumina.Presentation.Web.Common.DTO.Scheduling;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Admin.Scheduler.GetSchedulerDisplayPreferences;

/// <summary>
/// API endpoint for the <c>/{culture}/admin/api-scheduled-jobs/display-preferences</c> route.
/// </summary>
public class GetSchedulerDisplayPreferencesEndpoint : BaseEndpoint<EmptyRequest, IResult>
{
    private readonly IApiHttpClient _apiHttpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetSchedulerDisplayPreferencesEndpoint"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected service for interactions with the API.</param>
    public GetSchedulerDisplayPreferencesEndpoint(IApiHttpClient apiHttpClient)
    {
        _apiHttpClient = apiHttpClient;
    }

    /// <summary>
    /// Configures the endpoint.
    /// </summary>
    public override void Configure()
    {
        base.Configure();
        Verbs(Http.GET);
        Routes(WebRoutes.Scheduler.GET_DISPLAY_PREFERENCES);
        DontAutoTag();
        Options(options => options.WithTags("Admin"));
        Policies(AuthorizationPolicies.REQUIRE_ADMIN_ROLE);
    }

    /// <summary>
    /// Gets the display preferences of the scheduler page of the current user.
    /// </summary>
    /// <param name="request">The request object.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(EmptyRequest request, CancellationToken cancellationToken)
    {
        SchedulerDisplayPreferencesDto displayPreferences = await _apiHttpClient.GetAsync<SchedulerDisplayPreferencesDto>(ApiRoutes.ScheduledJobs.GET_DISPLAY_PREFERENCES, cancellationToken).ConfigureAwait(false);
        return JsonSuccess(displayPreferences);
    }
}
