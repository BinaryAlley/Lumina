#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Requests.Themes;
using Lumina.Contracts.Responses.Themes;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Themes.Queries.GetThemeAsset;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetThemeAssetEndpoint"/> API endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetThemeAssetEndpointSummary : Summary<GetThemeAssetEndpoint, GetThemeAssetRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetThemeAssetEndpointSummary"/> class.
    /// </summary>
    public GetThemeAssetEndpointSummary()
    {
        Summary = "Retrieves an asset file of a theme.";
        Description = "Retrieves the binary content of an asset file of a theme, with its MIME content type.";

        ExampleRequest = new GetThemeAssetRequest(
            ThemeId: "lumina-default",
            AssetPath: "assets/style.css"
        );

        RequestParam(r => r.ThemeId, "The manifest id of the theme. Required.");
        RequestParam(r => r.AssetPath, "The asset path relative to the theme pack root. Required.");

        Response(200, "The asset file of the theme is returned.", "application/octet-stream");

        Response(422, "The request did not pass validation checks.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc4918#section-11.2",
                title = "General.Validation",
                status = 422,
                detail = "OneOrMoreValidationErrorsOccurred",
                instance = "/api/v1/themes/{themeId}/assets/{assetPath}",
                errors = new Dictionary<string, string[]>
                {
                    {
                        "General.Validation", new[]
                        {
                            "ThemeIdCannotBeEmpty",
                            "ThemeAssetPathCannotBeEmpty"
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
                detail = "ThemeFilesUnreadable",
                instance = "/api/v1/themes/{themeId}/assets/{assetPath}",
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
                instance = "/api/v1/themes/{themeId}/assets/{assetPath}",
                traceId = "00-57d15dadd702dbd4aeb5dc9b7cee68ee-9330237dbb2ce0e5-00"
            }
        );
    }
}
