#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.DTO.Common;
using Lumina.Presentation.Web.Common.Requests.Themes;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Admin.Themes.DeleteTheme;

/// <summary>
/// Class used for providing a textual description for the <see cref="DeleteThemeEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class DeleteThemeEndpointSummary : Summary<DeleteThemeEndpoint, DeleteThemeRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteThemeEndpointSummary"/> class.
    /// </summary>
    public DeleteThemeEndpointSummary()
    {
        Summary = "Deletes a theme.";
        Description = "Deletes the theme identified by the request, switching to another available theme when the deleted theme was the active one.";

        RequestParam(r => r.ThemeId, "The manifest id of the theme to delete.");

        Response(200, "The theme was successfully deleted.", example: new SuccessResponse(true));
    }
}
