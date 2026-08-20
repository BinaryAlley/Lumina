#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Requests.Themes;
using Lumina.Contracts.Responses.Themes;
using Lumina.Domain.SharedKernel.Common.Enums.Themes;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Themes.Queries.GetThemeTemplate;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetThemeTemplateEndpoint"/> API endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetThemeTemplateEndpointSummary : Summary<GetThemeTemplateEndpoint, GetThemeTemplateRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetThemeTemplateEndpointSummary"/> class.
    /// </summary>
    public GetThemeTemplateEndpointSummary()
    {
        Summary = "Retrieves the template of a theme.";
        Description = "Retrieves the raw template of a theme selected by a page key, after applying the theme engine sanitization.";

        ExampleRequest = new GetThemeTemplateRequest(
            ThemeId: "lumina-default",
            PageKey: "index"
        );

        RequestParam(r => r.ThemeId, "The manifest id of the theme. Required.");
        RequestParam(r => r.PageKey, "The page key that selects the template. Required.");

        Response(200, "The template of the theme is returned.",
            example: new ThemeTemplateResponse(
                new ThemeResponse(
                    Id: Guid.NewGuid(),
                    ThemeId: "lumina-default",
                    Name: "Lumina Default",
                    Description: "A clean, readable theme.",
                    Author: "Lumina Team",
                    Version: "1.2.0",
                    PreviewPath: "preview.png",
                    InstallSource: ThemeInstallSource.Bundled,
                    IsCurrent: true,
                    InstalledAtUtc: DateTime.UtcNow
                ),
                "<html><body><div class=\"content\">{{content}}</div></body></html>"
            ));

        Response(422, "The request did not pass validation checks.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc4918#section-11.2",
                title = "General.Validation",
                status = 422,
                detail = "OneOrMoreValidationErrorsOccurred",
                instance = "/api/v1/themes/{themeId}/templates/{pageKey}",
                errors = new Dictionary<string, string[]>
                {
                    {
                        "General.Validation", new[]
                        {
                            "ThemeIdCannotBeEmpty",
                            "PageKeyCannotBeEmpty"
                        }
                    }
                },
                traceId = "00-2470be4248a2a5a0c6f70579975a6954-b9c3ba9544a03500-00"
            }
        );

        Response(403, "The request failed because the stored files of the theme are not readable.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc9110#section-15.5.4",
                title = "General.Failure",
                status = 403,
                detail = "ThemeNotAvailable",
                instance = "/api/v1/themes/{themeId}/templates/{pageKey}",
                traceId = "00-a712bbf99ca8ab485f86a762ae5ae74d-b3a2eb78813b0a5d-00"
            }
        );

        Response(404, "The request failed because the provided theme does not exist.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                title = "General.NotFound",
                status = 404,
                detail = "ThemeNotFound",
                instance = "/api/v1/themes/{themeId}/templates/{pageKey}",
                traceId = "00-57d15dadd702dbd4aeb5dc9b7cee68ee-9330237dbb2ce0e5-00"
            }
        );
    }
}
