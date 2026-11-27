#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.DTO.Common;
using Lumina.Presentation.Web.Common.DTO.Scheduling;
using Lumina.Presentation.Web.Common.Enums.Scheduling;
using Lumina.Presentation.Web.Common.Requests.Scheduling;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Admin.Scheduler.AddScheduledJob;

/// <summary>
/// Class used for providing a textual description for the <see cref="AddScheduledJobEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddScheduledJobEndpointSummary : Summary<AddScheduledJobEndpoint, AddScheduledJobRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddScheduledJobEndpointSummary"/> class.
    /// </summary>
    public AddScheduledJobEndpointSummary()
    {
        Summary = "Adds a scheduled job.";
        Description = "Adds a scheduled job that can run its task periodically or once.";

        RequestParam(r => r.Name, "The name of the scheduled job. Required.");
        RequestParam(r => r.TaskType, "The type of the task executed by the scheduled job. Required.");
        RequestParam(r => r.ScheduleType, "The type of the schedule of the scheduled job. Required.");
        RequestParam(r => r.IntervalMinutes, "The interval in minutes of the schedule, when the schedule type is WithIntervalInMinutes.");
        RequestParam(r => r.Hour, "The hour of the schedule, when the schedule type is DailyAtHourAndMinute.");
        RequestParam(r => r.Minute, "The minute of the schedule, when the schedule type is DailyAtHourAndMinute.");

        ExampleRequest = new AddScheduledJobRequest(
            Name: "Rescan media libraries",
            TaskType: ScheduledTaskType.ScanMediaLibraries,
            ScheduleType: ScheduleType.DailyAtHourAndMinute,
            IntervalMinutes: null,
            Hour: 3,
            Minute: 0
        );

        Response(200, "The scheduled job was successfully added.", example: new SuccessResponse<ScheduledJobDto>(true, default));
    }
}
