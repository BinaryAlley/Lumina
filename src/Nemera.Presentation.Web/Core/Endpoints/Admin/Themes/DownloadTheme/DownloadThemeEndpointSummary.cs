#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Nemera.Presentation.Web.Common.Requests.Themes;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Nemera.Presentation.Web.Core.Endpoints.Admin.Themes.DownloadTheme;

/// <summary>
/// Class used for providing a textual description for the <see cref="DownloadThemeEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class DownloadThemeEndpointSummary : Summary<DownloadThemeEndpoint, GetThemeArchiveRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DownloadThemeEndpointSummary"/> class.
    /// </summary>
    public DownloadThemeEndpointSummary()
    {
        Summary = "Downloads the archive of a theme.";
        Description = "Downloads the ZIP archive of the installed theme identified by the request.";

        RequestParam(r => r.ThemeId, "The manifest id of the theme to download. Required.");

        ExampleRequest = new GetThemeArchiveRequest(
            ThemeId: "nemera-default"
        );

        Response(200, "The ZIP archive of the theme is returned.", "application/zip");
    }
}
