#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Requests.Themes;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Themes.ThemeAssets;

/// <summary>
/// Class used for providing a textual description for the <see cref="ThemeAssetsEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class ThemeAssetsEndpointSummary : Summary<ThemeAssetsEndpoint, GetThemeAssetRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ThemeAssetsEndpointSummary"/> class.
    /// </summary>
    public ThemeAssetsEndpointSummary()
    {
        Summary = "Retrieves an asset file of a theme.";
        Description = "Serves the binary content of an asset file of a theme, fetched from the remote API.";

        RequestParam(r => r.ThemeId, "The manifest id of the theme. Required.");
        RequestParam(r => r.Path, "The asset path relative to the theme pack root. Required.");

        ExampleRequest = new GetThemeAssetRequest(
            ThemeId: "lumina-default",
            Path: "assets/style.css"
        );

        Response(200, "The asset file of the theme is returned.", "application/octet-stream");
    }
}
