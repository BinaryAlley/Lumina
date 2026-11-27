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

namespace Nemera.Presentation.Web.Core.Endpoints.Admin.Scheduler.StartScheduledJob;

/// <summary>
/// API endpoint for the <c>/{culture}/admin/api-scheduled-jobs/{scheduledJobId}/start</c> route.
/// </summary>
public class StartScheduledJobEndpoint : BaseEndpoint<StartScheduledJobRequest, IResult>
{
    private readonly IApiHttpClient _apiHttpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="StartScheduledJobEndpoint"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected service for interactions with the API.</param>
    public StartScheduledJobEndpoint(IApiHttpClient apiHttpClient)
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
        Routes(WebRoutes.Scheduler.START_SCHEDULED_JOB);
        DontAutoTag();
        Options(options => options.WithTags("Admin"));
        Policies(AuthorizationPolicies.REQUIRE_ADMIN_ROLE);
        EnableAntiforgery();
    }

    /// <summary>
    /// Starts the execution cycle of a scheduled job.
    /// </summary>
    /// <param name="request">The request object.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(StartScheduledJobRequest request, CancellationToken cancellationToken)
    {
        await _apiHttpClient.PutIgnoringResponseAsync<StartScheduledJobRequest>(ApiRoutes.ScheduledJobs.START_SCHEDULED_JOB.Replace("{scheduledJobId}", request.ScheduledJobId.ToString()), request, cancellationToken).ConfigureAwait(false);
        return JsonSuccess();
    }
}
