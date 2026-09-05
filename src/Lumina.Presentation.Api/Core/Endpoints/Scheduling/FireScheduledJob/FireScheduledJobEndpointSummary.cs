#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Requests.Scheduling;
using Lumina.Contracts.Responses.Scheduling;
using Lumina.Domain.SharedKernel.Common.Enums.Scheduling;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Scheduling.FireScheduledJob;

/// <summary>
/// Class used for providing a textual description for the <see cref="FireScheduledJobEndpoint"/> API endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class FireScheduledJobEndpointSummary : Summary<FireScheduledJobEndpoint, FireScheduledJobRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FireScheduledJobEndpointSummary"/> class.
    /// </summary>
    public FireScheduledJobEndpointSummary()
    {
        Summary = "Fires the task of a scheduled job once.";
        Description = "Runs the task of a scheduled job once immediately, without affecting its execution cycle. Only administrators can fire scheduled jobs.";

        ExampleRequest = new FireScheduledJobRequest(ScheduledJobId: Guid.NewGuid());

        RequestParam(request => request.ScheduledJobId, "The unique identifier of the scheduled job whose task is fired. Required.");

        Response(200, "The task of the scheduled job was successfully fired.",
            example: new ScheduledJobResponse(
                Id: Guid.NewGuid(),
                Name: "Clean temporary files",
                TaskType: ScheduledTaskType.CleanTemporaryFiles,
                ScheduleType: ScheduleType.WithIntervalInMinutes,
                IntervalMinutes: 1440,
                Hour: null,
                Minute: null,
                Status: ScheduledJobStatus.Completed,
                LastStartedOnUtc: DateTime.UtcNow.AddMinutes(-1),
                LastCompletedOnUtc: DateTime.UtcNow
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
                    instance = "/api/v1/scheduled-jobs/{scheduledJobId}/fire"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "Invalid token: The token expired at '01/01/2024 01:00:00'",
                    instance = "/api/v1/scheduled-jobs/{scheduledJobId}/fire"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "The token is invalid",
                    instance = "/api/v1/scheduled-jobs/{scheduledJobId}/fire"
                }
            }
        );

        Response(403, "The request failed because the user making the request is not an administrator, or because the task of the scheduled job is currently running.", "application/problem+json",
            example: new[]
            {
                new
                {
                    type = "https://tools.ietf.org/html/rfc9110#section-15.5.4",
                    title = "General.Failure",
                    status = 403,
                    detail = "NotAuthorized",
                    instance = "/api/v1/scheduled-jobs/{scheduledJobId}/fire",
                    traceId = "00-a712bbf99ca8ab485f86a762ae5ae74d-b3a2eb78813b0a5d-00"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc9110#section-15.5.4",
                    title = "General.Failure",
                    status = 403,
                    detail = "ScheduledJobAlreadyRunning",
                    instance = "/api/v1/scheduled-jobs/{scheduledJobId}/fire",
                    traceId = "00-a712bbf99ca8ab485f86a762ae5ae74d-b3a2eb78813b0a5d-00"
                }
            }
        );

        Response(404, "The request failed because the provided scheduled job does not exist.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                title = "General.NotFound",
                status = 404,
                detail = "ScheduledJobNotFound",
                instance = "/api/v1/scheduled-jobs/{scheduledJobId}/fire",
                traceId = "00-57d15dadd702dbd4aeb5dc9b7cee68ee-9330237dbb2ce0e5-00"
            }
        );
    }
}
