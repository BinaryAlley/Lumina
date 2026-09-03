#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Responses.Scheduling;
using Lumina.Domain.SharedKernel.Common.Enums.Scheduling;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Scheduling.GetScheduledJobs;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetScheduledJobsEndpoint"/> API endpoint, for OpenAPI.
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
        Description = "Gets the list of scheduled jobs and their status. Only administrators can get the list of scheduled jobs.";

        Response(200, "The list of scheduled jobs was successfully retrieved.",
            example: new[]
            {
                new ScheduledJobResponse(
                    Id: Guid.NewGuid(),
                    Name: "Rescan media libraries",
                    TaskType: ScheduledTaskType.ScanMediaLibraries,
                    ScheduleType: ScheduleType.DailyAtHourAndMinute,
                    IntervalMinutes: null,
                    Hour: 3,
                    Minute: 0,
                    Status: ScheduledJobStatus.Active,
                    LastStartedOnUtc: DateTime.UtcNow.AddHours(-2),
                    LastCompletedOnUtc: DateTime.UtcNow.AddHours(-2).AddMinutes(5)
                )
            });

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
    }
}
