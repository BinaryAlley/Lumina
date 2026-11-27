#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Nemera.Presentation.Web.Common.Api;
using Nemera.Presentation.Web.Common.Authorization;
using Nemera.Presentation.Web.Common.Requests.Scheduling;
using Nemera.Presentation.Web.Common.Routes;
using Nemera.Presentation.Web.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Nemera.Presentation.Web.Core.Endpoints.Admin.Scheduler.UpdateSchedulerDisplayPreferences;

/// <summary>
/// API endpoint for the <c>/{culture}/admin/api-scheduled-jobs/display-preferences</c> route.
/// </summary>
public class UpdateSchedulerDisplayPreferencesEndpoint : BaseEndpoint<UpdateSchedulerDisplayPreferencesRequest, IResult>
{
    private readonly IApiHttpClient _apiHttpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateSchedulerDisplayPreferencesEndpoint"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected service for interactions with the API.</param>
    public UpdateSchedulerDisplayPreferencesEndpoint(IApiHttpClient apiHttpClient)
    {
        _apiHttpClient = apiHttpClient;
    }

    /// <summary>
    /// Configures the endpoint.
    /// </summary>
    public override void Configure()
    {
        base.Configure();
        Verbs(Http.PUT);
        Routes(WebRoutes.Scheduler.UPDATE_DISPLAY_PREFERENCES);
        DontAutoTag();
        Options(options => options.WithTags("Admin"));
        Policies(AuthorizationPolicies.REQUIRE_ADMIN_ROLE);
        EnableAntiforgery();
    }

    /// <summary>
    /// Updates the display preferences of the scheduler page of the current user.
    /// </summary>
    /// <param name="request">The request containing the display preferences of the scheduler page of the current user.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(UpdateSchedulerDisplayPreferencesRequest request, CancellationToken cancellationToken)
    {
        await _apiHttpClient.PutIgnoringResponseAsync<UpdateSchedulerDisplayPreferencesRequest>(ApiRoutes.ScheduledJobs.UPDATE_DISPLAY_PREFERENCES, request, cancellationToken).ConfigureAwait(false);
        return JsonSuccess();
    }
}
