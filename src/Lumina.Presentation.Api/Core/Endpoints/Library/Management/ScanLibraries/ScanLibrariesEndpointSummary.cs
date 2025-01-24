#region ========================================================================= USING =====================================================================================
using FastEndpoints;
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
        Description = "Starts the scanning process of all media libraries, if the user making the request is an Admin.";

        Response(204, "The media libraries scan was successfully started.");

        Response(401, "Authentication required.", "application/problem+json",
            example: new[]
            {
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "You are not authorized",
                    instance = "/api/v1/libraries/scan"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "Invalid token: The token expired at '01/01/2024 01:00:00'",
                    instance = "/api/v1/libraries/scan"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "The token is invalid",
                    instance = "/api/v1/libraries/scan"
                }
            }
        );

        Response(403, "The request failed because the user making the request is not an Admin.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc9110#section-15.5.4",
                title = "General.Failure",
                status = 403,
                detail = "NotAuthorized",
                instance = "/api/v1//libraries/scan",
                traceId = "00-a712bbf99ca8ab485f86a762ae5ae74d-b3a2eb78813b0a5d-00"
            }
        );
    }
}
