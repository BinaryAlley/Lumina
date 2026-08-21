#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Responses.Themes;
using Lumina.Domain.SharedKernel.Common.Enums.Themes;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Themes.Queries.GetThemes;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetThemesEndpoint"/> API endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetThemesEndpointSummary : Summary<GetThemesEndpoint, EmptyRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetThemesEndpointSummary"/> class.
    /// </summary>
    public GetThemesEndpointSummary()
    {
        Summary = "Retrieves the installed themes.";
        Description = "Retrieves the list of all installed themes.";

        Response(200, "The list of installed themes is returned.",
            example: new ThemeResponse[] {
            new (
                Id: Guid.NewGuid(),
                ThemeId: "lumina-default",
                Name: "Lumina Default",
                Description: "A clean, readable theme.",
                Author: "Lumina Team",
                Version: "1.2.0",
                PreviewPath: "preview.png",
                InstallSource: ThemeInstallSource.Bundled,
                IsCurrent: true,
                InstalledAtUtc: DateTime.UtcNow,
                IsDeleted: false
            ),
            new (
                Id: Guid.NewGuid(),
                ThemeId: "neon-grid",
                Name: "Neon Grid",
                Description: "A dark theme with a neon grid background.",
                Author: "Lumina Team",
                Version: "0.9.3",
                PreviewPath: "preview.jpg",
                InstallSource: ThemeInstallSource.Uploaded,
                IsCurrent: null,
                InstalledAtUtc: DateTime.UtcNow,
                IsDeleted: false
            )
        });

        Response(401, "Authentication required.", "application/problem+json",
            example: new[]
            {
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "You are not authorized",
                    instance = "/api/v1/themes"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "Invalid token: The token expired at '01/01/2024 01:00:00'",
                    instance = "/api/v1/themes"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "The token is invalid",
                    instance = "/api/v1/themes"
                }
            }
        );
    }
}
