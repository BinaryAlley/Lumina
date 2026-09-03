#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.DTO.Common;
using Lumina.Presentation.Web.Common.Enums.Scheduling;
using Lumina.Presentation.Web.Common.Requests.Scheduling;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Admin.Scheduler.UpdateSchedulerDisplayPreferences;

/// <summary>
/// Class used for providing a textual description for the <see cref="UpdateSchedulerDisplayPreferencesEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateSchedulerDisplayPreferencesEndpointSummary : Summary<UpdateSchedulerDisplayPreferencesEndpoint, UpdateSchedulerDisplayPreferencesRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateSchedulerDisplayPreferencesEndpointSummary"/> class.
    /// </summary>
    public UpdateSchedulerDisplayPreferencesEndpointSummary()
    {
        Summary = "Updates the display preferences of the scheduler page.";
        Description = "Stores the display preferences of the scheduler page of the current user, so that they are restored on every client the user signs in from.";

        RequestParam(r => r.JobTypeFilter, "The type of the scheduled job tasks whose executions are shown on the scheduler page, or null when all of them are shown.");
        RequestParam(r => r.DisplayTimeSpan, "The time span, expressed in the display time unit, that the scheduler page shows. Required.");
        RequestParam(r => r.DisplayTimeUnit, "The unit in which the displayed time span of the scheduler page is expressed. Required.");

        ExampleRequest = new UpdateSchedulerDisplayPreferencesRequest(
            JobTypeFilter: ScheduledTaskType.ScanMediaLibraries,
            DisplayTimeSpan: 10,
            DisplayTimeUnit: SchedulerDisplayTimeUnit.Minutes
        );

        Response(200, "The display preferences of the scheduler page were successfully updated.", example: new SuccessResponse(true));
    }
}
