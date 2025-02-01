#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Library.Management.CancelLibraryScan;

/// <summary>
/// Class used for providing a textual description for the <see cref="CancelLibraryScanEndpoint"/> API endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class CancelLibraryScanEndpointSummary : Summary<CancelLibraryScanEndpoint, EmptyRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CancelLibraryScanEndpointSummary"/> class.
    /// </summary>
    public CancelLibraryScanEndpointSummary()
    {
        Summary = "Cancells the previously started scan of a media library.";
        Description = "Cancells the scanning process of a media library of a user, if the user making the request is an Admin or the library belongs to them.";

        Response(204, "The media library scan was successfully cancelled.");

        Response(401, "Authentication required.", "application/problem+json",
            example: new[]
            {
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "You are not authorized",
                    instance = "/api/v1/libraries/{id}/cancel-scan"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "Invalid token: The token expired at '01/01/2024 01:00:00'",
                    instance = "/api/v1/libraries/{id}/cancel-scan"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "The token is invalid",
                    instance = "/api/v1/libraries/{id}/cancel-scan"
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
                instance = "/api/v1/libraries/{id}/cancel-scan",
                errors = new Dictionary<string, string[]>
                {
                    {
                        "General.Validation", new[]
                        {
                            "LibraryIdCannotBeEmpty"
                        }
                    }
                },
                traceId = "00-2470be4248a2a5a0c6f70579975a6954-b9c3ba9544a03500-00"
            }
        );
    }
}
