#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Responses.Themes;
using Lumina.Domain.SharedKernel.Common.Enums.Themes;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Themes.Management.InstallTheme;

/// <summary>
/// Class used for providing a textual description for the <see cref="InstallThemeEndpoint"/> API endpoint, for OpenAPI.
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

        Response(200, "The theme was successfully installed.",
            example: new ThemeResponse(
                Id: Guid.NewGuid(),
                ThemeId: "lumina-default",
                Name: "Lumina Default",
                Description: "A clean, readable theme.",
                Author: "Lumina Team",
                Version: "1.2.0",
                PreviewPath: "preview.png",
                InstallSource: ThemeInstallSource.Uploaded,
                IsCurrent: null,
                InstalledAtUtc: DateTime.UtcNow,
                IsDeleted: false
            ));

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

        Response(403, "The request failed because the user is not an Admin, or the uploaded theme pack is not readable.", "application/problem+json",
            example: new[]
            {
                new
                {
                    type = "https://tools.ietf.org/html/rfc9110#section-15.5.4",
                    title = "General.Failure",
                    status = 403,
                    detail = "NotAuthorized",
                    instance = "/api/v1/themes",
                    traceId = "00-a712bbf99ca8ab485f86a762ae5ae74d-b3a2eb78813b0a5d-00"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc9110#section-15.5.4",
                    title = "General.Failure",
                    status = 403,
                    detail = "ThemeArchiveNotReadable",
                    instance = "/api/v1/themes",
                    traceId = "00-a712bbf99ca8ab485f86a762ae5ae74d-b3a2eb78813b0a5d-00"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc9110#section-15.5.4",
                    title = "General.Failure",
                    status = 403,
                    detail = "ThemeFilesUnreadable",
                    instance = "/api/v1/themes",
                    traceId = "00-a712bbf99ca8ab485f86a762ae5ae74d-b3a2eb78813b0a5d-00"
                }
            }
        );

        Response(422, "The request did not pass validation checks.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc4918#section-11.2",
                title = "General.Validation",
                status = 422,
                detail = "OneOrMoreValidationErrorsOccurred",
                instance = "/api/v1/themes",
                errors = new Dictionary<string, string[]>
                {
                    {
                        "General.Validation", new[]
                        {
                            "ThemeArchiveCannotBeNull"
                        }
                    }
                },
                traceId = "00-2470be4248a2a5a0c6f70579975a6954-b9c3ba9544a03500-00"
            }
        );
    }
}
