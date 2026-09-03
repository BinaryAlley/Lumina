#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.Authorization;
using Lumina.Presentation.Web.Common.DTO.Scheduling;
using Lumina.Presentation.Web.Common.Requests.Scheduling;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Admin.Scheduler.GetScheduledJobHistory;

/// <summary>
/// API endpoint for the <c>/{culture}/admin/api-scheduled-jobs/history</c> route.
/// </summary>
public class GetScheduledJobHistoryEndpoint : BaseEndpoint<GetScheduledJobHistoryRequest, IResult>
{
    private readonly IApiHttpClient _apiHttpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetScheduledJobHistoryEndpoint"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected service for interactions with the API.</param>
    public GetScheduledJobHistoryEndpoint(IApiHttpClient apiHttpClient)
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
        Routes(WebRoutes.Scheduler.GET_SCHEDULED_JOB_HISTORY);
        DontAutoTag();
        Options(options => options.WithTags("Admin"));
        Policies(AuthorizationPolicies.REQUIRE_ADMIN_ROLE);
    }

    /// <summary>
    /// Gets the history of the executions of the tasks of scheduled jobs.
    /// </summary>
    /// <param name="request">The request object.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(GetScheduledJobHistoryRequest request, CancellationToken cancellationToken)
    {
        ScheduledJobExecutionDto[] executions = await _apiHttpClient.GetAsync<ScheduledJobExecutionDto[]>(BuildHistoryEndpoint(request), cancellationToken).ConfigureAwait(false);
        return JsonSuccess(executions);
    }

    /// <summary>
    /// Builds the API endpoint used to get the history of the executions of the tasks of scheduled jobs, from the provided <paramref name="request"/>.
    /// </summary>
    /// <param name="request">The request object.</param>
    /// <returns>The API endpoint to which the history request is sent.</returns>
    private static string BuildHistoryEndpoint(GetScheduledJobHistoryRequest request)
    {
        StringBuilder endpoint = new(ApiRoutes.ScheduledJobs.GET_SCHEDULED_JOB_HISTORY);
        if (request.From is not null || request.To is not null)
        {
            endpoint.Append('?');
            if (request.From is not null)
                endpoint.Append($"from={Uri.EscapeDataString(request.From.Value.ToString("o"))}");
            if (request.To is not null)
            {
                if (request.From is not null)
                    endpoint.Append('&');
                endpoint.Append($"to={Uri.EscapeDataString(request.To.Value.ToString("o"))}");
            }
        }
        return endpoint.ToString();
    }
}
