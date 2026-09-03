#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Requests.Scheduling;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Scheduling.RemoveScheduledJob;

/// <summary>
/// Class used for providing a textual description for the <see cref="RemoveScheduledJobEndpoint"/> API endpoint, for OpenAPI.
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
        Description = "Removes a scheduled job, after stopping its execution cycle, if it has one. Only administrators can remove scheduled jobs.";

        ExampleRequest = new RemoveScheduledJobRequest(ScheduledJobId: Guid.NewGuid());

        RequestParam(request => request.ScheduledJobId, "The unique identifier of the scheduled job to remove. Required.");

        Response(200, "The scheduled job was successfully removed.");

        Response(401, "Authentication required.", "application/problem+json",
            example: new[]
            {
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "You are not authorized",
                    instance = "/api/v1/scheduled-jobs/{scheduledJobId}"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "Invalid token: The token expired at '01/01/2024 01:00:00'",
                    instance = "/api/v1/scheduled-jobs/{scheduledJobId}"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "The token is invalid",
                    instance = "/api/v1/scheduled-jobs/{scheduledJobId}"
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
                instance = "/api/v1/scheduled-jobs/{scheduledJobId}",
                traceId = "00-a712bbf99ca8ab485f86a762ae5ae74d-b3a2eb78813b0a5d-00"
            }
        );

        Response(404, "The request failed because the provided scheduled job does not exist.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                title = "General.NotFound",
                status = 404,
                detail = "ScheduledJobNotFound",
                instance = "/api/v1/scheduled-jobs/{scheduledJobId}",
                traceId = "00-57d15dadd702dbd4aeb5dc9b7cee68ee-9330237dbb2ce0e5-00"
            }
        );
    }
}
