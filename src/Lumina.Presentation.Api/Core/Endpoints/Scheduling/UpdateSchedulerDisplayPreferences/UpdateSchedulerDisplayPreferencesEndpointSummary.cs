#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Requests.Scheduling;
using Lumina.Domain.SharedKernel.Common.Enums.Scheduling;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Scheduling.UpdateSchedulerDisplayPreferences;

/// <summary>
/// Class used for providing a textual description for the <see cref="UpdateSchedulerDisplayPreferencesEndpoint"/> API endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateSchedulerDisplayPreferencesEndpointSummary : Summary<UpdateSchedulerDisplayPreferencesEndpoint, UpdateSchedulerDisplayPreferencesRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateSchedulerDisplayPreferencesEndpointSummary"/> class.
    /// </summary>
    public UpdateSchedulerDisplayPreferencesEndpointSummary()
    {
        Summary = "Updates the display preferences of the scheduler page of the current user.";
        Description = "Updates the display preferences of the scheduler page of the current user. Only administrators can use the scheduler page.";

        ExampleRequest = new UpdateSchedulerDisplayPreferencesRequest(
            JobTypeFilter: ScheduledTaskType.ScanMediaLibraries,
            DisplayTimeSpan: 10,
            DisplayTimeUnit: SchedulerDisplayTimeUnit.Minutes
        );

        RequestParam(request => request.JobTypeFilter, "The type of the scheduled job tasks whose executions are shown on the scheduler page, or null when all of them are shown.");
        RequestParam(request => request.DisplayTimeSpan, "The time span, expressed in the display time unit, that the scheduler page shows. Required.");
        RequestParam(request => request.DisplayTimeUnit, "The unit in which the displayed time span of the scheduler page is expressed. Required.");

        Response(200, "The display preferences of the scheduler page of the current user were successfully updated.");

        Response(401, "Authentication required.", "application/problem+json",
            example: new[]
            {
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "You are not authorized",
                    instance = "/api/v1/scheduled-jobs/display-preferences"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "Invalid token: The token expired at '01/01/2024 01:00:00'",
                    instance = "/api/v1/scheduled-jobs/display-preferences"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "The token is invalid",
                    instance = "/api/v1/scheduled-jobs/display-preferences"
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
                instance = "/api/v1/scheduled-jobs/display-preferences",
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
                instance = "/api/v1/scheduled-jobs/display-preferences",
                errors = new Dictionary<string, string[]>
                {
                    {
                        "General.Validation", new[]
                        {
                            "SchedulerDisplayTimeSpanMustBePositive"
                        }
                    }
                },
                traceId = "00-2470be4248a2a5a0c6f70579975a6954-b9c3ba9544a03500-00"
            }
        );
    }
}
