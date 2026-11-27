#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.DTO.Common;
using Lumina.Presentation.Web.Common.Requests.Scheduling;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Admin.Scheduler.StopScheduledJob;

/// <summary>
/// Class used for providing a textual description for the <see cref="StopScheduledJobEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class StopScheduledJobEndpointSummary : Summary<StopScheduledJobEndpoint, StopScheduledJobRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StopScheduledJobEndpointSummary"/> class.
    /// </summary>
    public StopScheduledJobEndpointSummary()
    {
        Summary = "Stops the execution cycle of a scheduled job.";
        Description = "Stops the execution cycle of the scheduled job identified by the request, without removing it.";

        RequestParam(r => r.ScheduledJobId, "The unique identifier of the scheduled job whose execution cycle is stopped. Required.");

        ExampleRequest = new StopScheduledJobRequest(ScheduledJobId: Guid.NewGuid());

        Response(200, "The execution cycle of the scheduled job was successfully stopped.", example: new SuccessResponse(true));
    }
}
