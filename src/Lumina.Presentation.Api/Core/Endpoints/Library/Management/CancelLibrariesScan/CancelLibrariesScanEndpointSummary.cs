#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Responses.MediaLibrary.Management;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Library.Management.CancelLibrariesScan;

/// <summary>
/// Class used for providing a textual description for the <see cref="CancelLibrariesScanEndpoint"/> API endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class CancelLibrariesScanEndpointSummary : Summary<CancelLibrariesScanEndpoint, EmptyRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CancelLibrariesScanEndpointSummary"/> class.
    /// </summary>
    public CancelLibrariesScanEndpointSummary()
    {
        Summary = "Cancells the previously started scan of all media libraries.";
        Description = "Cancells the scanning process of all media libraries of a user, or all libraries, if the user making the request is an Admin.";

        Response(204, "The media libraries scan was successfully cancelled.");

        Response(401, "Authentication required.", "application/problem+json",
            example: new[]
            {
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "You are not authorized",
                    instance = "/api/v1/libraries/cancel-scan"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "Invalid token: The token expired at '01/01/2024 01:00:00'",
                    instance = "/api/v1/libraries/cancel-scan"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "The token is invalid",
                    instance = "/api/v1/libraries/cancel-scan"
                }
            }
        );
    }
}
