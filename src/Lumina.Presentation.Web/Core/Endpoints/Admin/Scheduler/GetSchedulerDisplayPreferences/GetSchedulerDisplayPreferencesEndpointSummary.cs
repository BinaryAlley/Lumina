#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.DTO.Common;
using Lumina.Presentation.Web.Common.DTO.Scheduling;
using Lumina.Presentation.Web.Common.Enums.Scheduling;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Admin.Scheduler.GetSchedulerDisplayPreferences;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetSchedulerDisplayPreferencesEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetSchedulerDisplayPreferencesEndpointSummary : Summary<GetSchedulerDisplayPreferencesEndpoint, EmptyRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetSchedulerDisplayPreferencesEndpointSummary"/> class.
    /// </summary>
    public GetSchedulerDisplayPreferencesEndpointSummary()
    {
        Summary = "Gets the display preferences of the scheduler page.";
        Description = "Retrieves the display preferences of the scheduler page of the current user, or the default display preferences when none are stored yet.";

        Response(200, "The display preferences of the scheduler page are returned.", example: new SuccessResponse<SchedulerDisplayPreferencesDto>(true, new SchedulerDisplayPreferencesDto(
            UserId: Guid.NewGuid(),
            JobTypeFilter: ScheduledTaskType.ScanMediaLibraries,
            DisplayTimeSpan: 10,
            DisplayTimeUnit: SchedulerDisplayTimeUnit.Minutes
        )));
    }
}
