#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.DTO.Common;
using Lumina.Presentation.Web.Common.Requests.Scheduling;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Admin.Scheduler.FireScheduledJob;

/// <summary>
/// Class used for providing a textual description for the <see cref="FireScheduledJobEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class FireScheduledJobEndpointSummary : Summary<FireScheduledJobEndpoint, FireScheduledJobRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FireScheduledJobEndpointSummary"/> class.
    /// </summary>
    public FireScheduledJobEndpointSummary()
    {
        Summary = "Fires the task of a scheduled job.";
        Description = "Runs the task of the scheduled job identified by the request once immediately, without affecting its execution cycle.";

        RequestParam(r => r.ScheduledJobId, "The unique identifier of the scheduled job whose task is fired. Required.");

        ExampleRequest = new FireScheduledJobRequest(ScheduledJobId: Guid.NewGuid());

        Response(200, "The task of the scheduled job was successfully fired.", example: new SuccessResponse(true));
    }
}
