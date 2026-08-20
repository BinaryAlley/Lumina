#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Requests.Themes;
using Lumina.Contracts.Responses.Themes;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Themes.Queries.GetThemeArchive;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetThemeArchiveEndpoint"/> API endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetThemeArchiveEndpointSummary : Summary<GetThemeArchiveEndpoint, GetThemeArchiveRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetThemeArchiveEndpointSummary"/> class.
    /// </summary>
    public GetThemeArchiveEndpointSummary()
    {
        Summary = "Retrieves the archive of a theme.";
        Description = "Retrieves the downloadable ZIP archive of an installed theme.";

        ExampleRequest = new GetThemeArchiveRequest(
            ThemeId: "lumina-default"
        );

        RequestParam(r => r.ThemeId, "The manifest id of the theme. Required.");

        Response(200, "The ZIP archive of the theme is returned.", "application/zip",
            example: new ThemeArchiveResponse(
                Bytes: [],
                FileName: "lumina-default.zip",
                ContentType: "application/zip"
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
                    instance = "/api/v1/themes/{themeId}/archive"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "Invalid token: The token expired at '01/01/2024 01:00:00'",
                    instance = "/api/v1/themes/{themeId}/archive"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "The token is invalid",
                    instance = "/api/v1/themes/{themeId}/archive"
                }
            }
        );

        Response(422, "The request did not pass validation checks.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc4918#section-11.2",
                title = "General.Validation",
                status = 422,
                detail = "OneOrMoreValidationErrorsOccurred",
                instance = "/api/v1/themes/{themeId}/archive",
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

        Response(404, "The request failed because the provided theme does not exist.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                title = "General.NotFound",
                status = 404,
                detail = "ThemeNotFound",
                instance = "/api/v1/themes/{themeId}/archive",
                traceId = "00-57d15dadd702dbd4aeb5dc9b7cee68ee-9330237dbb2ce0e5-00"
            }
        );
    }
}
