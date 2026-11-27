#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Nemera.Presentation.Web.Common.Api;
using Nemera.Presentation.Web.Common.Authorization;
using Nemera.Presentation.Web.Common.DTO.Scheduling;
using Nemera.Presentation.Web.Common.Requests.Scheduling;
using Nemera.Presentation.Web.Common.Routes;
using Nemera.Presentation.Web.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Nemera.Presentation.Web.Core.Endpoints.Admin.Scheduler.AddScheduledJob;

/// <summary>
/// API endpoint for the <c>/{culture}/admin/api-scheduled-jobs/add</c> route.
/// </summary>
public class AddScheduledJobEndpoint : BaseEndpoint<AddScheduledJobRequest, IResult>
{
    private readonly IApiHttpClient _apiHttpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="AddScheduledJobEndpoint"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected service for interactions with the API.</param>
    public AddScheduledJobEndpoint(IApiHttpClient apiHttpClient)
    {
        _apiHttpClient = apiHttpClient;
    }

    /// <summary>
    /// Configures the endpoint.
    /// </summary>
    public override void Configure()
    {
        base.Configure();
        Verbs(Http.POST);
        Routes(WebRoutes.Scheduler.ADD_SCHEDULED_JOB);
        DontAutoTag();
        Options(options => options.WithTags("Admin"));
        Policies(AuthorizationPolicies.REQUIRE_ADMIN_ROLE);
        EnableAntiforgery();
    }

    /// <summary>
    /// Adds a scheduled job.
    /// </summary>
    /// <param name="request">The request object.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(AddScheduledJobRequest request, CancellationToken cancellationToken)
    {
        ScheduledJobDto scheduledJob = await _apiHttpClient.PostAsync<ScheduledJobDto, AddScheduledJobRequest>(ApiRoutes.ScheduledJobs.ADD_SCHEDULED_JOB, request, cancellationToken).ConfigureAwait(false);
        return JsonSuccess(scheduledJob);
    }
}
