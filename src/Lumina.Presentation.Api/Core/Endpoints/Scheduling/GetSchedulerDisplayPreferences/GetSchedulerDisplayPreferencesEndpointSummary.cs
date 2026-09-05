#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Responses.Scheduling;
using Lumina.Domain.SharedKernel.Common.Enums.Scheduling;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Scheduling.GetSchedulerDisplayPreferences;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetSchedulerDisplayPreferencesEndpoint"/> API endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetSchedulerDisplayPreferencesEndpointSummary : Summary<GetSchedulerDisplayPreferencesEndpoint, EmptyRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetSchedulerDisplayPreferencesEndpointSummary"/> class.
    /// </summary>
    public GetSchedulerDisplayPreferencesEndpointSummary()
    {
        Summary = "Gets the display preferences of the scheduler page of the current user.";
        Description = "Gets the display preferences of the scheduler page of the current user. When no display preferences are stored for the user yet, the default display preferences are returned. Only administrators can use the scheduler page.";

        Response(200, "The display preferences of the scheduler page of the current user are returned.",
            example: new SchedulerDisplayPreferencesResponse(
                UserId: Guid.NewGuid(),
                JobTypeFilter: ScheduledTaskType.ScanMediaLibraries,
                DisplayTimeSpan: 10,
                DisplayTimeUnit: SchedulerDisplayTimeUnit.Minutes
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
    }
}
