#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Requests.Scheduling;
using Lumina.Contracts.Responses.Scheduling;
using Lumina.Domain.SharedKernel.Common.Enums.Scheduling;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Scheduling.GetScheduledJobHistory;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetScheduledJobHistoryEndpoint"/> API endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetScheduledJobHistoryEndpointSummary : Summary<GetScheduledJobHistoryEndpoint, GetScheduledJobHistoryRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetScheduledJobHistoryEndpointSummary"/> class.
    /// </summary>
    public GetScheduledJobHistoryEndpointSummary()
    {
        Summary = "Gets the history of the executions of the tasks of scheduled jobs.";
        Description = "Gets the executions of the tasks of scheduled jobs that started in the requested time interval. Only administrators can get the history of scheduled jobs.";

        ExampleRequest = new GetScheduledJobHistoryRequest(From: DateTime.UtcNow.AddDays(-1), To: DateTime.UtcNow);

        RequestParam(request => request.From, "The inclusive lower bound of the interval for which the history is requested. Optional.");
        RequestParam(request => request.To, "The inclusive upper bound of the interval for which the history is requested. Optional.");

        Response(200, "The execution history was successfully retrieved.",
            example: new[]
            {
                new ScheduledJobExecutionResponse(
                    Id: Guid.NewGuid(),
                    ScheduledJobId: Guid.NewGuid(),
                    TaskType: ScheduledTaskType.CleanTemporaryFiles,
                    IsCycleRun: true,
                    StartedOnUtc: DateTime.UtcNow.AddMinutes(-10),
                    CompletedOnUtc: DateTime.UtcNow.AddMinutes(-9)
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
                    instance = "/api/v1/scheduled-jobs/history"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "Invalid token: The token expired at '01/01/2024 01:00:00'",
                    instance = "/api/v1/scheduled-jobs/history"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "The token is invalid",
                    instance = "/api/v1/scheduled-jobs/history"
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
                instance = "/api/v1/scheduled-jobs/history",
                traceId = "00-a712bbf99ca8ab485f86a762ae5ae74d-b3a2eb78813b0a5d-00"
            }
        );
    }
}
