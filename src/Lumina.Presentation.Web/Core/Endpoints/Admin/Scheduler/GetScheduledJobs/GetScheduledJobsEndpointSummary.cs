#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.DTO.Common;
using Lumina.Presentation.Web.Common.DTO.Scheduling;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Admin.Scheduler.GetScheduledJobs;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetScheduledJobsEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetScheduledJobsEndpointSummary : Summary<GetScheduledJobsEndpoint, EmptyRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetScheduledJobsEndpointSummary"/> class.
    /// </summary>
    public GetScheduledJobsEndpointSummary()
    {
        Summary = "Gets the list of scheduled jobs.";
        Description = "Retrieves the list of scheduled jobs and their status.";

        Response(200, "The list of scheduled jobs is returned.", example: new SuccessResponse<ScheduledJobDto[]>(true, default));
    }
}
