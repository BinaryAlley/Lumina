#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.DTO.Common;
using Lumina.Presentation.Web.Common.DTO.Themes;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Admin.Themes.InstallTheme;

/// <summary>
/// Class used for providing a textual description for the <see cref="InstallThemeEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class InstallThemeEndpointSummary : Summary<InstallThemeEndpoint, EmptyRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InstallThemeEndpointSummary"/> class.
    /// </summary>
    public InstallThemeEndpointSummary()
    {
        Summary = "Installs a theme pack.";
        Description = "Installs the theme pack uploaded in the multipart form of the request, replacing the files of an existing theme with the same manifest id.";

        Response(200, "The theme was successfully installed.", example: new SuccessResponse<ThemeInfoDto>(true, default));
    }
}
