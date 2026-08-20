#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.DTO.Common;
using Lumina.Presentation.Web.Common.Requests.Themes;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Admin.Themes.SetCurrentTheme;

/// <summary>
/// Class used for providing a textual description for the <see cref="SetCurrentThemeEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class SetCurrentThemeEndpointSummary : Summary<SetCurrentThemeEndpoint, SetCurrentThemeRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SetCurrentThemeEndpointSummary"/> class.
    /// </summary>
    public SetCurrentThemeEndpointSummary()
    {
        Summary = "Sets the currently active theme.";
        Description = "Sets the theme identified by the request as the currently active theme.";

        RequestParam(r => r.ThemeId, "The manifest id of the theme to activate.");

        Response(200, "The theme was successfully activated.", example: new SuccessResponse(true));
    }
}
