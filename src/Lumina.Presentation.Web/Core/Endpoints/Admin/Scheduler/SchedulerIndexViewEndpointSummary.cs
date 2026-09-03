#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Admin.Scheduler;

/// <summary>
/// Class used for providing a textual description for the <see cref="SchedulerIndexViewEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class SchedulerIndexViewEndpointSummary : Summary<SchedulerIndexViewEndpoint, EmptyRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SchedulerIndexViewEndpointSummary"/> class.
    /// </summary>
    public SchedulerIndexViewEndpointSummary()
    {
        Summary = "Renders the scheduled jobs dashboard view.";
        Description = "Renders the view for managing the scheduled jobs and their execution cycle.";

        Response(200, "The view for managing the scheduled jobs and their execution cycle is rendered.");
    }
}
