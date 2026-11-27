#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.DTO.Common;
using Lumina.Presentation.Web.Common.DTO.Themes;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Admin.Themes.GetThemes;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetThemesEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetThemesEndpointSummary : Summary<GetThemesEndpoint, EmptyRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetThemesEndpointSummary"/> class.
    /// </summary>
    public GetThemesEndpointSummary()
    {
        Summary = "Retrieves the theme administration data.";
        Description = "Retrieves the installed themes, together with the current theme and the theme engine settings.";

        Response(200, "The theme administration data is returned.", example: new SuccessResponse<ThemeAdminDto>(true, default));
    }
}
