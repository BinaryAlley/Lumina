#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Responses.Themes;
using Lumina.Domain.SharedKernel.Common.Enums.Themes;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Themes.Queries.GetCurrentTheme;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetCurrentThemeEndpoint"/> API endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetCurrentThemeEndpointSummary : Summary<GetCurrentThemeEndpoint, EmptyRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetCurrentThemeEndpointSummary"/> class.
    /// </summary>
    public GetCurrentThemeEndpointSummary()
    {
        Summary = "Retrieves the currently active theme.";
        Description = "Retrieves the metadata of the theme that is currently active.";

        Response(200, "The currently active theme is returned.",
            example: new ThemeResponse(
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
            ));
    }
}
