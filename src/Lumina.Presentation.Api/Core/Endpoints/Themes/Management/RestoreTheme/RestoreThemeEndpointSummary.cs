#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Requests.Themes;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Themes.Management.RestoreTheme;

/// <summary>
/// Class used for providing a textual description for the <see cref="RestoreThemeEndpoint"/> API endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class RestoreThemeEndpointSummary : Summary<RestoreThemeEndpoint, RestoreThemeRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RestoreThemeEndpointSummary"/> class.
    /// </summary>
    public RestoreThemeEndpointSummary()
    {
        Summary = "Restores a deleted bundled theme.";
        Description = "Restores the files and reactivates a bundled theme that was soft deleted by the user, restoring the pack from the shipped archive.";

        ExampleRequest = new RestoreThemeRequest(
            ThemeId: "editorial-paper"
        );

        RequestParam(r => r.ThemeId, "The manifest id of the bundled theme to restore. Required.");

        Response(204, "The theme was successfully restored.");

        Response(401, "Authentication required.", "application/problem+json",
            example: new[]
            {
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "You are not authorized",
                    instance = "/api/v1/themes/{themeId}/restore"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "Invalid token: The token expired at '01/01/2024 01:00:00'",
                    instance = "/api/v1/themes/{themeId}/restore"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "The token is invalid",
                    instance = "/api/v1/themes/{themeId}/restore"
                }
            }
        );

        Response(403, "The request failed because the user is not an Admin or the theme is not a soft deleted bundled theme.", "application/problem+json",
            example: new[]
            {
                new
                {
                    type = "https://tools.ietf.org/html/rfc9110#section-15.5.4",
                    title = "General.Failure",
                    status = 403,
                    detail = "NotAuthorized",
                    instance = "/api/v1/themes/{themeId}/restore",
                    traceId = "00-a712bbf99ca8ab485f86a762ae5ae74d-b3a2eb78813b0a5d-00"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc9110#section-15.5.4",
                    title = "General.Failure",
                    status = 403,
                    detail = "ThemeCannotBeRestored",
                    instance = "/api/v1/themes/{themeId}/restore",
                    traceId = "00-a712bbf99ca8ab485f86a762ae5ae74d-b3a2eb78813b0a5d-00"
                }
            }
        );

        Response(404, "The request failed because the provided theme does not exist.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                title = "General.NotFound",
                status = 404,
                detail = "ThemeNotFound",
                instance = "/api/v1/themes/{themeId}/restore",
                traceId = "00-57d15dadd702dbd4aeb5dc9b7cee68ee-9330237dbb2ce0e5-00"
            }
        );

        Response(422, "The request did not pass validation checks.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc4918#section-11.2",
                title = "General.Validation",
                status = 422,
                detail = "OneOrMoreValidationErrorsOccurred",
                instance = "/api/v1/themes/{themeId}/restore",
                errors = new Dictionary<string, string[]>
                {
                    {
                        "General.Validation", new[]
                        {
                            "ThemeIdCannotBeEmpty"
                        }
                    }
                },
                traceId = "00-2470be4248a2a5a0c6f70579975a6954-b9c3ba9544a03500-00"
            }
        );
    }
}
