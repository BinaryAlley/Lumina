#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Responses.Themes;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Themes.Queries.GetThemeSettings;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetThemeSettingsEndpoint"/> API endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetThemeSettingsEndpointSummary : Summary<GetThemeSettingsEndpoint, EmptyRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetThemeSettingsEndpointSummary"/> class.
    /// </summary>
    public GetThemeSettingsEndpointSummary()
    {
        Summary = "Retrieves the theme engine settings.";
        Description = "Retrieves the settings of the theme engine, including the maximum archive size.";

        Response(200, "The theme engine settings are returned.",
            example: new ThemeSettingsResponse(
                MaxArchiveBytes: 8388608,
                DefaultThemeId: "lumina-default"
            ));
    }
}
