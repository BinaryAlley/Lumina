#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.DTO.Common;
using Lumina.Presentation.Web.Common.DTO.Scheduling;
using Lumina.Presentation.Web.Common.Requests.Scheduling;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Admin.Scheduler.GetScheduledJobHistory;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetScheduledJobHistoryEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetScheduledJobHistoryEndpointSummary : Summary<GetScheduledJobHistoryEndpoint, GetScheduledJobHistoryRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetScheduledJobHistoryEndpointSummary"/> class.
    /// </summary>
    public GetScheduledJobHistoryEndpointSummary()
    {
        Summary = "Gets the history of the executions of the tasks of scheduled jobs.";
        Description = "Retrieves the executions of the tasks of scheduled jobs that started in the requested time interval.";

        RequestParam(r => r.From, "The inclusive lower bound of the interval for which the history is requested. Optional.");
        RequestParam(r => r.To, "The inclusive upper bound of the interval for which the history is requested. Optional.");

        ExampleRequest = new GetScheduledJobHistoryRequest(From: DateTime.UtcNow.AddDays(-1), To: DateTime.UtcNow);

        Response(200, "The execution history of the tasks of the scheduled jobs is returned.", example: new SuccessResponse<ScheduledJobExecutionDto[]>(true, default));
    }
}
