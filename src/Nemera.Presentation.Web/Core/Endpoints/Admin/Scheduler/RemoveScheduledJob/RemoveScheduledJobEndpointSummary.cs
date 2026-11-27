#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.DTO.Common;
using Lumina.Presentation.Web.Common.Requests.Scheduling;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Admin.Scheduler.RemoveScheduledJob;

/// <summary>
/// Class used for providing a textual description for the <see cref="RemoveScheduledJobEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class RemoveScheduledJobEndpointSummary : Summary<RemoveScheduledJobEndpoint, RemoveScheduledJobRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RemoveScheduledJobEndpointSummary"/> class.
    /// </summary>
    public RemoveScheduledJobEndpointSummary()
    {
        Summary = "Removes a scheduled job.";
        Description = "Removes the scheduled job identified by the request, after stopping its execution cycle, if it has one.";

        RequestParam(r => r.ScheduledJobId, "The unique identifier of the scheduled job to remove. Required.");

        ExampleRequest = new RemoveScheduledJobRequest(ScheduledJobId: Guid.NewGuid());

        Response(200, "The scheduled job was successfully removed.", example: new SuccessResponse(true));
    }
}
