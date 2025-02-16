#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Responses.MediaLibrary.Management;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Library.Management.ScanLibraries;

/// <summary>
/// Class used for providing a textual description for the <see cref="ScanLibrariesEndpoint"/> API endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class ScanLibrariesEndpointSummary : Summary<ScanLibrariesEndpoint, EmptyRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ScanLibrariesEndpointSummary"/> class.
    /// </summary>
    public ScanLibrariesEndpointSummary()
    {
        Summary = "Triggers the scan of all media libraries.";
        Description = "Starts the scanning process of all media libraries of a user, or all libraries, if the user is an Admin.";

        Response(200, "The media libraries scan was successfully started.",
            example: new ScanLibraryResponse[] {
            new (
                ScanId: Guid.NewGuid(),
                LibraryId: Guid.NewGuid()
            ),
            new (
                ScanId: Guid.NewGuid(),
                LibraryId: Guid.NewGuid()
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
                    instance = "/api/v1/libraries/scans"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "Invalid token: The token expired at '01/01/2024 01:00:00'",
                    instance = "/api/v1/libraries/scans"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "The token is invalid",
                    instance = "/api/v1/libraries/scans"
                }
            }
        );
    }
}
