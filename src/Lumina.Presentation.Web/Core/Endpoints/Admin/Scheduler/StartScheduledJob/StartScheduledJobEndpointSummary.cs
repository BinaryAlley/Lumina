#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.DTO.Common;
using Lumina.Presentation.Web.Common.Requests.Scheduling;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Admin.Scheduler.StartScheduledJob;

/// <summary>
/// Class used for providing a textual description for the <see cref="StartScheduledJobEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class StartScheduledJobEndpointSummary : Summary<StartScheduledJobEndpoint, StartScheduledJobRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StartScheduledJobEndpointSummary"/> class.
    /// </summary>
    public StartScheduledJobEndpointSummary()
    {
        Summary = "Starts the execution cycle of a scheduled job.";
        Description = "Starts the execution cycle of the scheduled job identified by the request, which runs its task once immediately and then on its schedule.";

        RequestParam(r => r.ScheduledJobId, "The unique identifier of the scheduled job whose execution cycle is started. Required.");

        ExampleRequest = new StartScheduledJobRequest(ScheduledJobId: Guid.NewGuid());

        Response(200, "The execution cycle of the scheduled job was successfully started.", example: new SuccessResponse(true));
    }
}
