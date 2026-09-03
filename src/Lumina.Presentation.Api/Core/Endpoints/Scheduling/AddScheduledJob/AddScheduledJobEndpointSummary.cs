#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Requests.Scheduling;
using Lumina.Contracts.Responses.Scheduling;
using Lumina.Domain.SharedKernel.Common.Enums.Scheduling;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Scheduling.AddScheduledJob;

/// <summary>
/// Class used for providing a textual description for the <see cref="AddScheduledJobEndpoint"/> API endpoint, for OpenAPI.
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
        Description = "Adds a scheduled job that can run its task periodically or once. Only administrators can add scheduled jobs.";

        ExampleRequest = new AddScheduledJobRequest(
            Name: "Rescan media libraries",
            TaskType: ScheduledTaskType.ScanMediaLibraries,
            ScheduleType: ScheduleType.DailyAtHourAndMinute,
            IntervalMinutes: null,
            Hour: 3,
            Minute: 0
        );

        RequestParam(request => request.Name, "The name of the scheduled job. Required.");
        RequestParam(request => request.TaskType, "The type of the task executed by the scheduled job. Required.");
        RequestParam(request => request.ScheduleType, "The type of the schedule of the scheduled job. Required.");
        RequestParam(request => request.IntervalMinutes, "The interval in minutes of the schedule, required when the schedule type is WithIntervalInMinutes.");
        RequestParam(request => request.Hour, "The hour of the schedule, required when the schedule type is DailyAtHourAndMinute.");
        RequestParam(request => request.Minute, "The minute of the schedule, required when the schedule type is DailyAtHourAndMinute.");

        Response(200, "The scheduled job was successfully added.",
            example: new ScheduledJobResponse(
                Id: Guid.NewGuid(),
                Name: "Rescan media libraries",
                TaskType: ScheduledTaskType.ScanMediaLibraries,
                ScheduleType: ScheduleType.DailyAtHourAndMinute,
                IntervalMinutes: null,
                Hour: 3,
                Minute: 0,
                Status: ScheduledJobStatus.Added,
                LastStartedOnUtc: null,
                LastCompletedOnUtc: null
            ));

        Response(401, "Authentication required.", "application/problem+json",
            example: new[]
            {
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "You are not authorized",
                    instance = "/api/v1/scheduled-jobs"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "Invalid token: The token expired at '01/01/2024 01:00:00'",
                    instance = "/api/v1/scheduled-jobs"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "The token is invalid",
                    instance = "/api/v1/scheduled-jobs"
                }
            }
        );

        Response(403, "The request failed because the user making the request is not an administrator.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc9110#section-15.5.4",
                title = "General.Failure",
                status = 403,
                detail = "NotAuthorized",
                instance = "/api/v1/scheduled-jobs",
                traceId = "00-a712bbf99ca8ab485f86a762ae5ae74d-b3a2eb78813b0a5d-00"
            }
        );

        Response(422, "The request did not pass validation checks.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc4918#section-11.2",
                title = "General.Validation",
                status = 422,
                detail = "OneOrMoreValidationErrorsOccurred",
                instance = "/api/v1/scheduled-jobs",
                errors = new Dictionary<string, string[]>
                {
                    {
                        "General.Validation", new[]
                        {
                            "ScheduledJobNameCannotBeEmpty",
                            "IntervalMinutesMustBePositive",
                            "HourMustBeBetweenZeroAndTwentyThree",
                            "MinuteMustBeBetweenZeroAndFiftyNine",
                            "InvalidScheduleType"
                        }
                    }
                },
                traceId = "00-2470be4248a2a5a0c6f70579975a6954-b9c3ba9544a03500-00"
            }
        );
    }
}
